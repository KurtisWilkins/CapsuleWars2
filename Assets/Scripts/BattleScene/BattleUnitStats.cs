using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class BattleUnitStats
{
    public string unitID;
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
    //Statuses
    public int isProtectedCountRecieved = 0;
    public int isShieldCountRecieved = 0;
    public int isStunnedCountRecieved = 0;
    public int isFrozenCountRecieved = 0;
    public int isTrappedCountRecieved = 0;
    public int isMarkedCountRecieved = 0;
    public int isUnLuckyCountRecieved = 0;
    public int isLastStandCountRecieved = 0;
    public int isMadnessCountRecieved = 0;
    public int isCursedCountRecieved = 0;
    public int isAttackBoostedCountRecieved = 0;
    public int isAttackBrokenCountRecieved = 0;
    public int isDefenseBoostedCountRecieved = 0;
    public int isDefenseBrokenCountRecieved = 0;
    public int isSpeedBoostedCountRecieved = 0;
    public int isSpeedBrokenCountRecieved = 0;
    public int isAccuracyBoostedCountRecieved = 0;
    public int isAccuracyBrokenCountRecieved = 0;
    public int isCritRateBoostedCountRecieved = 0;
    public int isCritRateBrokenCountRecieved = 0;
    public int isCritDamageBoostedCountRecieved = 0;
    public int isCritDamageBrokenCountRecieved = 0;
    public int isResistenceBoostedCountRecieved = 0;
    public int isResistenceBrokenCountRecieved = 0;

    public int brandedCountRecieved = 0;

    public int isProtectedCountApplied = 0;
    public int isShieldCountApplied = 0;
    public int isStunnedCountApplied = 0;
    public int isFrozenCountApplied = 0;
    public int isTrappedCountApplied = 0;
    public int isMarkedCountApplied = 0;
    public int isUnLuckyCountApplied = 0;
    public int isLastStandCountApplied = 0;
    public int isMadnessCountApplied = 0;
    public int isCursedCountApplied = 0;
    public int isAttackBoostedCountApplied = 0;
    public int isAttackBrokenCountApplied = 0;
    public int isDefenseBoostedCountApplied = 0;
    public int isDefenseBrokenCountApplied = 0;
    public int isSpeedBoostedCountApplied = 0;
    public int isSpeedBrokenCountApplied = 0;
    public int isAccuracyBoostedCountApplied = 0;
    public int isAccuracyBrokenCountApplied = 0;
    public int isCritRateBoostedCountApplied = 0;
    public int isCritRateBrokenCountApplied = 0;
    public int isCritDamageBoostedCountApplied = 0;
    public int isCritDamageBrokenCountApplied = 0;
    public int isResistenceBoostedCountApplied = 0;
    public int isResistenceBrokenCountApplied = 0;

    public int brandedCountApplied = 0;

    public BattleUnitStats()
    {

    }
}

public class BattleStats
{
    public List<BattleUnitStats> battleUnitStats = new List<BattleUnitStats>();

    public float totalDamageDealtMax = 0f;
    public float totalDamageRecievedMax = 0f;
    public float totalHealingDoneMax = 0f;
    public float totalHealingRecievedMax = 0f;
    public int totalKOsMax = 0;
    public int revivesMax = 0;
    public int revivedMax = 0;
    public int faintsMax = 0;
    public int buffsAppliedMax = 0;
    public int debuffsAppliedMax = 0;
    public int buffsReceivedMax = 0;
    public int debuffsReceivedMax = 0;
    //StaMaxtuses
    public int isProtectedCountAppliedMax = 0;
    public int isShieldCountAppliedMax = 0;
    public int isStunnedCountAppliedMax = 0;
    public int isFrozenCountAppliedMax = 0;
    public int isTrappedCountAppliedMax = 0;
    public int isMarkedCountAppliedMax = 0;
    public int isUnLuckyCountAppliedMax = 0;
    public int isLastStandCountAppliedMax = 0;
    public int isMadnessCountAppliedMax = 0;
    public int isCursedCountAppliedMax = 0;
    public int isAttackBoostedCountAppliedMax = 0;
    public int isAttackBrokenCountAppliedMax = 0;
    public int isDefenseBoostedCountAppliedMax = 0;
    public int isDefenseBrokenCountAppliedMax = 0;
    public int isSpeedBoostedCountAppliedMax = 0;
    public int isSpeedBrokenCountAppliedMax = 0;
    public int isAccuracyBoostedCountAppliedMax = 0;
    public int isAccuracyBrokenCountAppliedMax = 0;
    public int isCritRateBoostedCountAppliedMax = 0;
    public int isCritRateBrokenCountAppliedMax = 0;
    public int isCritDamageBoostedCountAppliedMax = 0;
    public int isCritDamageBrokenCountAppliedMax = 0;
    public int isResistenceBoostedCountAppliedMax = 0;
    public int isResistenceBrokenCountAppliedMax = 0;
    public int brandedCountAppliedMax = 0;
    public int isProtectedCountRecievedMax = 0;
    public int isShieldCountRecievedMax = 0;
    public int isStunnedCountRecievedMax = 0;
    public int isFrozenCountRecievedMax = 0;
    public int isTrappedCountRecievedMax = 0;
    public int isMarkedCountRecievedMax = 0;
    public int isUnLuckyCountRecievedMax = 0;
    public int isLastStandCountRecievedMax = 0;
    public int isMadnessCountRecievedMax = 0;
    public int isCursedCountRecievedMax = 0;
    public int isAttackBoostedCountRecievedMax = 0;
    public int isAttackBrokenCountRecievedMax = 0;
    public int isDefenseBoostedCountRecievedMax = 0;
    public int isDefenseBrokenCountRecievedMax = 0;
    public int isSpeedBoostedCountRecievedMax = 0;
    public int isSpeedBrokenCountRecievedMax = 0;
    public int isAccuracyBoostedCountRecievedMax = 0;
    public int isAccuracyBrokenCountRecievedMax = 0;
    public int isCritRateBoostedCountRecievedMax = 0;
    public int isCritRateBrokenCountRecievedMax = 0;
    public int isCritDamageBoostedCountRecievedMax = 0;
    public int isCritDamageBrokenCountRecievedMax = 0;
    public int isResistenceBoostedCountRecievedMax = 0;
    public int isResistenceBrokenCountRecievedMax = 0;
    public int brandedCountRecievedMax = 0;

    public BattleStats()
    {

    }
}
