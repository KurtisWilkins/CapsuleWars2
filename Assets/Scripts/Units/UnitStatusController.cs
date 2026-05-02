using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitStatusController : MonoBehaviour
{
    [SerializeField] private string unitID;
    [SerializeField] public string unitName;
    [SerializeField] public int teamID;
    [SerializeField] private bool unitDead = false;
    [SerializeField] public int level;
    [SerializeField] public int rank;
    [SerializeField] public int race;
    [SerializeField] public int rarity;
    [SerializeField] public UnitClass_SO unitClass;
    [SerializeField] public ElementType_SO unitType1;
    [SerializeField] public ElementType_SO unitType2;

    [SerializeField] private int attackBase;
    [SerializeField] private int attackElementBase;
    [SerializeField] private int healthBase;
    [SerializeField] private int defenseBase;
    [SerializeField] private int defenseElementBase;
    [SerializeField] private int speedBase;
    [SerializeField] private int accuracyBase;
    [SerializeField] private int resistanceBase;
    [SerializeField] private int critRateBase;
    [SerializeField] private int critDamageBase;

    [SerializeField] private int massBase;

    [SerializeField] private int attackModified;
    [SerializeField] private int attackElementModified;
    [SerializeField] private int healthModified;
    [SerializeField] private int defenseModified;
    [SerializeField] private int defenseElementModified;
    [SerializeField] private int speedModified;
    [SerializeField] private int accuracyModified;
    [SerializeField] private int resistanceModified;
    [SerializeField] private int critRateModified;
    [SerializeField] private int critDamageModified;

    [SerializeField] private int massModified;

    [SerializeField] public bool isProtected = false;
    public Coroutine isProtectedCoroutine;
    [SerializeField] public bool isShield = false;
    public Coroutine isShieldCoroutine;
    [SerializeField] public bool isStunned = false;
    public Coroutine isStunnedCoroutine;
    [SerializeField] public bool isFrozen = false;
    public Coroutine isFrozenCoroutine;
    [SerializeField] public bool isTrapped = false;
    public Coroutine isTrappedCoroutine;
    [SerializeField] public bool isMarked = false;
    public Coroutine isMarkedCoroutine;
    [SerializeField] public bool isUnLucky = false;
    public Coroutine isUnLuckyCoroutine;
    [SerializeField] public bool isLastStand = false;
    public Coroutine isLastStandCoroutine;
    [SerializeField] public bool isMadness = false;
    public Coroutine isMadnessCoroutine;
    [SerializeField] public bool isCursed = false;
    public Coroutine isCursedCoroutine;

    [SerializeField] public bool isAttackBoosted = false;
    public Coroutine isAttackBoostedCoroutine;
    [SerializeField] public bool isAttackBroken = false;
    public Coroutine isAttackBrokenCoroutine;
    [SerializeField] public bool isDefenseBoosted = false;
    public Coroutine isDefenseBoostedCoroutine;
    [SerializeField] public bool isDefenseBroken = false;
    public Coroutine isDefenseBrokenCoroutine;
    [SerializeField] public bool isSpeedBoosted = false;
    public Coroutine isSpeedBoostedCoroutine;
    [SerializeField] public bool isSpeedBroken = false;
    public Coroutine isSpeedBrokenCoroutine;
    [SerializeField] public bool isAccuracyBoosted = false;
    public Coroutine isAccuracyBoostedCoroutine;
    [SerializeField] public bool isAccuracyBroken = false;
    public Coroutine isAccuracyBrokenCoroutine;
    [SerializeField] public bool isCritRateBoosted = false;
    public Coroutine isCritRateBoostedCoroutine;
    [SerializeField] public bool isCritRateBroken = false;
    public Coroutine isCritRateBrokenCoroutine;
    [SerializeField] public bool isCritDamageBoosted = false;
    public Coroutine isCritDamageBoostedCoroutine;
    [SerializeField] public bool isCritDamageBroken = false;
    public Coroutine isCritDamageBrokenCoroutine;
    [SerializeField] public bool isResistenceBoosted = false;
    public Coroutine isResistenceBoostedCoroutine;
    [SerializeField] public bool isResistenceBroken = false;
    public Coroutine isResistenceBrokenCoroutine;

    public event Action<bool, UnitStatusController, UnitStatusController> onIsProtected;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsShield;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsStunned;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsFrozen;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsTrapped;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsMarked;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsUnLucky;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsLastStand;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsMadness;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsCursed;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsAttackBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsAttackBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsDefenseBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsDefenseBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsSpeedBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsSpeedBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsAccuracyBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsAccuracyBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsCritRateBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsCritRateBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsCritDamageBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsCritDamageBroken;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsResistenceBoosted;
    public event Action<bool, UnitStatusController, UnitStatusController> onIsResistenceBroken;

    [SerializeField] public int brandCount = 0;

    public UnitDTO dTO = new UnitDTO();

    private UnitInventory unitInventory;

    public static event Action<UnitStatusController> OnUnitStatusSpawned;
    public static event Action<UnitStatusController> OnUnitStatusDespawned;

    // Start is called before the first frame update
    void Start()
    {
        unitInventory = GetComponent<UnitInventory>();
        OnUnitStatusSpawned?.Invoke(this);
        GetComponent<UnitHealthController>().onDeath += UnitDead;
    }

    private void OnDestroy()
    {
        OnUnitStatusDespawned?.Invoke(this);
        GetComponent<UnitHealthController>().onDeath -= UnitDead;
    }

    public void LoadUnitStatusController(UnitDTO unitDTO, Database database, int tID = 0)
    {
        attackBase = unitDTO.attackBase;
        attackElementBase = unitDTO.attackElementBase;
        healthBase = unitDTO.healthBase;
        defenseBase = unitDTO.defenceBase;
        defenseElementBase = unitDTO.defenceElementBase;
        speedBase = unitDTO.speedBase;
        accuracyBase = unitDTO.accuracryBase;
        resistanceBase = unitDTO.resistanceBase;
        critRateBase = unitDTO.critRateBase;
        critDamageBase = unitDTO.critDamageBase;
        massBase = unitDTO.mass;
        teamID = tID;
        unitID = unitDTO.iD;
        unitName = unitDTO.name;

        rarity = unitDTO.rarity;
        race = unitDTO.raceID;
        level = unitDTO.level;
        rank = unitDTO.rank;

        unitClass = database.classTypes[unitDTO.classID];
        unitType1 = database.elementTypes[unitDTO.type1ID];
        unitType2 = database.elementTypes[unitDTO.type2ID];
        dTO = unitDTO;

    }

    private void UnitDead(bool x,UnitStatusController attacker, UnitStatusController defender)
    {
        unitDead = x;
    }

    public int GetBaseAttack() { return attackBase; }
    public int GetBaseAttackElement() { return attackElementBase; }
    public int GetBaseHealth() { return healthBase; }
    public int GetBaseDefense() { return defenseBase; }
    public int GetBaseDefenseElement() { return defenseElementBase; }
    public int GetBaseSpeed() { return speedBase; }
    public int GetBaseAccuracy() { return accuracyBase; }
    public int GetBaseResistance() { return resistanceBase; }
    public int GetBaseCritRate() { return critRateBase; }
    public int GetBaseCritDamage() { return critDamageBase; }
    public int GetBaseMass() { return massBase; }
    public bool IsUnitDead() { return unitDead; }
    public int GetTeamID() { return teamID; }
    public string GetUnitID() { return unitID; }
    public void ChangeTeamID(int x) { teamID = x; }


    public int GetAttack() { return attackModified; }
    public int GetAttackElement() { return attackElementModified; }
    public int GetHealth() { return healthModified; }
    public int GetDefense() { return defenseModified; }
    public int GetDefenseElement() { return defenseElementModified; }
    public int GetSpeed() { return speedModified; }
    public int GetAccuracy() { return accuracyModified; }
    public int GetResistance() { return resistanceModified; }
    public int GetCritRate() { return critRateModified; }
    public int GetCritDamage() { return critDamageModified; }
    public int GetMass() { return massModified; }


    public void BuildStats(List<BattleDeploymentStats> battleDeploymentStats)
    {
        attackModified = Mathf.RoundToInt(BuildAttackMod() * (1 + unitClass.attackBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.attackBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.attackBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        attackElementModified = Mathf.RoundToInt(BuildAttackElementMod() * (1 + unitClass.attackElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.attackElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.attackElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        healthModified = Mathf.RoundToInt(BuildHealthMod() * (1 + unitClass.healthBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.healthBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.healthBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        speedModified = Mathf.RoundToInt(BuildSpeedMod() * (1 + unitClass.speedBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.speedBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.speedBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        defenseModified = Mathf.RoundToInt(BuildDefenseMod() * (1 + unitClass.defenseBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.defenseBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.defenseBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        defenseElementModified = Mathf.RoundToInt(BuildDefenseElementMod() * (1 + unitClass.defenseElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.defenseElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.defenseElementBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        accuracyModified = Mathf.RoundToInt(BuildAccuracyMod() * (1 + unitClass.accuracyBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.accuracyBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.accuracyBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        resistanceModified = Mathf.RoundToInt(BuildResistanceMod() * (1 + unitClass.resistenceBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.resistenceBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.resistenceBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        critRateModified = Mathf.RoundToInt(BuildCritRateMod() * (1 + unitClass.critRateBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.critRateBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.critRateBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        critDamageModified = Mathf.RoundToInt(BuildCritDamageMod() * (1 + unitClass.critDamageBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.critDamageBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.critDamageBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
        massModified = Mathf.RoundToInt(BuildMassMod() * (1 + unitClass.massBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].classCount[unitClass.classTypeID] / 3), 3)] + unitType1.massBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType1.elementTypeID] / 3), 3)] + unitType2.massBuff[Mathf.Min(Mathf.FloorToInt(battleDeploymentStats[teamID].typeCount[unitType2.elementTypeID] / 3), 3)]));
    }

    public int BuildAttackMod()
    {
        return Mathf.RoundToInt(attackBase * (1 + (level * .02f) + (rank * .12f))) + unitInventory.GetEquipmentAttack();
    }

    public int BuildAttackElementMod()
    {
        return unitInventory.GetEquipmentAttackElement() + Mathf.RoundToInt(attackElementBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildHealthMod()
    {
        return unitInventory.GetEquipmentHealth() + Mathf.RoundToInt(healthBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildSpeedMod()
    {
        return unitInventory.GetEquipmentSpeed() + Mathf.RoundToInt(speedBase * ((1 + rank * .1f) + (level * .08f)));
    }

    public int BuildDefenseMod()
    {
        return unitInventory.GetEquipmentDefense() + Mathf.RoundToInt(defenseBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildDefenseElementMod()
    {
        return unitInventory.GetEquipmentDefenseElement() + Mathf.RoundToInt(defenseElementBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildAccuracyMod()
    {
        return unitInventory.GetEquipmentAccuracy() + Mathf.RoundToInt(accuracyBase * (1 + level * .02f + rank * .1f));
    }

    public int BuildResistanceMod()
    {
        return unitInventory.GetEquipmentResistence() + Mathf.RoundToInt(resistanceBase * (1 + level * .02f + rank * .1f));
    }

    public int BuildCritRateMod()
    {
        return unitInventory.GetEquipmentCritRate() + Mathf.RoundToInt(critRateBase * (1 + rank * .1f));
    }

    public int BuildCritDamageMod()
    {
        return unitInventory.GetEquipmentCritDamage() + Mathf.RoundToInt(critDamageBase * (1 + level * .02f + rank * .12f));
    }

    public int BuildMassMod()
    {
        return unitInventory.GetEquipmentmass() + Mathf.RoundToInt(massBase * (1 + rank * .1f));
    }

    public void SetIsProtected(bool x,UnitStatusController attacker)
    {
        isProtected = x;
        onIsProtected?.Invoke(x, attacker,this);
        if (!x)
        {
            StopCoroutine(isProtectedCoroutine);
        }
    }

    public void SetIsShield(bool x, UnitStatusController attacker)
    {
        isShield = x;
        onIsShield?.Invoke(x, attacker,this);
    }

    public void SetIsStunned(bool x, UnitStatusController attacker)
    {
        isStunned = x;
        onIsStunned?.Invoke(x, attacker,this);
    }

    public void SetIsFrozen(bool x, UnitStatusController attacker)
    {
        isFrozen = x;
        onIsFrozen?.Invoke(x, attacker,this);
    }

    public void SetIsTrapped(bool x, UnitStatusController attacker)
    {
        isTrapped = x;
        onIsTrapped?.Invoke(x, attacker,this);
    }

    public void SetIsMarked(bool x, UnitStatusController attacker)
    {
        isMarked = x;
        onIsMarked?.Invoke(x, attacker,this);
    }

    public void SetIsUnLucky(bool x, UnitStatusController attacker)
    {
        isUnLucky = x;
        onIsUnLucky?.Invoke(x, attacker,this);
    }

    public void SetIsLastStand(bool x, UnitStatusController attacker)
    {
        isLastStand = x;
        onIsLastStand?.Invoke(x, attacker,this);
    }

    public void SetIsMadness(bool x, UnitStatusController attacker)
    {
        isMadness = x;
        onIsMadness?.Invoke(x, attacker,this);
    }

    public void SetIsCursed(bool x, UnitStatusController attacker)
    {
        isCursed = x;
        onIsCursed?.Invoke(x, attacker,this);
    }

    public void SetIsAttackBoosted(bool x, UnitStatusController attacker)
    {
        isAttackBoosted = x;
        onIsAttackBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsAttackBroken(bool x, UnitStatusController attacker)
    {
        isAttackBroken = x;
        onIsAttackBroken?.Invoke(x, attacker,this);
    }

    public void SetIsDefenseBoosted(bool x, UnitStatusController attacker)
    {
        isDefenseBoosted = x;
        onIsDefenseBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsDefenseBroken(bool x, UnitStatusController attacker)
    {
        isDefenseBroken = x;
        onIsDefenseBroken?.Invoke(x, attacker,this);
    }

    public void SetIsSpeedBoosted(bool x, UnitStatusController attacker)
    {
        isSpeedBoosted = x;
        onIsSpeedBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsSpeedBroken(bool x, UnitStatusController attacker)
    {
        isSpeedBroken = x;
        onIsSpeedBroken?.Invoke(x, attacker,this);
    }

    public void SetIsAccuracyBoosted(bool x, UnitStatusController attacker)
    {
        isAccuracyBoosted = x;
        onIsAccuracyBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsAccuracyBroken(bool x, UnitStatusController attacker)
    {
        isAccuracyBroken = x;
        onIsAccuracyBroken?.Invoke(x, attacker,this);
    }

    public void SetIsCritRateBoosted(bool x, UnitStatusController attacker)
    {
        isCritRateBoosted = x;
        onIsCritRateBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsCritRateBroken(bool x, UnitStatusController attacker)
    {
        isCritRateBroken = x;
        onIsCritRateBroken?.Invoke(x, attacker,this);
    }

    public void SetIsCritDamageBoosted(bool x, UnitStatusController attacker)
    {
        isCritDamageBoosted = x;
        onIsCritDamageBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsCritDamageBroken(bool x, UnitStatusController attacker)
    {
        isCritDamageBroken = x;
        onIsCritDamageBroken?.Invoke(x, attacker,this);
    }

    public void SetIsResistenceBoosted(bool x, UnitStatusController attacker)
    {
        isResistenceBoosted = x;
        onIsResistenceBoosted?.Invoke(x, attacker,this);
    }

    public void SetIsResistenceBroken(bool x, UnitStatusController attacker)
    {
        isResistenceBroken = x;
        onIsResistenceBroken?.Invoke(x, attacker,this);
    }
}
