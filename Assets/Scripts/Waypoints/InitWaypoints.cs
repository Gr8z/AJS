using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//Init the waypoints by adding a smooth curve to those that need it
public class InitWaypoints : MonoBehaviour
{
  //All the waypoints where a car can spawn
  public GameObject[] spawnWayPoints;

  //Control point parameters
  public float turnLeftDist; //8
  public float turnRightDist; //4

  //Array with the final wp data with the final paths
  //Need 8 because the software has to predict the future pos so no randomness allowed when
  //a car has been given a path
  public static List<WaypointData> finalPaths = new List<WaypointData>();
  //Need two lists or one will be updated in OnDrawGizmos and ruin everything
  public static List<WaypointData> finalPathsDebug = new List<WaypointData>();



  void Start()
  {
    //Has to do it here as well
    //Generate the unsmooth paths
    GeneratePaths(finalPaths);

    //Smooth the path with curves
    SmoothPaths(finalPaths);
  }



  //To debug before game begins, change in final version
  void OnDrawGizmos()
  {
    //Important to clear in OnDrawGizmos or will add more paths to the same list
    finalPathsDebug.Clear();

    //Generate the unsmooth paths
    GeneratePaths(finalPathsDebug);

    //Smooth the path with curves
    SmoothPaths(finalPathsDebug);

    //Display the paths in the traffic intersection only
    for (int i = 0; i < finalPathsDebug.Count; i++)
    {
      //Spawn wp
      WaypointData spawnWP = finalPathsDebug[i];

      //Get the wp where the curve starts
      WaypointData currentWp = spawnWP.nextWp;

      //Straight line through the intersection
      if (currentWp.nextWp.isCurve == false)
      {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(currentWp.pos, currentWp.nextWp.pos);
      }
      //Curve
      else
      {
        Gizmos.color = Color.blue;

        while (currentWp.nextWp.isCurve == true)
        {
          Gizmos.DrawLine(currentWp.pos, currentWp.nextWp.pos);

          currentWp = currentWp.nextWp;
        }

        Gizmos.DrawLine(currentWp.pos, currentWp.nextWp.pos);
      }
    }
  }



  //Generate the 8 possible paths and store them in an array
  void GeneratePaths(List<WaypointData> pathList)
  {
    //Loop through all spawnpoints and build the final paths
    for (int i = 0; i < spawnWayPoints.Length; i++)
    {
      GameObject spawnWpObj = spawnWayPoints[i];

      //Get the lane number
      int laneNumber = spawnWpObj.GetComponent<Waypoint>().laneNumber;

      //This will form the final path
      WaypointData latestWp = new WaypointData(spawnWpObj.transform.position, false, false, laneNumber);

      //Add it to the list of final paths because this is a spawn position
      pathList.Add(latestWp);

      //The waypoint after the spawn wp is always where the curve starts
      GameObject startWpObj = spawnWpObj.GetComponent<Waypoint>().nextWaypoints[0];

      latestWp.nextWp = new WaypointData(startWpObj.transform.position, true, false, laneNumber);

      //Save the previous wp
      WaypointData tmp = latestWp;

      latestWp = latestWp.nextWp;

      latestWp.previousWp = tmp;

      //Need this so we always begin from this wp each time we check if we drive straight or have a curve
      WaypointData wpBeforeTurn = latestWp;


      //Now we always have 2 options: turn or drive straight forward
      List<GameObject> afterTurnWPs = startWpObj.GetComponent<Waypoint>().nextWaypoints;

      for (int j = 0; j < afterTurnWPs.Count; j++)
      {
        //We need one object for each path, so maybe we need to copy the latest wp obj
        //When we copy we also add it to the list of final paths
        //Check if the next wp is null to see if we need to copy
        if (wpBeforeTurn.nextWp != null)
        {
          //Copy the latest wp obj
          wpBeforeTurn = GetCopyOfLatestWp(wpBeforeTurn, pathList);
        }

        latestWp = wpBeforeTurn;

        //Get the wp after the turn this loop
        GameObject afterTurnWPObj = afterTurnWPs[j];

        //Add the wp after the intersection and the last wp (same for all) 
        latestWp.nextWp = new WaypointData(afterTurnWPObj.transform.position, false, false, laneNumber);

        //Change wp to the last wp
        latestWp = latestWp.nextWp;

        Vector3 endPos = afterTurnWPObj.GetComponent<Waypoint>().nextWaypoints[0].transform.position;

        latestWp.nextWp = new WaypointData(endPos, false, false, laneNumber);
      }
    }
  }



  //Copy the a waypoint obj
  WaypointData GetCopyOfLatestWp(WaypointData latestWp, List<WaypointData> pathList)
  {
    WaypointData first = latestWp.previousWp;

    WaypointData newWp = new WaypointData(first.pos, first.isStop, first.isCurve, first.laneNumber);

    //Add it to the list of all paths
    pathList.Add(newWp);

    WaypointData tmp = newWp;

    newWp.nextWp = new WaypointData(latestWp.pos, latestWp.isStop, latestWp.isCurve, latestWp.laneNumber);

    //Go to the next wp in the path
    newWp = newWp.nextWp;

    newWp.previousWp = tmp;

    return newWp;
  }



  //Smooth the paths with curves
  void SmoothPaths(List<WaypointData> pathList)
  {
    for (int i = 0; i < pathList.Count; i++)
    {
      WaypointData thisWp = pathList[i];

      Vector3 spawnPos = thisWp.pos;
      Vector3 startPos = thisWp.nextWp.pos;
      Vector3 endPos = thisWp.nextWp.nextWp.pos;
      Vector3 deSpawnPos = thisWp.nextWp.nextWp.nextWp.pos;

      //Get the coordinates of the curve (if any)
      List<Vector3> turnCoordinates = GenerateTurnCoordinates(spawnPos, startPos, endPos, deSpawnPos);

      int laneNumber = thisWp.laneNumber;

      //Add the curve to the path
      if (turnCoordinates != null)
      {
        WaypointData latestWp = thisWp.nextWp;

        for (int k = 0; k < turnCoordinates.Count; k++)
        {
          latestWp.nextWp = new WaypointData(turnCoordinates[k], false, true, laneNumber);

          latestWp = latestWp.nextWp;
        }

        //Need to add the last 2 coordinates manually
        latestWp.nextWp = new WaypointData(endPos, false, false, laneNumber);

        latestWp.nextWp.nextWp = new WaypointData(deSpawnPos, false, false, laneNumber);
      }
    }
  }



  //Generate eventual turn coordinates form one start wp and one end wp
  //Returns null if not turning
  //spawnPos - where the cars are spawning
  //startPos - where the curve is beginning
  //endPos - where the curve is ending
  //deSpawnPos - where the paths are ending
  List<Vector3> GenerateTurnCoordinates(Vector3 spawnPos, Vector3 startPos, Vector3 endPos, Vector3 deSpawnPos)
  {
    //The vector to the beginning of the curve from the spawn
    Vector3 toBeforeTurnWP = startPos - spawnPos;

    //The vector to the wp after the curve from the spawn
    Vector3 toAfterTurnWP = endPos - spawnPos;

    //Is this a turn or straight forward? Measure the angle!
    float angle = Vector3.Angle(toBeforeTurnWP, toAfterTurnWP);

    //Straight line - cant use 0 because of maybe precision error
    if (angle < 0.001f)
    {
      return null;
    }
    //This is a turn - make it smooth by adding a bezier curve
    else
    {
      //The bezier curve has control points that determines the shape of the curve
      //So to make this work we need the forward direction of the start/end wps to position
      //the control points

      //The direction of the start wp from the spawn wp
      Vector3 toStartDir = toBeforeTurnWP.normalized;

      //The direction of the despawn wp from the end after the curve wp
      Vector3 toEndDir = (deSpawnPos - endPos).normalized;

      //Distance to control points
      float cpDistance = 0f;

      //Need to determine if we are turning left or right to get the correct shape of the curve
      if (IsTurningLeft(toStartDir, startPos, endPos))
      {
        cpDistance = turnLeftDist;
      }
      else
      {
        cpDistance = turnRightDist;
      }

      //Generate the coordinates of the curve = excluding start/end points
      List<Vector3> turnCoordinates = GenerateBezierCurve(startPos, endPos, toStartDir, toEndDir, cpDistance);

      return turnCoordinates;
    }
  }



  //Are we turning left or right
  //youDir - the direction you are facing
  //youPos - the position of you
  //wpPos - ths position of the wp
  bool IsTurningLeft(Vector3 youDir, Vector3 youPos, Vector3 wpPos)
  {
    bool isTurningLeft = true;

    //The direction you are facing
    //Vector3 youDir = youTrans.forward;

    //The direction from you to the waypoint
    Vector3 waypointDir = wpPos - youPos;

    //The cross product between these vectors
    Vector3 crossProduct = Vector3.Cross(youDir, waypointDir);

    //The dot product between the your up vector and the cross product
    //This can be said to be a volume that can be negative
    float dotProduct = Vector3.Dot(crossProduct, Vector3.up);

    //Now we can decide if we should turn left or right
    if (dotProduct > 0f)
    {
      //Debug.Log("Turn right");

      isTurningLeft = false;
    }
    //else
    //{
    //    Debug.Log("Turn left");
    //}

    return isTurningLeft;
  }



  //Generate the Bezier curve
  //startDir is the forward direction of the start point so we can position the control points - same with endDir
  List<Vector3> GenerateBezierCurve(Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, float controlPointDist)
  {
    //The positions of the control points
    Vector3 cp1 = startPos + startDir * controlPointDist;
    Vector3 cp2 = endPos + endDir * -controlPointDist;

    //The resolution of the line
    //Make sure the resolution is adding up to 1, so 0.3 will give a gap at the end, but 0.2 will work
    float resolution = 0.1f;

    //How many loops?
    int loops = Mathf.FloorToInt(1f / resolution);

    //To save the coordinates
    List<Vector3> turnCoordinates = new List<Vector3>();

    for (int i = 1; i <= loops; i++)
    {
      //Which t position are we at?
      float t = i * resolution;

      //Find the coordinates between the control points with a Catmull-Rom spline
      Vector3 newPos = DeCasteljausAlgorithm(startPos, cp1, cp2, endPos, t);

      //Add the coordinate to the list of all coordinates
      if (i < loops)
      {
        turnCoordinates.Add(newPos);
      }
    }

    return turnCoordinates;
  }



  //The De Casteljau's Algorithm to make a Bezier curve
  //A - start
  //B - control point 1
  //C - control point 2
  //D - end
  Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
  {
    //Linear interpolation = lerp = (1 - t) * A + t * B
    //Could use Vector3.Lerp(A, B, t)

    //To make it faster
    float oneMinusT = 1f - t;

    //Layer 1
    Vector3 Q = oneMinusT * A + t * B;
    Vector3 R = oneMinusT * B + t * C;
    Vector3 S = oneMinusT * C + t * D;

    //Layer 2
    Vector3 P = oneMinusT * Q + t * R;
    Vector3 T = oneMinusT * R + t * S;

    //Final interpolated position
    Vector3 U = oneMinusT * P + t * T;

    return U;
  }
}
