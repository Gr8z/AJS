using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntersectionManager : MonoBehaviour
{
  public Transform debugSphere;

  //A list with all cars in the simulation
  //Needed so we can find close cars when we spawn cars
  //public static List<GameObject> allCars = new List<GameObject>();
  //We dont need a list, just a number to keep track of how many cars we have 
  public static int numberOfActiveCars = 0;

  //A list with cars waiting for a clear path - can't use a queue because sometimes we have to 
  //prioritize other cars when an emergency vehicle arrives
  public static List<Car> carsWaitingForPath = new List<Car>();

  //A list with arrays of all cars (rectangles) in the intersection at that time
  //Cant use queue because we need to access all layers in the queue and not just the first
  public static List<List<Rectangle>> allRectangles = new List<List<Rectangle>>();

  //A list with emergency vehicles
  public static List<GameObject> emergencyVehicles = new List<GameObject>();

  //Lane numbers, so we can find which lane is next to another lane
  public static int[,] laneCombinations = { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };

  //Intersection management methods
  OneCarOnly oneCarOnlyMethod;
  TrafficLights trafficLightsMethod;
  PredictPosition predictPositionMethod;

  //To give each car a unique id
  public static int carID = 0;

  //Debug
  List<GameObject> debugRectangles = new List<GameObject>();

  float timer = 0f;


  //To get average time through the intersection
  private static int numberOfCarsThatHasFinished = 0;
  public static float averageTime = 0f;


  void Start()
  {
    oneCarOnlyMethod = GetComponent<OneCarOnly>();

    trafficLightsMethod = GetComponent<TrafficLights>();

    predictPositionMethod = GetComponent<PredictPosition>();


  }



  void Update()
  {
    timer += Time.deltaTime;

    if (carsWaitingForPath.Count > 0)
    {
      if (timer > 0.1f)
      {
        //Check if the first car in the queue can find a no-collision-path through the intersection
        predictPositionMethod.UpdatePredictPosition(timer);

        timer = 0f;
      }
    }
  }



  void FixedUpdate()
  {
    //Remove the latest time step where we store the rectangles for collision
    if (allRectangles.Count > 0)
    {
      allRectangles.RemoveAt(0);
    }
  }



  //Remove a car from the list of all cars - used when a car has finished the path
  public static void RemoveCar(GameObject objToRemove)
  {
    //The time it took for the car in seconds
    float timeThroughIntersection = Time.realtimeSinceStartup - objToRemove.GetComponent<CarController>().spawnTime;

    //Update average time
    numberOfCarsThatHasFinished += 1;

    //http://math.stackexchange.com/questions/22348/how-to-add-and-subtract-values-from-an-average
    averageTime = averageTime + ((timeThroughIntersection - averageTime) / numberOfCarsThatHasFinished);

    //allCars.Remove(objToRemove);
    IntersectionManager.numberOfActiveCars -= 1;
  }



  //Get a unique car id
  public static int GetCarID()
  {
    carID += 1;

    return carID;
  }



  //Display all rectangles this time step
  void DisplayRectangles()
  {
    //Remove all old
    if (debugRectangles.Count > 0)
    {
      for (int i = 0; i < debugRectangles.Count; i++)
      {
        Destroy(debugRectangles[i]);
      }
    }

    if (allRectangles.Count > 1)
    {
      List<Rectangle> rectanglesToDisplay = allRectangles[1];

      for (int i = 0; i < rectanglesToDisplay.Count; i++)
      {
        //Display
        DisplayOneRectangle(rectanglesToDisplay[i]);
      }
    }
  }



  //Display one rectangle with a mesh
  void DisplayOneRectangle(Rectangle rect)
  {
    //Create a new game object
    GameObject newRect = new GameObject();

    //Add a mesh renderer and mesh filter to it
    newRect.AddComponent<MeshFilter>();
    newRect.AddComponent<MeshRenderer>();

    //The vertices
    Vector3[] newVertices = new Vector3[4];

    //Add the vertices
    Vector3 height = new Vector3(0f, 0.1f, 0f);

    newVertices[0] = rect.FL + height;
    newVertices[1] = rect.FR + height;
    newVertices[2] = rect.BL + height;
    newVertices[3] = rect.BR + height;

    //The triangles (6 because we need a bottom so we can move the corners the way we want)
    int[] newTriangles = new int[6];

    //Clockwise
    newTriangles[0] = 0; //FL
    newTriangles[1] = 3; //BR
    newTriangles[2] = 2; //BL

    newTriangles[3] = 0; //FL
    newTriangles[4] = 1; //FR
    newTriangles[5] = 3; //BR


    //Create the rectangle
    Mesh mesh = new Mesh();

    mesh.vertices = newVertices;
    mesh.triangles = newTriangles;

    //Give the mesh to the gameobject
    newRect.GetComponent<MeshFilter>().mesh = mesh;

    newRect.transform.parent = transform;

    debugRectangles.Add(newRect);
  }


}
