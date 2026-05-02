using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hair", menuName = "UnitBuilder/Hair")]
public class UnitHair_SO : ScriptableObject
{
    [SerializeField] public int hairID;
    [SerializeField] public Sprite hair;
    [SerializeField] public Color hairColor;
}
