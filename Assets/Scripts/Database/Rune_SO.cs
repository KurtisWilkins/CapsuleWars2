using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rune", menuName = "UnitBuilder/Rune")]
public class Rune_SO : ScriptableObject
{
    [SerializeField] public int runeID;
    [SerializeField] public string runeNameID;
    [SerializeField] public string runeDescriptionID;
    [SerializeField] public Sprite runeIcon;
    [SerializeField] public int runeBenefitRequirement = 2;
}
