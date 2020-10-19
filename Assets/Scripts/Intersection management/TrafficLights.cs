using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Intersection management method - traffic lights
public class TrafficLights : MonoBehaviour
{
  //The traffic lights we can change to green / red
  public GameObject[] trafficLights;
  //The color of the green lights
  public Color greenColor;

  //The current traffic lights in the array above that are green
  int greenTrafficLights = 0;
  //Timers
  float trafficLightTimer = 0f;
  float clearIntersectionTimer = 0f;
  //Bools
  bool hasStopedCars = false;



  public void InitTrafficLights()
  {
    ChangeTrafficLightColor(greenColor);
  }



  public void UpdateTrafficLights()
  {
    trafficLightTimer += Time.deltaTime;

    //Should maybe start cars here as well in case a car arrived late???

    //Change traffic light
    if (trafficLightTimer > 15f)
    {
      //Stop all cars that haven't reached the intersection if we haven't done so already
      if (!hasStopedCars)
      {
        hasStopedCars = true;

        //Stop all cars in the current lanes
        StartStopCars(false);

        //Set color to yellow
        ChangeTrafficLightColor(Color.yellow);
      }

      //But we also need to wait some time so all cars can clear the intersection
      clearIntersectionTimer += Time.deltaTime;

      if (clearIntersectionTimer > 5f)
      {
        //Reset
        trafficLightTimer = 0f;

        clearIntersectionTimer = 0f;

        hasStopedCars = false;

        //Set color to red
        ChangeTrafficLightColor(Color.red);


        //Set the next traffic lights in the intersection to green
        greenTrafficLights += 1;

        //Are we at the end of the array?
        if (greenTrafficLights > 3)
        {
          greenTrafficLights = 0;
        }

        //Start all cars in the current lanes
        StartStopCars(true);

        //Set color to green
        ChangeTrafficLightColor(greenColor);
      }
    }
  }



  //Stop or start all cars that are affected by the traffic lights
  void StartStopCars(bool shouldStart)
  {
    List<Car> activeCars = IntersectionManager.carsWaitingForPath;

    for (int i = 0; i < activeCars.Count; i++)
    {
      CarController carScript = activeCars[i].carScript;

      //Is the car in the correct lane?
      if (carScript.startLane == IntersectionManager.laneCombinations[greenTrafficLights, 0] ||
          carScript.startLane == IntersectionManager.laneCombinations[greenTrafficLights, 1])
      {
        //Only cars that are heading towards a traffic light are affected
        if (carScript.currentWaypoint.isStop)
        {
          //Stop the car
          if (!shouldStart)
          {
            Transform carTrans = activeCars[i].carObj.transform;

            //Get the front pos of the car
            Vector3 frontPos = carTrans.position + carTrans.forward * carScript.carLength * 0.5f;

            //The distance between the front of the car and the traffic light
            float distance = (frontPos - carScript.currentWaypoint.pos).magnitude;

            //Stop if the front of the car is more than a certain distance from the traffic light
            if (distance > 3f)
            {
              carScript.hasClearPath = shouldStart;
            }
          }
          //Start the car
          else
          {
            carScript.hasClearPath = shouldStart;
          }
        }
      }
    }
  }



  //Change color of traffic lights
  void ChangeTrafficLightColor(Color lightColor)
  {
    //Loop through all traffic lights
    for (int i = 0; i < trafficLights.Length; i++)
    {
      Waypoint wpScript = trafficLights[i].GetComponent<Waypoint>();

      //Is the traffic light in the correct lane?
      if (wpScript.laneNumber == IntersectionManager.laneCombinations[greenTrafficLights, 0] ||
          wpScript.laneNumber == IntersectionManager.laneCombinations[greenTrafficLights, 1])
      {
        trafficLights[i].GetComponent<Renderer>().material.color = lightColor;
      }
    }
  }
}
