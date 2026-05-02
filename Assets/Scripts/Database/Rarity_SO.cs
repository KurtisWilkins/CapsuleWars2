using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New RarityType", menuName = "UnitBuilder/RarityType")]
public class Rarity_SO : ScriptableObject
{
    [SerializeField] public int rarityID;
    [SerializeField] public string nameID;
    [SerializeField] public string descritpionID;
    [SerializeField] public Sprite rarityTypeIcon;
    [SerializeField] public Color rarityTextColor;

    public void SetTextForRarity(Text text)
    {
        text.text = LocalizationManager.GetTranslation(nameID);
        text.color = rarityTextColor;
    }
}
