using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetTextFromSlider : MonoBehaviour
{
    public TMP_Text text;
    public Slider slider;
    public bool showPercentage;

    private void Start()
    {
        text.text = showPercentage ? (((slider.value - slider.minValue) * 100) / (slider.maxValue - slider.minValue)).ToString("0.00") + "%" : slider.value.ToString("0.00");
    }

    public void SetSliderValue(Slider _slider)
    {
        text.text = showPercentage ? (((_slider.value - _slider.minValue) * 100) / (_slider.maxValue - _slider.minValue)).ToString("0.00") + "%" : _slider.value.ToString("0.00");
    }   
}
