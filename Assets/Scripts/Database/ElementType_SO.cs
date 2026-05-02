using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New ElementType", menuName = "UnitBuilder/ElementType")]
public class ElementType_SO : ScriptableObject
{
    [SerializeField] public int elementTypeID;
    [SerializeField] public string nameID;
    [SerializeField] public string descritpionID;
    [SerializeField] public Sprite elementTypeIcon;
    [SerializeField] public Color elementTextColor;

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


    public void SetTextForElement(Text text)
    {
        text.text = LocalizationManager.GetTranslation(nameID);
        text.color = elementTextColor;
    }
}
