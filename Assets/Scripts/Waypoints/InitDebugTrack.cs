using UnityEngine;
using System.Collections;

//Creates the waypoints for the debug track where we debug the pid controller
public class InitDebugTrack : MonoBehaviour
{
  public GameObject waypointObj;

  public GameObject carObj;



  void Start()
  {
    int laneNumber = waypointObj.GetComponent<Waypoint>().laneNumber;

    WaypointData startWaypoint = new WaypointData(waypointObj.transform.position, false, false, laneNumber);

    GameObject wp = waypointObj.GetComponent<Waypoint>().nextWaypoints[0];

    WaypointData currentWaypoint = startWaypoint;

    int counter = 0;
    while (!wp.GetComponent<Waypoint>().isStop)
    {
      currentWaypoint.nextWp = new WaypointData(wp.transform.position, false, false, laneNumber);

      wp = wp.GetComponent<Waypoint>().nextWaypoints[0];

      currentWaypoint = currentWaypoint.nextWp;

      counter += 1;

      if (counter > 50)
      {
        print("endless loop");

        break;
      }
    }

    //Close the loop
    currentWaypoint.nextWp = startWaypoint;

    carObj.GetComponent<CarController>().currentWaypoint = startWaypoint.nextWp;
    carObj.GetComponent<CarController>().previousWaypoint = startWaypoint;
  }
}
