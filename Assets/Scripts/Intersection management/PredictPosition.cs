using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Intersection management method - predict future positions of the first car in the queue
//and see if it intersects with another car
public class PredictPosition : MonoBehaviour
{
  //Acceleration in [m/s^2] Ferrari has 7.6 so much lower than that
  public float acceleration; //7
                             //Drag force to get a lower acc when driving faster
  public float dragFactor;
  public float frontBuffer;
  public float sideBuffer;


  //The transform used to simulate the path - which is naive but easier in the beginning
  public Transform simulationTransform;

  //A list with all simulated positions of this car, added if we find a path
  List<Rectangle> simulatedCarPositions = new List<Rectangle>();


  public void UpdatePredictPosition(float time)
  {
    //We have an emergency vehicle and need to prioritize it
    if (IntersectionManager.emergencyVehicles.Count > 0)
    {
      //Prioritize vehicles with the same lane as the emergency vehicle
      int emergencyLane = IntersectionManager.emergencyVehicles[0].GetComponent<CarController>().startLane;

      //Also find the neighboring lane because that lane will never collide with the lane the emergency car is in
      //so we can send all those cars forward as well
      int lane1 = 0;
      int lane2 = 0;
      for (int j = 0; j < IntersectionManager.laneCombinations.Length; j++)
      {
        //Is one of these lanes the lane the emergency vehicle is in?
        if (IntersectionManager.laneCombinations[j, 0] == emergencyLane ||
            IntersectionManager.laneCombinations[j, 1] == emergencyLane)
        {
          lane1 = IntersectionManager.laneCombinations[j, 0];
          lane2 = IntersectionManager.laneCombinations[j, 1];

          break;
        }
      }

      //Loop through all cars and see if they can drive through the intersection
      for (int i = 0; i < IntersectionManager.carsWaitingForPath.Count; i++)
      {
        //Get the lane the car is in
        int lane = IntersectionManager.carsWaitingForPath[i].startLane;

        //Send the car forward if it's in the same lane as the emergency car or in the neighboring lane
        if ((lane == lane1 || lane == lane2) && CanCarFindPath(i))
        {
          break;
        }
      }
    }
    //Send forward the first car that can find a clear path or has waited for maximum time
    else
    {
      for (int i = 0; i < IntersectionManager.carsWaitingForPath.Count; i++)
      {
        //If the timer is above a certain limit, then prioritize the first car in the queue
        if (IntersectionManager.carsWaitingForPath[i].waitTime > 10f && i != 0)
        {
          //End the loop because we only want to check if the first car can find a path
          break;
        }

        if (CanCarFindPath(i))
        {
          //Only one car is allowed to get a path each update
          break;
        }
        //This means the car couldn't find a path, so maybe we need to add a waiting time to it
        else
        {
          Car thisCar = IntersectionManager.carsWaitingForPath[i];

          float distSqrToIntersection = (thisCar.carScript.currentWaypoint.pos - thisCar.carObj.transform.position).sqrMagnitude;

          //This car is close to the intersection and is not moving fast so it is "waiting"
          if (distSqrToIntersection < 20f * 20f && thisCar.carScript.GetSpeed() < 5f)
          {
            thisCar.waitTime += time;
          }
        }
      }
    }
  }



  //Test if one car from the list can find a path
  private bool CanCarFindPath(int listPos)
  {
    bool hasFoundPath = false;

    //Get the first car in the queue but don't remove it until we know it can find a path
    Car car = new Car(IntersectionManager.carsWaitingForPath[listPos].carObj);

    //This car is behind another car so we cant send it forward until the car infront of it is gone
    if (IsBehindAnotherCar(car))
    {
      return hasFoundPath;
    }

    //Get the distance from the car to the intersection
    float distanceSqrToIntersection = (car.carObj.transform.position - car.carScript.currentWaypoint.pos).sqrMagnitude;

    //Begin predicting the path if the car is a certain distance from the intersection
    //Use the same distance as when the car begins to slow down to intesection speed
    if (distanceSqrToIntersection < 10f * 10f)
    {
      //Check if the path is clear from this point
      hasFoundPath = IsPathClear(car);

      if (hasFoundPath)
      {
        //The car can move through the intersection without any problems!
        car.carScript.hasClearPath = true;

        //Is this an emergency vehicle, if so remove it from that list as well
        if (IntersectionManager.emergencyVehicles.Count > 0 && car.carObj == IntersectionManager.emergencyVehicles[0])
        {
          IntersectionManager.emergencyVehicles.RemoveAt(0);
        }

        //Remove the car from the queue
        IntersectionManager.carsWaitingForPath.RemoveAt(listPos);
      }
    }

    return hasFoundPath;
  }



  //Check if the path from the current position of the car until it leaves the intersection is clear
  private bool IsPathClear(Car car)
  {
    //Move the transform to the car's position
    Vector3 carPos = car.carObj.transform.position;

    //Make sure we are at 0 height
    carPos.y = 0f;

    simulationTransform.position = carPos;

    //The current wp the car is heading towards
    WaypointData currentWP = car.carScript.currentWaypoint;
    //We can't always get the previous wp from the current wp, so we need to save it as well
    Vector3 previousWPPos = car.carScript.previousWaypoint.pos;

    //Make sure the transform is looking at the wp it is moving towards
    simulationTransform.LookAt(currentWP.pos);

    //Integration parameters
    float timeStep = 0.02f;
    //Get the current position
    Vector3 pos = simulationTransform.position;
    //Get the current speed - but dont forget to convert from km/h to m/s
    Vector3 vVec = car.carScript.GetSpeed() * (1f / 3.6f) * simulationTransform.forward;

    //So we can identify the correct timestep in the arrays
    int layer = 0;
    //To avoid inifnite loops
    int iterations = 0;

    //A list with all simulated positions of this car, added if we find a path
    simulatedCarPositions.Clear();

    //Main loop to see if the path is clear
    bool isClear = false;

    while (!isClear)
    {
      //Are we outside of the intersection with the entire car and not just the center?
      //Also add a small buffer to be on the safe side
      //Cant make the distance calculation inside the if because then it will never move to the next else if
      //if the distance is not long enough
      if (currentWP.nextWp == null &&
          (simulationTransform.position - previousWPPos).sqrMagnitude > (car.halfLength + 4f) * (car.halfLength + 4f))
      {
        isClear = true;

        //If we have found a path, we need to add all car rectangles to the list with all future car positions in the intersection
        for (int i = 0; i < simulatedCarPositions.Count; i++)
        {
          IntersectionManager.allRectangles[i].Add(simulatedCarPositions[i]);
        }
      }
      //Have we reached the limit to avoid infinite loops. If each update is 0.02 seconds, 
      //then 500 means it took more than 10 seconds to not reach the end of the intersection
      else if (iterations > 1000)
      {
        //print("Failed to find a path to the end");

        isClear = false;

        break;
      }
      else
      {
        //Get the position and acceleration next time step with an integration method

        //Method 1: Forward Euler
        GetNextPosVelForwardEuler(pos, vVec, timeStep, car.maxSpeed, out pos, out vVec);

        //Method 2: Henun's Method
        //GetNextPosVelHeunsMethod(pos, vVec, timeStep, maxSpeed, out pos, out vVec);

        //Move the transform to the new position
        simulationTransform.position = pos;

        //Can we move to this position without colliding with another car?
        if (!IsColliding(layer, simulationTransform, car, simulatedCarPositions))
        {
          //Has the transform passed the waypoint
          float progress = Math.CalculateProgressBetweenWaypoints(simulationTransform.position, previousWPPos, currentWP.pos);

          //Change wp if we should
          if (progress >= 1f)
          {
            previousWPPos = currentWP.pos;

            currentWP = currentWP.nextWp;

            //Aim the transform against this new wp
            simulationTransform.LookAt(currentWP.pos);

            //print("Changed wp");
          }

          //Change array layer
          layer += 1;
        }
        //Collision so we cant find a path
        else
        {
          break;
        }
      }

      iterations += 1;
    }


    return isClear;
  }



  //Integration method Forward Euler
  private void GetNextPosVelForwardEuler(
      Vector3 pos,
      Vector3 vVec,
      float timeStep,
      float maxSpeed,
      out Vector3 posNext,
      out Vector3 vVecNext)
  {
    float v = Mathf.Clamp(vVec.magnitude, 0f, maxSpeed);

    //Forward Euler
    posNext = pos + timeStep * v * simulationTransform.forward;

    float aDrag = v * v * dragFactor;

    //Accelerate if we are driving slower than the maxSpeed
    float a = v < maxSpeed ? acceleration : 0f;

    vVecNext = vVec + timeStep * (a - aDrag) * simulationTransform.forward;
  }



  //Integration method Heun's method
  private void GetNextPosVelHeunsMethod(
      Vector3 pos,
      Vector3 vVec,
      float timeStep,
      float maxSpeed,
      out Vector3 posNext,
      out Vector3 vVecNext)
  {
    //Forward Euler
    Vector3 posEuler = Vector3.zero;
    Vector3 velEuler = Vector3.zero;

    GetNextPosVelForwardEuler(pos, vVec, timeStep, maxSpeed, out posEuler, out velEuler);


    //Heun's method, which is using Forward Euler, but is more accurate
    //The method uses the average of the acceleration this time step and the next step
    float v = vVec.magnitude;
    float vEuler = velEuler.magnitude;

    float a = v < maxSpeed ? acceleration : 0f;
    float aEuler = vEuler < maxSpeed ? acceleration : 0f;

    float aDrag = v * v * dragFactor;
    float aDragEuler = vEuler * vEuler * dragFactor;

    posNext = pos + timeStep * ((v + vEuler) * 0.5f) * simulationTransform.forward;

    vVecNext = vVec + timeStep * (((a - aDrag) + (aEuler - aDragEuler)) * 0.5f) * simulationTransform.forward;
  }



  //Check for collisions at this time step
  private bool IsColliding(int layer, Transform simTrans, Car car, List<Rectangle> carRects)
  {
    bool isColliding = false;

    //Create the rectangle
    Rectangle carRect = CreateRectangle(simTrans, car);

    //Add the rectangle to the list we are going to add if the path through the intersection is clear
    carRects.Add(carRect);

    //Have we reached this layer before?
    if (IntersectionManager.allRectangles.Count <= layer)
    {
      //We haven't reached this level before, so we need to add a new array 
      List<Rectangle> newList = new List<Rectangle>();

      //Add the list to the queue
      IntersectionManager.allRectangles.Add(newList);

      //Know we know we are not colliding because only rectangle at this time step
      return isColliding;
    }
    //Check for collision with all rectangles in this layer
    else
    {
      List<Rectangle> allRectangles = IntersectionManager.allRectangles[layer];

      //Loop through all rectangles and check for intersection
      for (int i = 0; i < allRectangles.Count; i++)
      {
        Rectangle thisRect = allRectangles[i];

        if (Intersections.IsIntersectingOBBRectangleRectangle(
            thisRect.FL, thisRect.FR, thisRect.BL, thisRect.BR,
            carRect.FL, carRect.FR, carRect.BL, carRect.BR))
        {
          isColliding = true;

          break;
        }
      }
    }

    return isColliding;
  }



  //Create a rectangle
  private Rectangle CreateRectangle(Transform simTrans, Car car)
  {
    //Get the rectangle's corners
    //Can make the car a little bigger to be on the safe side if detecting collisions
    //This will also make the car transit out from the grid, which is not possible with the line method
    //1.0 is no buffer, increase to increase the buffer
    Vector3 carBufferForward = car.halfLength * frontBuffer * simTrans.forward;
    Vector3 carBufferSide = car.halfWidth * sideBuffer * simTrans.right;

    Vector3 forwardPos = simTrans.position + carBufferForward;

    Vector3 FL = forwardPos - carBufferSide;
    Vector3 FR = forwardPos + carBufferSide;

    Vector3 backPos = simTrans.position - carBufferForward;

    Vector3 BL = backPos - carBufferSide;
    Vector3 BR = backPos + carBufferSide;


    //Create the rectangle
    Rectangle newRect = new Rectangle(FL, FR, BL, BR);

    return newRect;
  }



  //Check if a car is behind another car by raycasting
  //For some reason, it doesnt always work to do this by using the carscript
  private bool IsBehindAnotherCar(Car car)
  {
    bool isBehindAnotherCar = false;

    RaycastHit hit;

    //Fire the ray from the front of the car, 0.3 meters up
    Transform carTrans = car.carObj.transform;

    Vector3 origin = carTrans.position + carTrans.up * 0.3f + carTrans.forward * car.carScript.carLength * 0.5f;

    if (Physics.Raycast(origin, carTrans.forward, out hit, 100f))
    {
      //Debug.DrawRay(origin, transform.forward * 10f, Color.white);

      Transform objectThatWasHit = hit.collider.transform.parent;

      //If we have hit another car
      if (objectThatWasHit != null && objectThatWasHit.CompareTag("Car"))
      {
        //If the car we hit with the ray is waiting for a path
        if (!objectThatWasHit.GetComponent<CarController>().hasClearPath)
        {
          isBehindAnotherCar = true;
        }
      }
    }

    return isBehindAnotherCar;
  }
}
