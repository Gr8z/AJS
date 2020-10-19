using UnityEngine;
using System.Collections;

public struct Car
{
  //The car object that can drive
  public GameObject carObj;
  //The script that controls the car, to avoid a lot of GetComponent 
  public CarController carScript;
  //The car's data we need
  public float halfWidth;
  public float halfLength;
  //Maximum speed in m/s
  public float maxSpeed;
  //How long has this car waited at the intersection?
  public float waitTime;
  //In which lane did this car start?
  public int startLane;


  public Car(GameObject carObj)
  {
    this.carObj = carObj;
    this.carScript = carObj.GetComponent<CarController>();
    //Need half of the values to get the corner positions
    this.halfWidth = carScript.carWidth * 0.5f;
    this.halfLength = carScript.carLength * 0.5f;
    //Calculate the max speed from km/h to m/s
    this.maxSpeed = carScript.maxIntersectionSpeed * (1f / 3.6f);

    this.waitTime = 0f;

    this.startLane = carScript.startLane;
  }
}
