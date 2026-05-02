using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;

[System.Serializable]
public class UnitBattlePlacement
{
    [SerializeField] public int xPosition;
    [SerializeField] public int yPosition;
    [SerializeField] public int gridIndex;
    [SerializeField] public int teamIndex;
    [SerializeField] public bool isDeployed;
    [SerializeField] public UnitDTO UnitDTO;
    [SerializeField] public Texture unitIcon;
}
