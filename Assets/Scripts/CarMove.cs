using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarMove : MonoBehaviour
{
  public Vector3 movespeed;
  public float MaxDist = 500;
  public Vector3 userDirection = Vector3.forward;

  Vector3 spawnLocation;

  // Start is called before the first frame update
  void Start()
  {
    movespeed = PlayerPrefs.GetFloat("vehicleSpeed") * Vector3.forward;
    spawnLocation = transform.position;
  }

  // Update is called once per frame
  void Update()
  {
    transform.Translate(movespeed * Time.deltaTime);

    // Delete vehicle when it reached max distance
    if (Vector3.Distance(spawnLocation, transform.position) > MaxDist)
    {
      Destroy(gameObject);
    }
  }
}
