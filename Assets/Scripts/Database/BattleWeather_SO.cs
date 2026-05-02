using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BattleGround", menuName = "BattleBuilder/BattleWeather")]
public class BattleWeather_SO : ScriptableObject
{
    [SerializeField] public int battleWeatherID;
    [SerializeField] public string nameID;
    [SerializeField] public string descriptionID;
    [SerializeField] public bool hasGameObject = true;
    [SerializeField] public GameObject battleWeather;
}
