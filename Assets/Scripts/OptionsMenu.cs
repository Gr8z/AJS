﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
  public Slider speedSlider;
  public Text sliderText;

  public Toggle enableModelS;
  public Toggle enableModelX;
  public Toggle enableRoadster;
  public Toggle enableAudio;

  public TMP_Dropdown numRoads;
  public TMP_Dropdown numLanes;


  void Start()
  {
    speedSlider.value = PlayerPrefs.GetFloat("vehicleSpeed");

    enableModelS.isOn = (PlayerPrefs.GetInt("enableModelS") == 1) ? true : false;
    enableModelX.isOn = (PlayerPrefs.GetInt("enableModelX") == 1) ? true : false;
    enableRoadster.isOn = (PlayerPrefs.GetInt("enableRoadster") == 1) ? true : false;
    enableAudio.isOn = (PlayerPrefs.GetInt("enableAudio") == 1) ? true : false;

    numRoads.value = PlayerPrefs.GetInt("numRoads") - 3;
    numLanes.value = (PlayerPrefs.GetInt("numLanes") / 2) - 1;
  }

  public void clickDone()
  {
    PlayerPrefs.SetFloat("vehicleSpeed", speedSlider.value);
    PlayerPrefs.SetInt("enableModelS", enableModelS.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableModelX", enableModelX.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableRoadster", enableRoadster.isOn ? 1 : 0);
    PlayerPrefs.SetInt("enableAudio", enableAudio.isOn ? 1 : 0);

    // Pad the values by 2 sinces there is no 0 and 1.
    PlayerPrefs.SetInt("numRoads", numRoads.value + 3);
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
