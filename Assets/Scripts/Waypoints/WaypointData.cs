using UnityEngine;
using System.Collections;

public class WaypointData
{
  //The position of the waypoint
  public Vector3 pos;
  //Is this a stop wp where the car should stop before it continues
  public bool isStop;
  //Is this a wp we have added to make a curve
  public bool isCurve;
  //The waypoint after this one
  public WaypointData nextWp = null;
  //Previous waypoint to make it easier to generate the paths
  public WaypointData previousWp = null;
  //Which lane number is this waypoint on
  public int laneNumber;


  public WaypointData(Vector3 pos, bool isStop, bool isCurve, int laneNumber)
  {
    //Increase the height to make it easier to see the lines, 
    //or they will be at the same height as the road
    this.pos = pos + new Vector3(0f, 0.01f, 0f);
    this.isStop = isStop;
    this.isCurve = isCurve;
    this.laneNumber = laneNumber;
  }
}
