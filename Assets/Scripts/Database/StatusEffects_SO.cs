using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Abilities/StatusEffects", order = 0)]
public class StatusEffects_SO : ScriptableObject
{
    [SerializeField] public int id;
    [SerializeField] public Sprite icon;
    [SerializeField] public string effectNameID;
    [SerializeField] public string effectDescriptionID;
}
