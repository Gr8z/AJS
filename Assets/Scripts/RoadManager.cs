using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadManager : MonoBehaviour
{
  // Three Roads
  public GameObject ThreeRoadSystem;
  public GameObject ThreeRoadTwoLaneSystem;
  public GameObject ThreeRoadFourLaneSystem;
  public GameObject ThreeRoadSixLaneSystem;

  // Four Roads
  public GameObject FourRoadSystem;
  public GameObject FourRoadTwoLaneSystem;
  public GameObject FourRoadFourLaneSystem;
  public GameObject FourRoadSixLaneSystem;

  // Five Roads
  public GameObject FiveRoadSystem;
  public GameObject FiveRoadTwoLaneSystem;
  public GameObject FiveRoadFourLaneSystem;
  public GameObject FiveRoadSixLaneSystem;

  void Start()
  {
    int numRoads = PlayerPrefs.GetInt("numRoads");
    int numLanes = PlayerPrefs.GetInt("numLanes");

    GameObject LanesToActivate = null;

    switch (numRoads)
    {
      case 3:
        {
          switch (numLanes)
          {
            case 2: { LanesToActivate = ThreeRoadTwoLaneSystem; break; }
            case 4: { LanesToActivate = ThreeRoadFourLaneSystem; break; }
            case 6: { LanesToActivate = ThreeRoadSixLaneSystem; break; }

            default: break;
          }

          ThreeRoadSystem.SetActive(true);
          LanesToActivate.SetActive(true);
          break;
        }
      case 4:
        {
          switch (numLanes)
          {
            case 2: { LanesToActivate = FourRoadTwoLaneSystem; break; }
            case 4: { LanesToActivate = FourRoadFourLaneSystem; break; }
            case 6: { LanesToActivate = FourRoadSixLaneSystem; break; }

            default: break;
          }

          FourRoadSystem.SetActive(true);
          LanesToActivate.SetActive(true);
          break;
        }
      case 5:
        {
          switch (numLanes)
          {
            case 2: { LanesToActivate = FiveRoadTwoLaneSystem; break; }
            case 4: { LanesToActivate = FiveRoadFourLaneSystem; break; }
            case 6: { LanesToActivate = FiveRoadSixLaneSystem; break; }

            default: break;
          }

          FiveRoadSystem.SetActive(true);
          LanesToActivate.SetActive(true);
          break;
        }

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
