using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : PanelBase
{
    private Transform Slider;
    private Transform Progress;
    private void Start()
    {
        Slider = transform.Find("Slider");
        Progress = transform.Find("Text");
    }

    public void SetSliderValue(float value)
    {
        Slider.GetComponent<Slider>().value = value;
    }
    public void AddProgress(float value)
    {
        Slider.GetComponent<Slider>().value += value;
    }
    public void RefreshProgress(float progress)
    {
        Slider.GetComponent<Slider>().value = progress;
        Progress.GetComponent<TextMeshProUGUI>().text = progress * 100f + "%";
    }
    public void SetProgressText(string progress)
    {
        Progress.GetComponent<TextMeshProUGUI>().text = progress;
    }
}
