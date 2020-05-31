using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
  public Slider speedSlider;
  public Text sliderText;

  public Toggle enableSmallVehicles;
  public Toggle enableBusses;
  public Toggle enableTrucks;
  public Toggle enableAudio;

  public TMP_Dropdown numRoads;
  public TMP_Dropdown numLanes;


  void Start()
  {
    speedSlider.value = PlayerPrefs.GetFloat("vehicleSpeed");

    enableSmallVehicles.isOn = (PlayerPrefs.GetInt("enableSmallVehicles") == 1) ? true : false;
    enableBusses.isOn = (PlayerPrefs.GetInt("enableBusses") == 1) ? true : false;
    enableTrucks.isOn = (PlayerPrefs.GetInt("enableTrucks") == 1) ? true : false;
    enableAudio.isOn = (PlayerPrefs.GetInt("enableAudio") == 1) ? true : false;

    numRoads.value = PlayerPrefs.GetInt("numRoads") - 2;
    numLanes.value = (PlayerPrefs.GetInt("numLanes") / 2) - 1;
  }

  public void clickDone()
  {
    PlayerPrefs.SetFloat("vehicleSpeed", speedSlider.value);
    PlayerPrefs.SetInt("enableSmallVehicles", enableSmallVehicles.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableBusses", enableBusses.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableTrucks", enableTrucks.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableAudio", enableAudio.isOn ? 1 : 0);

    // Pad the values by 2 sinces there is no 0 and 1.
    PlayerPrefs.SetInt("numRoads", numRoads.value + 2);
    PlayerPrefs.SetInt("numLanes", (numLanes.value + 1) * 2);
  }

  public void ToggleSound()
  {
    int isOn = enableAudio.isOn ? 1 : 0;
    PlayerPrefs.SetInt("enableAudio", isOn);
    AudioListener.volume = isOn;
  }

  public void UpdateSpeedValue()
    {
    	sliderText.text = speedSlider.value.ToString() + " km/hr";
    }
}
