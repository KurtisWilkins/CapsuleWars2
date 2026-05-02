using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Eyes", menuName = "UnitBuilder/Eyes")]
public class UnitEye_SO : ScriptableObject
{
    [SerializeField] public int eyeID;

    [SerializeField] public Sprite l_BackEye;
    [SerializeField] public Sprite l_FrontEye;
    [SerializeField] public Sprite r_BackEye;
    [SerializeField] public Sprite r_FrontEye;

    [SerializeField] public Color l_BackEyeColor;
    [SerializeField] public Color l_FrontEyeColor;
    [SerializeField] public Color r_BackEyeColor;
    [SerializeField] public Color r_FrontEyeColor;
}
