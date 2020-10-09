﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarMove : MonoBehaviour
{
  public float movespeed = 0.2f;
  public Vector3 userDirection = Vector3.forward;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    transform.Translate(userDirection * movespeed);
  }
}
