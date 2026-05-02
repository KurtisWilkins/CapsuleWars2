using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitClass", menuName = "UnitBuilder/ClassType")]
public class UnitClass_SO : ScriptableObject
{
    [SerializeField] public int classTypeID;
    [SerializeField] public string nameID;
    [SerializeField] public string descritpionID;
    [SerializeField] public Sprite classidTypeIconLarge;
    [SerializeField] public Sprite classidTypeIconSmall;

    [SerializeField] public float[] attackBuff = new float[4];
    [SerializeField] public float[] attackElementBuff = new float[4];
    [SerializeField] public float[] healthBuff = new float[4];
    [SerializeField] public float[] speedBuff = new float[4];
    [SerializeField] public float[] defenseBuff = new float[4];
    [SerializeField] public float[] defenseElementBuff = new float[4];
    [SerializeField] public float[] accuracyBuff = new float[4];
    [SerializeField] public float[] resistenceBuff = new float[4];
    [SerializeField] public float[] critRateBuff = new float[4];
    [SerializeField] public float[] critDamageBuff = new float[4];
    [SerializeField] public float[] massBuff = new float[4];
}
