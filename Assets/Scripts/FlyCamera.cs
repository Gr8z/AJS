using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCamera : MonoBehaviour
{

  /*
    Controls

    Mouse : Drag Camera
    WASD  : Directional movement
    Shift : Increase speed
    Space : Moves camera directly up per its local Y-axis
  */

  //
  // VARIABLES
  //

  private Vector3 mouseOrigin;  // Position of cursor when mouse dragging starts

  private bool isPanning;   // Is the camera being panned?
  private bool isRotating;  // Is the camera being rotated?
  private bool isZooming;   // Is the camera zooming?

  public float turnSpeed = 10.0f;  // Speed of camera turning when mouse moves in along an axis
  float mainSpeed = 50.0f;        //regular speed
  float shiftAdd = 60.0f;         //multiplied by how long shift is held.  Basically running
  float maxShift = 100.0f;        //Maximum speed when holdin gshift
  private float totalRun = 1.0f;

  //
  // UPDATE
  //

  void Update()
  {
    // Get the left mouse button
    if (Input.GetMouseButtonDown(0))
    {
      // Get mouse origin
      mouseOrigin = Input.mousePosition;
      isRotating = true;
    }

    // Disable movements on button release
    if (!Input.GetMouseButton(0)) isRotating = false;

    // Rotate camera along X and Y axis
    if (isRotating)
    {
      Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

      transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
      transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);
    }

    // Keyboard commands
    Vector3 p = GetBaseInput();

    if (Input.GetKey(KeyCode.LeftShift))
    {
      totalRun += Time.deltaTime;
      p = p * totalRun * shiftAdd;
      p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
      p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
      p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
    }
    else
    {
      totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
      p = p * mainSpeed;
    }

    p = p * Time.deltaTime;
    Vector3 newPosition = transform.position;
    if (Input.GetKey(KeyCode.Space))
    {
      //If player wants to move on X and Z axis only
      transform.Translate(p);
      newPosition.x = transform.position.x;
      newPosition.z = transform.position.z;
      transform.position = newPosition;
    }
    else
    {
      transform.Translate(p);
    }
  }

  private Vector3 GetBaseInput()
  {
    //returns the basic values, if it's 0 than it's not active.
    Vector3 p_Velocity = new Vector3();
    if (Input.GetKey(KeyCode.W))
    {
      p_Velocity += new Vector3(0, 0, 1);
    }
    if (Input.GetKey(KeyCode.S))
    {
      p_Velocity += new Vector3(0, 0, -1);
    }
    if (Input.GetKey(KeyCode.A))
    {
      p_Velocity += new Vector3(-1, 0, 0);
    }
    if (Input.GetKey(KeyCode.D))
    {
      p_Velocity += new Vector3(1, 0, 0);
    }
    return p_Velocity;
  }

}
