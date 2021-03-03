using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider slider;
    public Text textValue;

    public void SetMaxStamina(float stamina)
    {
        slider.maxValue = stamina;
        slider.value = stamina;
        SetText();
    }

    public void SetStamina(float stamina)
    {
        slider.value = stamina;
        SetText();
    }

    private void SetText()
    {
        textValue.text = System.Math.Round(slider.value, 2) + "/" + slider.maxValue;
    }
}
