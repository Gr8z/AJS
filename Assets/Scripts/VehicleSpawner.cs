using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
  public Vector3 targetPosition;
  public GameObject[] vehicles;
  public float spawnTime;
  public float spawnDelay;

  // Start is called before the first frame update
  void Start()
  {
    InvokeRepeating("SpawnVehicle", spawnTime, spawnDelay);
  }


  public void SpawnVehicle()
  {
    GameObject randomVehicle = vehicles[Random.Range(0, vehicles.Length)];
    GameObject spawnedVehicle = Instantiate(randomVehicle, transform.position, transform.rotation);
    spawnedVehicle.AddComponent<CarMove>();
  }
}
