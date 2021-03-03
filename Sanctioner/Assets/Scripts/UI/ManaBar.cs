using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public Slider slider;
    public Text textValue;

    public void SetMaxMana(float mana)
    {
        slider.maxValue = mana;
        slider.value = mana;
        SetText();
    }

    public void SetMana(float mana)
    {
        slider.value = mana;
        SetText();
    }

    private void SetText()
    {
        textValue.text = System.Math.Round(slider.value, 2) + "/" + slider.maxValue;
    }
}
