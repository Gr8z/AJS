using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadManager : MonoBehaviour
{
  public GameObject TwoLaneSystem;
  public GameObject FourLaneSystem;
  public GameObject SixLaneSystem;

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

  void Update()
  {
    // Load MainMenu if you press the ESC key.
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      SceneManager.LoadScene("Menu");
    }
  }
}
