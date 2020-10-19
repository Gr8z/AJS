using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
  public List<GameObject> nextWaypoints;

  //Should the car stop at this wp and wait for a clear path?
  //Measure this with distance so it brakes before the wp
  public bool isStop;

  //Each lane need to have a name, so we can clear the lane if an
  //emergency vehicle arrives
  public int laneNumber;



  void Start()
  {
    gameObject.SetActive(false);
  }
}
