using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectsStatsHolder : MonoBehaviour
{
    [SerializeField] public Image effectIcon;
    [SerializeField] public Text effectAmount;
    [SerializeField] public StatusEffects_SO effect;
    [SerializeField] public AbilityDescriptionPopup popup;

    void Awake()
    {
        effectIcon.sprite = effect.icon;
    }

    public void loadAmount(int amount, Color color)
    {
        effectAmount.color = color;
        effectAmount.text = amount.ToString();
    }

    public void ButtonClicked()
    {
        Debug.Log("Effect Clicked: " + effect.effectNameID);
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(effect.effectNameID, effect.effectDescriptionID, effect.icon);
    }
}
