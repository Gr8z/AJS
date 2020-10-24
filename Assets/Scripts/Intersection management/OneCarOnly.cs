using UnityEngine;
using System.Collections;

//Intersection management method - allow one car only in the intersection 
public class OneCarOnly : MonoBehaviour
{
  //One vehicle only in the intersection
  bool isCarInIntersection = false;
  //The vehicle in the  intersection
  GameObject intersectionCar;



  //Allow one car in the intersection at the time - First-in-first-out
  public void UpdateOneCarOnly()
  {
    //Send a vehicle forward if the intersection is empty
    if (!isCarInIntersection)
    {
      intersectionCar = IntersectionManager.carsWaitingForPath[0].carObj;

      //Remove the car from the list of waiting cars
      IntersectionManager.carsWaitingForPath.RemoveAt(0);

      intersectionCar.GetComponent<CarController>().hasClearPath = true;

      isCarInIntersection = true;
    }


    //Check if the car currently in the intersection has left the intersection
    if (intersectionCar != null)
    {
      //Is the car heading towards the last wp? If so it has left the intersection
      if (intersectionCar.GetComponent<CarController>().currentWaypoint.nextWp == null)
      {
        isCarInIntersection = false;
      }
    }
  }
}
