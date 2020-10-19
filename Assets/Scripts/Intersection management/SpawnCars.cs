using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SpawnCars : MonoBehaviour
{
  //Drags
  public GameObject modelSObj;
  public GameObject modelXObj;
  public GameObject roadsterObj;
  public GameObject policeObj;
  //Materials
  public Material[] carMaterials;
  //How often will we spawn a  new car
  public float timeUnitilNewCar;

  //Parent
  public Transform carParent;


  //To control how often the cars will spawn
  private float spawnTimer;

  //Store all car prefabs of all models in this list, to easier get a random car
  private List<GameObject> allCarsList = new List<GameObject>();

  //A list with spawn point positions in the array to get a random position
  int[] spawnPointsPosInArray;

  //Should we spawn a police car
  public static bool shouldSpawnPolice = false;


  void Start()
  {
    //Combine all cars into one list
    allCarsList.Add(modelSObj);
    allCarsList.Add(modelXObj);
    allCarsList.Add(roadsterObj);


    //Create the spawn point array (8 possible paths)
    spawnPointsPosInArray = new int[8];

    //Fill the array from 0 to 15
    for (int i = 0; i < spawnPointsPosInArray.Length; i++)
    {
      spawnPointsPosInArray[i] = i;
    }
  }



  void Update()
  {
    spawnTimer += Time.deltaTime;

    if (spawnTimer > timeUnitilNewCar && IntersectionManager.numberOfActiveCars < 50)
    {
      spawnTimer = 0f;

      SpawnCar();
    }

    //print(IntersectionManager.allCars.Count);
  }



  private void SpawnCar()
  {
    //Get a random car
    GameObject randomCar = null;

    if (!shouldSpawnPolice)
    {
      randomCar = allCarsList[Random.Range(0, allCarsList.Count)];
    }
    else
    {
      randomCar = policeObj;
    }

    //Get a random spawnpoint where there's no car
    WaypointData spawnPoint = GetRandomSpawnPoint();

    //If a car can spawn
    if (spawnPoint != null)
    {
      //Instantiate the new car
      GameObject newCar = Instantiate(randomCar, spawnPoint.pos, Quaternion.identity, carParent) as GameObject;

      //Make sure the car is active
      newCar.SetActive(true);

      //Rotate the car in the correct direction
      newCar.transform.LookAt(spawnPoint.nextWp.pos);

      //Give it waypoints
      CarController carScript = newCar.GetComponent<CarController>();

      carScript.currentWaypoint = spawnPoint.nextWp;
      carScript.previousWaypoint = spawnPoint;

      //Give the car a lane number so we know on which lane it is
      carScript.startLane = spawnPoint.laneNumber;

      //Give the car a unique id
      carScript.carID = IntersectionManager.GetCarID();

      //Give it a spawn time
      carScript.spawnTime = Time.realtimeSinceStartup;

      //Add it to all lists we need to add it to
      //Add the car to the list of cars active in the simulation
      //IntersectionManager.allCars.Add(newCar);
      IntersectionManager.numberOfActiveCars += 1;
      //Add the car to the list of cars waiting for a path
      IntersectionManager.carsWaitingForPath.Add(new Car(newCar));
      //If it's an emergency vehicle, also add it to another list
      if (shouldSpawnPolice)
      {
        IntersectionManager.emergencyVehicles.Add(newCar);
      }

      //Give the car a random color
      if (!shouldSpawnPolice)
      {
        Material randomMat = carMaterials[Random.Range(0, carMaterials.Length)];

        carScript.SetColor(randomMat);
      }

      //Doesnt matter if we havent spawned a police, so always set it to false
      shouldSpawnPolice = false;
    }
  }



  //Get a random spawnpoint where there's no car
  private WaypointData GetRandomSpawnPoint()
  {
    //Shuffle the array with random spawn points positions
    ShuffleArray();

    //Loop through all spawn points randomly (because of the shuffle these are now random so find first possible) 
    //and see if a spawn point is available (no cars in the area)
    for (int i = 0; i < spawnPointsPosInArray.Length; i++)
    {
      WaypointData currentSpawnPoint = InitWaypoints.finalPaths[spawnPointsPosInArray[i]];

      //Check if a car that has started at this spawnpoint is close to it
      if (IsACarCloseToSpawnPoint(currentSpawnPoint))
      {
        continue;
      }
      else
      {
        return currentSpawnPoint;
      }
    }

    return null;
  }



  //Shuffle the array with random spawn points positions
  private void ShuffleArray()
  {
    for (int i = 0; i < spawnPointsPosInArray.Length - 1; i++)
    {
      int tmp = spawnPointsPosInArray[i];

      //Move the current value to a random position in the array
      int randomIndex = Random.Range(0, spawnPointsPosInArray.Length);

      //Swap the values
      spawnPointsPosInArray[i] = spawnPointsPosInArray[randomIndex];

      spawnPointsPosInArray[randomIndex] = tmp;
    }

    //Test that the shuffle is working
    //string numbers = "";
    //for (int i = 0; i < spawnPointsPosInArray.Length; i++)
    //{
    //    numbers += spawnPointsPosInArray[i] + " ";
    //}

    //print(numbers);
  }



  //Check if a car that has started at this spawnpoint is close to it
  private bool IsACarCloseToSpawnPoint(WaypointData currentSpawnPoint)
  {
    bool isClose = false;

    //Begin by using naive method and see if any car is close by checking a radius
    //Max car length is 5 m, but we need a little more because we are using the hypotenuse
    float minDistSqr = 8f * 8f;

    //We only need to look through the list with cars waiting for a path
    for (int i = 0; i < IntersectionManager.carsWaitingForPath.Count; i++)
    {
      Car thisCar = IntersectionManager.carsWaitingForPath[i];

      //The distance sqr between the spawn point and the current car
      float distSqr = (thisCar.carObj.transform.position - currentSpawnPoint.pos).sqrMagnitude;

      //This car is close to the spawn point
      if (distSqr < minDistSqr)
      {
        //But is the car in the same lane?
        int carLaneNumber = thisCar.startLane;

        //A car that is close is in the same lane so we can't spawn a new car here
        if (currentSpawnPoint.laneNumber == carLaneNumber)
        {
          isClose = true;

          break;
        }
      }
    }

    return isClose;
  }


  //Spawn police
  public void SpawnPolice()
  {
    shouldSpawnPolice = true;
  }
}
