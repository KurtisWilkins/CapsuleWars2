using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitDTO
{
    public string iD;
    public string name;

    public int level;
    public int rank;
    public int classID;
    public int type1ID;
    public int type2ID;
    public int rarity;
    public int evolution;
    public int evolutionStratID;
    public int raceID;
    public int armatureID;


    public int targetingID;
    public int basicID;
    public int passiveID;

    public float currentHealthPrecent;
    public float currentExpPrecent;

    public int attackBase;
    public int attackElementBase;
    public int healthBase;
    public int defenceBase;
    public int defenceElementBase;
    public int speedBase;
    public int accuracryBase;
    public int resistanceBase;
    public int critRateBase;
    public int critDamageBase;

    public int mass;

    public int bodyID;
    public int eyesID;
    public int hairID;

    public float totalDamageDealt = 0f;
    public int attackCountDealt = 0;
    public float totalDamageRecieved = 0f;
    public int attackCountRecieved = 0;
    public float totalHealingDone = 0f;
    public int healingCountDealt = 0;
    public float totalHealingRecieved = 0f;
    public int healingCountRecieved = 0;
    public int totalKOs = 0;
    public int revives = 0;
    public int revived = 0;
    public int faints = 0;
    public int buffsApplied = 0;
    public int debuffsApplied = 0;
    public int buffsReceived = 0;
    public int debuffsReceived = 0;
    public int battlesBeenIn = 0;
    public int battlesWon = 0;
    public int streakKOsWOFaint = 0;
    public int battleStreakWOFaint = 0;



    public EquipmentDTO helmet = new EquipmentDTO();
    public EquipmentDTO shoulders = new EquipmentDTO();
    public EquipmentDTO back = new EquipmentDTO();
    public EquipmentDTO chest = new EquipmentDTO();
    public EquipmentDTO arms = new EquipmentDTO();
    public EquipmentDTO legs = new EquipmentDTO();
    public EquipmentDTO rightHand = new EquipmentDTO();
    public EquipmentDTO leftHand = new EquipmentDTO();

    public UnitDTO() { }

    public UnitDTO(UnitStatusController unitStatus, UnitHealthController unitHealth, UnitAttackController unitAttack, UnitMovementController unitMovement)
    {
        currentHealthPrecent = unitHealth.GetCurrentHealth();

        attackBase = unitStatus.GetBaseAttack();
        attackElementBase = unitStatus.GetBaseAttackElement();
        healthBase = unitStatus.GetBaseHealth();
        defenceBase = unitStatus.GetBaseDefense();
        defenceElementBase = unitStatus.GetBaseDefenseElement();
        speedBase = unitStatus.GetBaseSpeed();
        accuracryBase = unitStatus.GetBaseAccuracy();
        resistanceBase = unitStatus.GetBaseResistance();
        critRateBase = unitStatus.GetBaseCritRate();
        critDamageBase = unitStatus.GetBaseCritDamage();
        mass = unitStatus.GetBaseMass();


    }

    public UnitDTO(NPCUnitRoster nPC)
    {
        iD = Guid.NewGuid().ToString();
        name = nPC.name;

        level = nPC.level;
        rank = nPC.rank;
        classID = nPC.classID.classTypeID;
        type1ID = nPC.type1ID.elementTypeID;
        type2ID = nPC.type2ID.elementTypeID;
        rarity = nPC.rarity.rarityID;
        raceID = nPC.raceID.BodyID;
        armatureID = nPC.armatureID;


        targetingID = nPC.targeting.targetingID;
        basicID = nPC.basic.abilityID;
        passiveID = nPC.passive.abilityID;

        currentHealthPrecent = 1f;
        currentExpPrecent = 0f;

        attackBase = nPC.attackBase;
        attackElementBase = nPC.attackElementBase;
        healthBase = nPC.healthBase;
        defenceBase = nPC.defenceBase;
        defenceElementBase = nPC.defenceElementBase;
        speedBase = nPC.speedBase;
        accuracryBase = nPC.accuracryBase;
        resistanceBase = nPC.resistanceBase;
        critRateBase = nPC.critRateBase;
        critDamageBase = nPC.critDamageBase;

        mass = nPC.mass;

        bodyID = nPC.raceID.BodyID;
        eyesID = nPC.eyesID.eyeID;
        hairID = nPC.hairID.hairID;

        nPC.helmet.equiptmentSOID = nPC.helmet_SO.equipmentID;
        nPC.helmet.runeID = nPC.helmetRune_SO.runeID;
        helmet = nPC.helmet;

        nPC.shoulders.equiptmentSOID = nPC.shoulders_SO.equipmentID;
        nPC.shoulders.runeID = nPC.shouldersRune_SO.runeID;
        shoulders = nPC.shoulders;

        nPC.back.equiptmentSOID = nPC.back_SO.equipmentID;
        nPC.back.runeID = nPC.backRune_SO.runeID;
        back = nPC.back;

        nPC.chest.equiptmentSOID = nPC.chest_SO.equipmentID;
        nPC.chest.runeID = nPC.chestRune_SO.runeID;
        chest = nPC.chest;

        nPC.arms.equiptmentSOID = nPC.arms_SO.equipmentID;
        nPC.arms.runeID = nPC.armsRune_SO.runeID;
        arms = nPC.arms;

        nPC.legs.equiptmentSOID = nPC.legs_SO.equipmentID;
        nPC.legs.runeID = nPC.legsRune_SO.runeID;
        legs = nPC.legs;

        nPC.rightHand.equiptmentSOID = nPC.righthand_SO.equipmentID;
        nPC.rightHand.runeID = nPC.rightHandRune_SO.runeID;
        rightHand = nPC.rightHand;

        nPC.leftHand.equiptmentSOID = nPC.leftHand_SO.equipmentID;
        nPC.leftHand.runeID = nPC.leftHandRune_SO.runeID;
        leftHand = nPC.leftHand;
    }


    public int BuildAttackMod()
    {
        return Mathf.RoundToInt(attackBase * (1 + (level * .02f) + (rank * .12f))) + GetEquipmentAttack();
    }

    public int BuildAttackElementMod()
    {
        return GetEquipmentAttackElement() + Mathf.RoundToInt(attackElementBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildHealthMod()
    {
        return GetEquipmentHealth() + Mathf.RoundToInt(healthBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildSpeedMod()
    {
        return GetEquipmentSpeed() + Mathf.RoundToInt(speedBase * ((1 + rank * .1f) + (level * .08f)));
    }

    public int BuildDefenseMod()
    {
        return GetEquipmentDefense() + Mathf.RoundToInt(defenceBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildDefenseElementMod()
    {
        return GetEquipmentDefenseElement() + Mathf.RoundToInt(defenceElementBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildAccuracyMod()
    {
        return GetEquipmentAccuracy() + Mathf.RoundToInt(accuracryBase * (1 + level * .02f + rank * .1f));
    }

    public int BuildResistanceMod()
    {
        return GetEquipmentResistence() + Mathf.RoundToInt(resistanceBase * (1 + level * .02f + rank * .1f));
    }

    public int BuildCritRateMod()
    {
        return GetEquipmentCritRate() + Mathf.RoundToInt(critRateBase * (1 + rank * .1f));
    }

    public int BuildCritDamageMod()
    {
        return GetEquipmentCritDamage() + Mathf.RoundToInt(critDamageBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildMassMod()
    {
        return GetEquipmentmass() + Mathf.RoundToInt(mass * (1 + rank * .1f));
    }

    public int GetEquipmentAttack()
    {
        return helmet.attack + shoulders.attack + chest.attack + back.attack + arms.attack + legs.attack + rightHand.attack + leftHand.attack;
    }

    public int GetEquipmentAttackElement()
    {
        return helmet.attackElement + shoulders.attackElement + chest.attackElement + back.attackElement + arms.attackElement + legs.attackElement + rightHand.attackElement + leftHand.attackElement;
    }

    public int GetEquipmentHealth()
    {
        return helmet.health + shoulders.health + chest.health + back.health + arms.health + legs.health + rightHand.health + leftHand.health;
    }

    public int GetEquipmentSpeed()
    {
        return helmet.speed + shoulders.speed + chest.speed + back.speed + arms.speed + legs.speed + rightHand.speed + leftHand.speed;
    }

    public int GetEquipmentDefense()
    {
        return helmet.defense + shoulders.defense + chest.defense + back.defense + arms.defense + legs.defense + rightHand.defense + leftHand.defense;
    }

    public int GetEquipmentDefenseElement()
    {
        return helmet.defenseElement + shoulders.defenseElement + chest.defenseElement + back.defenseElement + arms.defenseElement + legs.defenseElement + rightHand.defenseElement + leftHand.defenseElement;
    }

    public int GetEquipmentAccuracy()
    {
        return helmet.accuracy + shoulders.accuracy + chest.accuracy + back.accuracy + arms.accuracy + legs.accuracy + rightHand.accuracy + leftHand.accuracy;
    }

    public int GetEquipmentResistence()
    {
        return helmet.resistence + shoulders.resistence + chest.resistence + back.resistence + arms.resistence + legs.resistence + rightHand.resistence + leftHand.resistence;
    }

    public int GetEquipmentCritRate()
    {
        return helmet.critrate + shoulders.critrate + chest.critrate + back.critrate + arms.critrate + legs.critrate + rightHand.critrate + leftHand.critrate;
    }

    public int GetEquipmentCritDamage()
    {
        return helmet.critDamage + shoulders.critDamage + chest.critDamage + back.critDamage + arms.critDamage + legs.critDamage + rightHand.critDamage + leftHand.critDamage;
    }

    public int GetEquipmentmass()
    {
        return helmet.mass + shoulders.mass + chest.mass + back.mass + arms.mass + legs.mass + rightHand.mass + leftHand.mass;
    }

    public void RecordUnitStats(BattleUnitStats battleUnitStats,bool won,bool fainted)
    {
        totalDamageDealt += battleUnitStats.totalDamageDealt;
        attackCountDealt += battleUnitStats.attackCountDealt;
        totalDamageRecieved += battleUnitStats.totalDamageRecieved;
        attackCountRecieved += battleUnitStats.attackCountRecieved;
        totalHealingDone += battleUnitStats.totalHealingDone;
        healingCountDealt += battleUnitStats.healingCountDealt;
        totalHealingRecieved += battleUnitStats.totalHealingRecieved;
        healingCountRecieved += battleUnitStats.healingCountRecieved;
        totalKOs += battleUnitStats.totalKOs;
        revives += battleUnitStats.revives;
        revived += battleUnitStats.revived;
        faints += battleUnitStats.faints;
        buffsApplied += battleUnitStats.buffsApplied;
        debuffsApplied += battleUnitStats.debuffsApplied;
        buffsReceived += battleUnitStats.buffsReceived;
        debuffsReceived += battleUnitStats.debuffsReceived;

        battlesBeenIn++;
        if (won)
        {
            battlesWon++;
        }
        if (!fainted)
        {
            streakKOsWOFaint += battleUnitStats.totalKOs;
            battleStreakWOFaint++;
        }
        else
        {
            streakKOsWOFaint = 0;
            battleStreakWOFaint = 0;
        }
    }

}
