using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
  //Drags
  //Add the wheels to here and if they can steer and have an engine attached to them
  public List<AxleInfo> axleInfos;
  //The collider so we can get the size of the car
  public BoxCollider carCollider;

  //The waypoints the car is following
  public WaypointData currentWaypoint;
  public WaypointData previousWaypoint;

  //Car models so we can change its color
  public enum CarModels { ModelS, ModelX, Roadster }

  public CarModels carModels;


  //Is this car controlled manually by the player with keyboard?
  private bool isManuallyControlled = false;

  //To average the steering angles to simulate the time it takes to turn the wheel
  private float averageSteeringAngle = 0f;

  //Speed of the car
  private float currentSpeed = 0f;
  //To get the speed we need to save the car's last position each update
  private Vector3 lastPosition;

  //Car data
  private float maxMotorTorque = 500f; //200f
  private float maxSteeringAngle = 35f; //Smaller than max to get a better behavior
  private float maxBrakeTorque = 1000f;
  private float maxSpeed = 50f; //[km/h]
                                //Max speed through the intersection
  public float maxIntersectionSpeed = 20f; //[km/h]
                                           //Where are the front wheels in relation to the center of the car so the car follows paths more accurate
                                           //Just check where z of front wheels are because they are positioned in relation to the center
  private float distToFrontAxle;
  //Width and length
  [System.NonSerialized]
  public float carLength;
  [System.NonSerialized]
  public float carWidth;
  //Distance between the wheels (= wheelbase), which is needed to simulate the future pos of the car
  //Will be calculated automatically by measuring the distance to the front and rear wheel from the center of the car
  private float wheelBase;

  //PID
  PIDController PIDScript;

  //Car modes
  enum CarModes { Stop, Drive }

  CarModes carMode;

  //Has the car a clear path through the intersection?
  public bool hasClearPath;

  //In which lane did this car start?
  [System.NonSerialized]
  public int startLane;

  //The car's id
  [System.NonSerialized]
  public int carID;

  //Is the car close to another car infront of it
  [System.NonSerialized]
  public bool isCloseToAnotherCar;

  //Spawn time
  public float spawnTime;


  public void Start()
  {
    //Calculate the data we need, such as length of the car
    InitCarData();

    PIDScript = GetComponent<PIDController>();

    //Set the mode to drive
    carMode = CarModes.Drive;

    //Make sure the car cant drive through the intersection
    hasClearPath = false;
  }



  public void Update()
  {
    //Everything that has to do with waypoints
    UpdateWaypoints();
  }



  public void FixedUpdate()
  {
    //Calculate the car's speed
    CalculateSpeed();

    //Calculate the car's data, such as steering angle, motor torque - and add it to the car
    CalculateCarData();
  }



  //Calculate the data we need, such as length of the car
  private void InitCarData()
  {
    //Calculate wheelbase = distance between the wheels
    //Where is the front axle in relation to center?
    foreach (AxleInfo axleInfo in axleInfos)
    {
      if (axleInfo.steering)
      {
        distToFrontAxle = axleInfo.leftWheel.transform.localPosition.z;

        //print(distToFrontAxle);
        wheelBase += distToFrontAxle;
      }
      else
      {
        //The distance to the rear axle to get wheelbase
        wheelBase += Mathf.Abs(axleInfo.leftWheel.transform.localPosition.z);
      }
    }

    //print(wheelBase);

    //Get the length and witdh of the car by using the size of the collider
    carLength = carCollider.size.z;
    carWidth = carCollider.size.x;
  }



  //Calculate the car's data, such as steering angle, motor torque - and add it to the car
  private void CalculateCarData()
  {
    float motorTorque = 0f;
    float steeringAngle = 0f;
    float brakeTorque = 0f;

    //Manual controls
    if (isManuallyControlled)
    {
      motorTorque = maxMotorTorque * Input.GetAxis("Vertical");

      //Should be limited because of speed but not needed for testing purposes
      steeringAngle = maxSteeringAngle * Input.GetAxis("Horizontal");
    }
    //Automatic controls
    else
    {
      //Check if we need to change car mode
      ChangeCarMode();

      if (carMode == CarModes.Drive)
      {
        motorTorque = CalculateMotorTorque();

        steeringAngle = CalculateSteeringAngle();

        brakeTorque = 0f;
      }
      else
      {
        //Motor torque and steering angle are already set to 0

        brakeTorque = maxBrakeTorque;
      }
    }

    //Add all the data to the car
    AddDataToCar(steeringAngle, motorTorque, brakeTorque);
  }



  //Calculate the car's steering angle
  private float CalculateSteeringAngle()
  {
    //Where is the car's steering axle
    Vector3 steerPos = transform.position + transform.forward * distToFrontAxle * 1.0f;

    //Get the distance between where the car is and where it should be
    float CTE = Math.GetCrossTrackError(steerPos, previousWaypoint.pos, currentWaypoint.pos);

    //Is CTE negative or positive?
    CTE *= Math.DirectionToReachTarget(transform, steerPos, currentWaypoint.pos);

    //Get the steering angle from the PID controller
    float wantedSteeringAngle = PIDScript.GetSteerFactorFromPIDController(CTE);
    
    //Limit the steering angle because the angle from the PID controller can be infinite
    //Get the angle between the car's forward vector and the vector between the car and the waypoint we are steering towards
    //This angle is always between 0 and 180 degrees
    float angleBetween = Vector3.Angle(transform.forward, (currentWaypoint.pos - transform.position).normalized);

    //This is to limit large steering angles when we are heading towards the waypoint which will produce a drunk behavior
    if (angleBetween < 20f)
    {
      wantedSteeringAngle = Mathf.Clamp(wantedSteeringAngle, -angleBetween, angleBetween);
    }
    //Want to steer as much as possible towards the waypoint
    else
    {
      wantedSteeringAngle = Mathf.Clamp(wantedSteeringAngle, -maxSteeringAngle, maxSteeringAngle);
    }


    //We also need to limit the steering angle when the car is moving fast
    float allowedSteeringAngle = LimitSteeringAngle(Mathf.Abs(wantedSteeringAngle), currentSpeed);

    //So we can steer left or right, which we removed when we calculated whats above
    if (wantedSteeringAngle < 0f)
    {
      allowedSteeringAngle *= -1f;
    }


    //Average the steering angles - it takes some time to turn the wheel, so realistic
    //but difficult to simulate when we predict future pos? But we need it or the car
    //will stop because the wheels are fluctuating to much
    //10 is min to avoid drunk behavior in curve
    averageSteeringAngle += (allowedSteeringAngle - averageSteeringAngle) / 10f;

    float steeringAngle = averageSteeringAngle;


    return steeringAngle;
  }



  //Calculate the car's motortorque
  private float CalculateMotorTorque()
  {
    float motorTorque = maxMotorTorque;


    if (currentSpeed > maxSpeed)
    {
      motorTorque = 0f;
    }


    //Slow down in the intersection
    if (currentWaypoint.isCurve && currentSpeed > maxIntersectionSpeed)
    {
      motorTorque = 0f;
    }

    //Slow down before we come to the intersection and if the car can move through the intersection
    float distToWp = (currentWaypoint.pos - transform.position).magnitude;
    
    if (currentWaypoint.isStop && distToWp < 10f && currentSpeed > maxIntersectionSpeed && hasClearPath)
    {
      motorTorque = 0f;
    }
    //If the car cant move through the intersection, then it should move slowly towards the stop waypoint
    else if (currentWaypoint.isStop && distToWp < 10f && currentSpeed > 2f && !hasClearPath)
    {
      motorTorque *= 0.2f;
    }

    return motorTorque;
  }



  //Check if we need to change car mode from drive to brake or vice versa
  private void ChangeCarMode()
  {
    //Init to drive and then check if we shouldn't drive
    carMode = CarModes.Drive;


    //Are we closed to the intersection and dont have a clear path?
    if (hasClearPath == false)
    {
      //Are we close to the intersection?
      if (currentWaypoint.isStop && (currentWaypoint.pos - transform.position).magnitude < carLength * 0.5f + 0.5f)
      {
        carMode = CarModes.Stop;
      }
    }


    //Are we close to another car infront of us and are not in the intersection?
    isCloseToAnotherCar = false;
    if (currentWaypoint.isStop || currentWaypoint.nextWp == null)
    {
      RaycastHit hit;

      //Fire the ray from the front of the car, 0.3 meters up
      Vector3 origin = transform.position + transform.up * 0.3f + transform.forward * carLength * 0.5f;

      if (Physics.Raycast(origin, transform.forward, out hit, 3f))
      {
        //Debug.DrawRay(origin, transform.forward * 10f, Color.white);

        Transform objectThatWasHit = hit.collider.transform.parent;

        //If we have hit another car
        if (objectThatWasHit != null && objectThatWasHit.CompareTag("Car"))
        {
          //If the car we hit with the ray is in the same lane
          //int otherCarsLaneNumber = objectThatWasHit.GetComponent<CarController>().startLane;

          carMode = CarModes.Stop;

          isCloseToAnotherCar = true;
        }
      }
    }
  }



  //Add the data to the car
  private void AddDataToCar(float steeringAngle, float motorTorque, float brakeTorque)
  {
    foreach (AxleInfo axleInfo in axleInfos)
    {
      if (axleInfo.steering)
      {
        axleInfo.leftWheel.steerAngle = steeringAngle;
        axleInfo.rightWheel.steerAngle = steeringAngle;
      }

      if (axleInfo.motor)
      {
        axleInfo.leftWheel.motorTorque = motorTorque;
        axleInfo.rightWheel.motorTorque = motorTorque;
      }

      axleInfo.leftWheel.brakeTorque = brakeTorque;
      axleInfo.rightWheel.brakeTorque = brakeTorque;

      //Update the wheel meshes so they rotate
      ApplyLocalPositionToVisuals(axleInfo.leftWheel);
      ApplyLocalPositionToVisuals(axleInfo.rightWheel);
    }
  }



  //Find the wheel mesh and apply rotation to it
  private void ApplyLocalPositionToVisuals(WheelCollider collider)
  {
    //No wheel mesh can be found
    if (collider.transform.childCount == 0)
    {
      return;
    }

    //Get the wheel mesh
    Transform visualWheel = collider.transform.GetChild(0);

    //Get the rotation and position this wheel mesh should have
    Vector3 position;
    Quaternion rotation;
    collider.GetWorldPose(out position, out rotation);

    //Add the rotation and position to the wheel mesh
    visualWheel.transform.position = position;
    visualWheel.transform.rotation = rotation;
  }



  //Calculate speed
  private void CalculateSpeed()
  {
    //Calculate the distance of the object between the fixedupdate to get [m/fixedupdate]
    float distance = (transform.position - lastPosition).magnitude;
    //Divide this value by Time.deltaTime to get [m/s], and multiply it by 3.6 to get [km/h]
    currentSpeed = (distance / Time.fixedDeltaTime) * 3.6f;

    //print("Speed:" + currentSpeed + "km/h");

    lastPosition = transform.position;
  }



  //Need to limit the steering angle so the car is not turning with 40 degrees at full speed
  private float LimitSteeringAngle(float maxSteeringAngle, float speed)
  {
    float maxSpeed = 200f;

    Vector2 min = new Vector2(0f, maxSteeringAngle);

    Vector2 max = new Vector2(maxSpeed, 0f);

    float t = speed / maxSpeed;

    //Assume linear relationship between speed and steering angle
    float limitedSteeringangle = Vector2.Lerp(min, max, t).y;

    return limitedSteeringangle;
  }



  //Everything that has to do with waypoints
  private void UpdateWaypoints()
  {
    //Where is the car's steering axle
    Vector3 steerPos = transform.position + transform.forward * distToFrontAxle * 1.0f;

    //Should we change waypoint?
    //How far have we travelled between the waypoints
    float progress = Math.CalculateProgressBetweenWaypoints(steerPos, previousWaypoint.pos, currentWaypoint.pos);

    //Change wp if progress > 1
    if (progress > 1f)
    {
      //Have we reached the last wp?
      if (currentWaypoint.nextWp == null)
      {
        //Remove the car
        IntersectionManager.RemoveCar(gameObject);

        Destroy(gameObject);
      }
      else
      {
        //But dont change waypoint if we should stop at this waypoint
        if (hasClearPath)
        {
          previousWaypoint = currentWaypoint;

          currentWaypoint = currentWaypoint.nextWp;
        }
      }
    }
  }



  //Change color of the car's body paint
  public void SetColor(Material carMaterial)
  {
    if (carModels == CarModels.ModelS)
    {
      gameObject.GetComponent<SetColorModelS>().SetColor(carMaterial);
    }
    else if (carModels == CarModels.ModelX)
    {
      gameObject.GetComponent<SetColorModelX>().SetColor(carMaterial);
    }
    if (carModels == CarModels.Roadster)
    {
      gameObject.GetComponent<SetColorRoadster>().SetColor(carMaterial);
    }
  }



  //Get methods
  public float GetSpeed()
  {
    return currentSpeed;
  }
}



[System.Serializable]
public class AxleInfo
{
  public WheelCollider leftWheel;
  public WheelCollider rightWheel;
  public bool motor;
  public bool steering;
}
