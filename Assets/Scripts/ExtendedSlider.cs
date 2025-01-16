using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendedSlider : MonoBehaviour
{
    public Slider slider;
    public Text valueDisplay;

    public void addOne()
    {
        if (slider.value < slider.maxValue)
        {
            slider.value++;
        }
    }

    public void removeOne()
    {
        if (slider.value > slider.minValue)
        {
            slider.value--;
        }
    }

    public void updateValueDisplay(float value)
    {
        valueDisplay.text = value.ToString();
    }
}
