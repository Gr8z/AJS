using UnityEngine;
using System.Collections;

public struct Rectangle
{
  //The corners of the rectangle
  public Vector3 FL;
  public Vector3 FR;
  public Vector3 BL;
  public Vector3 BR;
  //The car that belongs to this rectangle
  public int carID;


  //": this()" will call the other constructor
  public Rectangle(Vector3 FL, Vector3 FR, Vector3 BL, Vector3 BR, int carID) : this(FL, FR, BL, BR)
  {
    //Will be called after the other constructor so if we give it an idea, the id will be saved
    this.carID = carID;
  }


  //Constructor if we have no id
  public Rectangle(Vector3 FL, Vector3 FR, Vector3 BL, Vector3 BR)
  {
    this.FL = FL;
    this.FR = FR;
    this.BL = BL;
    this.BR = BR;

    //Give it a default id
    this.carID = 0;
  }
}
