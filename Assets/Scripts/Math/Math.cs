using UnityEngine;
using System.Collections;


public static class Math
{
  //Steer leeft or right to reach a target
  public static float DirectionToReachTarget(Transform youTrans, Vector3 steerPos, Vector3 targetPos)
  {
    //The right direction of the direction you are facing
    Vector3 youDir = youTrans.right;

    //The direction from you to the waypoint
    Vector3 waypointDir = targetPos - steerPos;

    //The dot product between the vectors
    float dotProduct = Vector3.Dot(youDir, waypointDir);

    //Now we can decide if we should turn left or right
    if (dotProduct > 0f)
    {
      //Debug.Log("Turn right");

      return 1f;
    }
    else
    {
      //Debug.Log("Turn left");

      return -1f;
    }
  }



  //How far between 2 waypoints have we travelled
  public static float CalculateProgressBetweenWaypoints(Vector3 carPos, Vector3 goingFromPos, Vector3 goingToPos)
  {
    //The vector between the character and the waypoint we are going from
    Vector3 a = carPos - goingFromPos;

    //The vector between the waypoints
    Vector3 b = goingToPos - goingFromPos;

    //Vector projection from https://en.wikipedia.org/wiki/Vector_projection
    float progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

    return progress;
  }



  //Get the distance between where the car is and where it should be
  public static float GetCrossTrackError(Vector3 carPos, Vector3 goingFromPos, Vector3 goingToPos)
  {
    //The first part is the same as when we check if we have passed a waypoint
    float progress = CalculateProgressBetweenWaypoints(carPos, goingFromPos, goingToPos);

    //The coordinate of the position where the car should be
    Vector3 wantedPos = goingFromPos + progress * (goingToPos - goingFromPos).normalized;

    //The error between the position where the car should be and where it is
    float error = (wantedPos - carPos).magnitude;

    return error;
  }
}
