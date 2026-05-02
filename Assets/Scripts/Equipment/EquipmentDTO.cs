using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentDTO
{
    public string equiptmentID;

    public string equiptmentNickName;
    public int equiptmentSOID;
    public int runeID;

    public int equiptmentLevel =1;

    public int attack =0;
    public int attackElement=0;
    public int health=0;
    public int speed=0;
    public int defense=0;
    public int defenseElement=0;
    public int accuracy=0;
    public int critrate=0;
    public int critDamage=0;
    public int resistence=0;
    public int mass = 0;

    public int equiptmentRarityIndex;
    public int equiptmentGradeIndex;

    public EquipmentDTO()
    {

    }

    public EquipmentDTO(Equipment equipment)
    {
        equiptmentID = equipment.equiptmentID;
        equiptmentNickName = equipment.equiptmentNickName;
        equiptmentSOID = equipment.equiptment_SO.equipmentID;
        runeID = equipment.rune_SO.runeID;
        equiptmentLevel = equipment.equiptmentLevel;

        attack = equipment.attack;
        attackElement = equipment.attackElement;
        health = equipment.health;
        speed = equipment.speed;
        defense = equipment.defense;
        defenseElement = equipment.defenseElement;
        accuracy = equipment.accuracy;
        critrate = equipment.critrate;
        critDamage = equipment.critDamage;
        resistence = equipment.resistence;
        mass = equipment.mass;

        equiptmentRarityIndex = equipment.equiptmentRarityIndex;
        equiptmentGradeIndex = equipment.equiptmentGradeIndex;
    }
}
