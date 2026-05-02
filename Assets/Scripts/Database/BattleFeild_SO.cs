using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BattleGround", menuName = "BattleBuilder/BattleGround")]
public class BattleFeild_SO : ScriptableObject
{
    [SerializeField] public int battleGroundID;
    [SerializeField] public string nameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public GameObject battleGround;
}
