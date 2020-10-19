using UnityEngine;
using System.Collections;

//Move a waypoint to the position where the mouse is in the world
public class TestWaypoint : MonoBehaviour
{


  void Start()
  {

  }


  void Update()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit))
    {
      //hit.collider.renderer.material.color = Color.red;
      //Debug.Log(hit);

      transform.position = hit.point;
    }
  }
}
