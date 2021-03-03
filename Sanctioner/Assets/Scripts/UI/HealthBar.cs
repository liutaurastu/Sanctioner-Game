using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Text textValue;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        SetText();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        SetText();
    }

    private void SetText()
    {
        textValue.text = slider.value + "/" + slider.maxValue;
    }
}
