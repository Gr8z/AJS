using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
  public GameObject TwoLaneSystem;
  public GameObject FourLaneSystem;
  public GameObject SixLaneSystem;

  // Start is called before the first frame update
  void Start()
  {
    int numLanes = PlayerPrefs.GetInt("numLanes");

    switch (numLanes)
    {
      case 2: { TwoLaneSystem.SetActive(true); break; }
      case 4: { FourLaneSystem.SetActive(true); break; }
      case 6: { SixLaneSystem.SetActive(true); break; }

      default: break;
    }
  }
}
