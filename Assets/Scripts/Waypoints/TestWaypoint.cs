using UnityEngine;
using System.Collections;

//Move a waypoint to the position where the mouse is in the world
public class TestWaypoint : MonoBehaviour
{
  void Update()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit))
    {
      transform.position = hit.point;
    }
  }
}
