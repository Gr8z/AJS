using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    public Text sliderText;
    public Slider slider;

    public void Update()
    {
    	sliderText.text = slider.value.ToString() + " km/hr";
    }
}
