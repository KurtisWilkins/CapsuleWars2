using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment
{
    public string equiptmentID;

    public string equiptmentNickName;
    [SerializeField] public UnitEquipment_SO equiptment_SO;
    public Rune_SO rune_SO;

    public int equiptmentLevel;

    [SerializeField] public int attack = 0;
    [SerializeField] public int attackElement = 0;
    [SerializeField] public int health = 0;
    [SerializeField] public int speed = 0;
    [SerializeField] public int defense = 0;
    [SerializeField] public int defenseElement = 0;
    [SerializeField] public int accuracy = 0;
    [SerializeField] public int critrate = 0;
    [SerializeField] public int critDamage = 0;
    [SerializeField] public int resistence = 0;
    [SerializeField] public int mass = 0;

    public int equiptmentRarityIndex;
    public int equiptmentGradeIndex;

    public Equipment()
    {

    }

    public Equipment(EquipmentDTO equiptment, Database database)
    {
        equiptmentID = equiptment.equiptmentID;
        equiptmentNickName = equiptment.equiptmentNickName;
        equiptment_SO = database.unitEquipment[equiptment.equiptmentSOID];
        rune_SO = database.runes[ equiptment.runeID];
        equiptmentLevel = equiptment.equiptmentLevel;

        attack = equiptment.attack;
        attackElement = equiptment.attackElement;
        health = equiptment.health;
        speed = equiptment.speed;
        defense = equiptment.defense;
        defenseElement = equiptment.defenseElement;
        accuracy = equiptment.accuracy;
        critrate = equiptment.critrate;
        critDamage = equiptment.critDamage;
        resistence = equiptment.resistence;
        mass = equiptment.mass;

        equiptmentRarityIndex = equiptment.equiptmentRarityIndex;
        equiptmentGradeIndex = equiptment.equiptmentGradeIndex;
    }
}
