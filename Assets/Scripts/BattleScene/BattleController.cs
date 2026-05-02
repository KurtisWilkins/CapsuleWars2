using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class BattleController : MonoBehaviour
{
    public static List<UnitStatusController> deployedUnits = new List<UnitStatusController>();
    public List<UnitStatusController> deployedUnitsLook;
    public BattleUnitSelectorMovement selectorMovement;
    public CaptureButtonMovement captureButtonMovement;
    public BattleGridMovement gridMovement;
    
    public static event Action<bool> onBattleStarted;
    public static event Action<bool> onBattleEnded;
    public static event Action<float> onBattleStatus;
    public static event Action onStatsChange;
    
    [SerializeField] public bool battleStarted = false;
    [SerializeField] public bool battleEnded = false;
    [SerializeField] public bool captureTrigger = false;
    [SerializeField] public bool isTrainerBattle = false;
    [SerializeField] public bool isVictory = false;
    [SerializeField] public List<BattleDeploymentStats> battleDeploymentStats = new List<BattleDeploymentStats>();
    [SerializeField] public List<BattleStats> unitBattleStats = new List<BattleStats>();

    [SerializeField] public GameObject endPanel;

    [SerializeField] public BattleData battleDataCheck = new BattleData();
    [SerializeField] public UnitStatusController captureUnit;
    [SerializeField] public GameObject captureProjectile;
    [SerializeField] public List<AbilityEffectsStrategy> effectStrategies = new List<AbilityEffectsStrategy>();
    [SerializeField] public Transform spawnPoint;
    [SerializeField] public UnitDescriptionBattlePanel unitDescriptionBattlePanel;
    [SerializeField] public BattleUnitStatsUIHolder battleUnitStatsUIHolder; 
    [SerializeField] public GameObject bonusIconPref;
    [SerializeField] public Transform transformFriendly;
    [SerializeField] public Transform transformEnemy;
    [SerializeField] public AbilityDescriptionPopup popup;
    [SerializeField] public Transform startBattleButton;
    [SerializeField] public Transform hidePanelButton;

    private void Awake()
    {
        deployedUnits.Clear();
        //intialize the stats list
        battleDeploymentStats.Add(new BattleDeploymentStats(UnitSpawner.Instance.database));
        battleDeploymentStats.Add(new BattleDeploymentStats(UnitSpawner.Instance.database));
        CreateBonusIcons(battleDeploymentStats[0], transformFriendly);
        CreateBonusIcons(battleDeploymentStats[1], transformEnemy);
        unitBattleStats.Add(new BattleStats());
        unitBattleStats.Add(new BattleStats());
    }


    // Start is called before the first frame update
    void Start()
    {
        Instantiate(PlayerData.battleData.battleMap.battleGround);
        if(PlayerData.battleData.battleWeather.battleWeatherID != 0)
        {
            Instantiate(PlayerData.battleData.battleWeather.battleWeather);
        }

        UnitStatusController.OnUnitStatusSpawned += AddUnitToDeployedList;
        UnitStatusController.OnUnitStatusDespawned += RemoveUnitToDeployedList;
        UnitStatusController.OnUnitStatusSpawned += AddUnitToStatsList;
        UnitStatusController.OnUnitStatusDespawned += RemoveUnitToStatsList;
        battleDataCheck = PlayerData.battleData;
        endPanel.SetActive(false);
        //Debug.Log("army size is " + PlayerData.battleData.enemyarmy.Count);
        isTrainerBattle = PlayerData.battleData.isTrainer;
        DeployEnemyArmy();
        StartCoroutine(waitForSound());
        ScreenFade.instance.FadeFromBlack();
        StartCoroutine(DelayHUDStart());
    }

    private void OnDestroy()
    {
        UnitStatusController.OnUnitStatusSpawned -= AddUnitToDeployedList;
        UnitStatusController.OnUnitStatusDespawned -= RemoveUnitToDeployedList;
        UnitStatusController.OnUnitStatusSpawned -= AddUnitToStatsList;
        UnitStatusController.OnUnitStatusDespawned -= RemoveUnitToStatsList;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (battleStarted && !battleEnded)
        {
            CheckBattleStatus();
        }
    }

    public void EndBattle()
    {
        StartCoroutine(LeaveBattleScene());
    }

    public void StartBattle(bool x)
    {
        onBattleStarted?.Invoke(x);
        deployedUnitsLook = deployedUnits;
        TriggerBonusIconMovement();
    }

    public void CaptureUnit()
    {
        if (true)
        {
            onBattleStarted?.Invoke(false);
            //get projectile Data
            AbilityData data = new AbilityData(captureUnit,captureUnit,false,captureUnit.unitType1);
            data.battleController = this;
            //Get Projectile
            GameObject effect = Instantiate(captureProjectile, gameObject.transform);
            effect.transform.position = spawnPoint.position;
            Projectile projectile = effect.GetComponent<Projectile>();
            projectile.LoadProjectileData(effectStrategies, data, captureUnit.gameObject);
        }
    }
    
    public void TransferUnitToParty()
    {
        captureUnit.dTO.currentHealthPrecent = captureUnit.gameObject.GetComponent<UnitHealthController>().GetNewCurrentHealth();
        //UnitDTO unit = new UnitDTO();
        //unit = 
        PlayerData.battleData.unitReward = captureUnit.dTO;
        PlayerData.battleData.hasUnitReward = true;
    }

    public void ButtonToStartLeave()
    {
        //if (battleStarted)
        //{
        //    EndBattle();
        //}
        //else
        //{
            StartBattle(!battleStarted);
            battleStarted = !battleStarted;
        //}
    }

    public void AddUnitToDeployedList(UnitStatusController unit)
    {
        deployedUnits.Add(unit);
        AddUnitDeploymentStats(unit);
        unit.GetComponent<UnitHealthController>().onHealthChange += UnitDamaged;
        unit.GetComponent<UnitHealthController>().onDeath += UnitsKOed;
        unit.GetComponent<UnitHealthController>().onRevive += UnitsRevived;
        unit.onIsDefenseBoosted += DefenseBoosted;
        unit.onIsDefenseBroken += DefenseBroken;
        unit.onIsAttackBroken += AttackBroken;
        unit.onIsAttackBoosted += AttackBoosted;
    }

    public void RemoveUnitToDeployedList(UnitStatusController unit)
    {
        deployedUnits.Remove(unit);
        RemoveUnitDeploymentStats(unit);
        unit.GetComponent<UnitHealthController>().onHealthChange -= UnitDamaged;
        unit.GetComponent<UnitHealthController>().onDeath -= UnitsKOed;
        unit.GetComponent<UnitHealthController>().onRevive -= UnitsRevived;
        unit.onIsDefenseBoosted -= DefenseBoosted;
        unit.onIsDefenseBroken -= DefenseBroken;
        unit.onIsAttackBroken -= AttackBroken;
        unit.onIsAttackBoosted -= AttackBoosted;
    }

    public void AddUnitToStatsList(UnitStatusController unit)
    {
        BattleUnitStats bST = new BattleUnitStats();
        bST.unitID = unit.GetUnitID();
        unitBattleStats[unit.GetTeamID()].battleUnitStats.Add(bST);
        battleUnitStatsUIHolder.SpawnBar(0 == unit.GetTeamID(), unit, bST, this);
        //Debug.Log("Unit Stats Class count" + unitBattleStats[unit.GetTeamID()].battleUnitStats.Count);
    }

    public void RemoveUnitToStatsList(UnitStatusController unit)
    {
        for (int i = 0; i < unitBattleStats[unit.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[unit.GetTeamID()].battleUnitStats[i].unitID == unit.GetUnitID())
            {
                battleUnitStatsUIHolder.Despawn(unit);
                unitBattleStats[unit.GetTeamID()].battleUnitStats.RemoveAt(i);
            }
        }
        //Debug.Log("Unit Stats Class count" + unitBattleStats[unit.GetTeamID()].battleUnitStats.Count);
    }

    void UnitDamaged(float x, UnitStatusController dealer, UnitStatusController Reciever)
    {
        if (x > 0)
        {
            for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
                {
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalDamageDealt += x;
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].attackCountDealt++;
                    if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalDamageDealt > unitBattleStats[dealer.GetTeamID()].totalDamageDealtMax)
                    {
                        unitBattleStats[dealer.GetTeamID()].totalDamageDealtMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalDamageDealt;
                    }
                }

            }
            for (int i = 0; i < unitBattleStats[Reciever.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].unitID == Reciever.GetUnitID())
                {
                    unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalDamageRecieved += x;
                    unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].attackCountRecieved++;
                    if (unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalDamageRecieved > unitBattleStats[Reciever.GetTeamID()].totalDamageRecievedMax)
                    {
                        unitBattleStats[Reciever.GetTeamID()].totalDamageRecievedMax = unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalDamageRecieved;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
                {
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalHealingDone += -x;
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].healingCountDealt++;
                    if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalHealingDone < unitBattleStats[dealer.GetTeamID()].totalHealingDoneMax)
                    {
                        unitBattleStats[dealer.GetTeamID()].totalHealingDoneMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalHealingDone;
                    }
                }
            }
            for (int i = 0; i < unitBattleStats[Reciever.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].unitID == Reciever.GetUnitID())
                {
                    unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalHealingRecieved += -x;
                    unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].healingCountRecieved++;
                    if (unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalHealingRecieved > unitBattleStats[Reciever.GetTeamID()].totalHealingRecievedMax)
                    {
                        unitBattleStats[Reciever.GetTeamID()].totalHealingRecievedMax = unitBattleStats[Reciever.GetTeamID()].battleUnitStats[i].totalHealingRecieved;
                    }
                }
            }
        }
        if (!battleEnded)
        {
            onStatsChange?.Invoke();
        }
    }

    void UnitsKOed(bool x,UnitStatusController dealer, UnitStatusController reciever)
    {
        if (x)
        {
            for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
                {
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalKOs++;
                    if(unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalKOs > unitBattleStats[dealer.GetTeamID()].totalKOsMax)
                    {
                        unitBattleStats[dealer.GetTeamID()].totalKOsMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].totalKOs;
                    }
                }
            }
            for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
                {
                    unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].faints++;
                    if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].faints > unitBattleStats[reciever.GetTeamID()].faintsMax)
                    {
                        unitBattleStats[reciever.GetTeamID()].faintsMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].faints;
                    }
                }
            }
        }
    }

    void UnitsRevived(bool x,UnitStatusController dealer,UnitStatusController reciever)
    {
        if (x)
        {
            for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
                {
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].revives += 1;
                    if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].revives > unitBattleStats[dealer.GetTeamID()].revivesMax)
                    {
                        unitBattleStats[dealer.GetTeamID()].revivesMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].revives;
                    }
                }
            }
            for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
                {
                    unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].revived++;
                    if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].revived > unitBattleStats[reciever.GetTeamID()].revivedMax)
                    {
                        unitBattleStats[reciever.GetTeamID()].revivedMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].revived;
                    }
                }
            }
        }
    }

    public void AttackBroken(bool x, UnitStatusController dealer, UnitStatusController reciever)
    {
        for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
                {
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBrokenCountApplied++;
                    unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].debuffsApplied++;
                    if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBrokenCountApplied > unitBattleStats[dealer.GetTeamID()].isAttackBrokenCountAppliedMax)
                    {
                        unitBattleStats[dealer.GetTeamID()].isAttackBrokenCountAppliedMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBrokenCountApplied;
                    }
                }
            }
            for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
            {
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
                {
                    unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBrokenCountRecieved++;
                    unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].debuffsReceived++;
                    if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBrokenCountRecieved > unitBattleStats[reciever.GetTeamID()].isAttackBrokenCountRecievedMax)
                    {
                        unitBattleStats[reciever.GetTeamID()].isAttackBrokenCountRecievedMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBrokenCountRecieved;
                    }
                }
            }
    }

    public void AttackBoosted(bool x, UnitStatusController dealer, UnitStatusController reciever)
    {
        for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
            {
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBoostedCountApplied++;
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].buffsApplied++;
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBoostedCountApplied > unitBattleStats[dealer.GetTeamID()].isAttackBoostedCountAppliedMax)
                {
                    unitBattleStats[dealer.GetTeamID()].isAttackBoostedCountAppliedMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isAttackBoostedCountApplied;
                }
            }
        }
        for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
            {
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBoostedCountRecieved++;
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].buffsReceived++;
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBoostedCountRecieved > unitBattleStats[reciever.GetTeamID()].isAttackBoostedCountRecievedMax)
                {
                    unitBattleStats[reciever.GetTeamID()].isAttackBoostedCountRecievedMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isAttackBoostedCountRecieved;
                }
            }
        }
    }

    public void DefenseBroken(bool x, UnitStatusController dealer, UnitStatusController reciever)
    {
        for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
            {
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountApplied++;
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].debuffsApplied++;
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountApplied > unitBattleStats[dealer.GetTeamID()].isDefenseBrokenCountAppliedMax)
                {
                    unitBattleStats[dealer.GetTeamID()].isDefenseBrokenCountAppliedMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountApplied;
                }
            }
        }
        for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
            {
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountRecieved++;
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].debuffsReceived++;
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountRecieved > unitBattleStats[reciever.GetTeamID()].isDefenseBrokenCountRecievedMax)
                {
                    unitBattleStats[reciever.GetTeamID()].isDefenseBrokenCountRecievedMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBrokenCountRecieved;
                }
            }
        }
    }

    public void DefenseBoosted(bool x, UnitStatusController dealer, UnitStatusController reciever)
    {
        for (int i = 0; i < unitBattleStats[dealer.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].unitID == dealer.GetUnitID())
            {
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountApplied++;
                unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].buffsApplied++;
                if (unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountApplied > unitBattleStats[dealer.GetTeamID()].isDefenseBoostedCountAppliedMax)
                {
                    unitBattleStats[dealer.GetTeamID()].isDefenseBoostedCountAppliedMax = unitBattleStats[dealer.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountApplied;
                }
            }
        }
        for (int i = 0; i < unitBattleStats[reciever.GetTeamID()].battleUnitStats.Count; i++)
        {
            if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].unitID == reciever.GetUnitID())
            {
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountRecieved++;
                unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].buffsReceived++;
                if (unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountRecieved > unitBattleStats[reciever.GetTeamID()].isDefenseBoostedCountRecievedMax)
                {
                    unitBattleStats[reciever.GetTeamID()].isDefenseBoostedCountRecievedMax = unitBattleStats[reciever.GetTeamID()].battleUnitStats[i].isDefenseBoostedCountRecieved;
                }
            }
        }
    }

    public void CheckBattleStatus()
    {
        int ally = 0;
        int foe = 0;

        foreach (UnitStatusController unit in deployedUnits)
        {
            if (!unit.IsUnitDead())
            {
                if (unit.GetTeamID() == 0)
                {
                    ally++;
                }
                else
                {
                    foe++;
                    captureUnit = unit;
                }
            }
        }

        onBattleStatus?.Invoke((ally / (foe + ally)));

        if (ally == 0 && foe > 0)
        {
            battleEnded = true;
            onBattleEnded?.Invoke(true);
            EndBattleUpdateStats();

        }
        if (foe == 0 && ally >= 0)
        {
            battleEnded = true;
            isVictory = true;
            onBattleEnded?.Invoke(true);
            EndBattleUpdateStats();
        }

        if (foe == 1 && !captureTrigger && !isTrainerBattle)
        {
            //Debug.Log("Trigger Capture Button");
            captureButtonMovement.TriggerTween();
            captureTrigger = true;
        }
    }


    public void DeployEnemyArmy()
    {
        for(int i = 0; i < PlayerData.battleData.enemyarmy.Count; i++)
        {
            if(PlayerData.battleData.enemyarmy[i].UnitDTO == null) { return; }
            UnitSpawner.Instance.SpawnUnit(PlayerData.battleData.enemyarmy[i].UnitDTO,
                new Vector3(PlayerData.battleData.enemyarmy[i].xPosition*.75f, PlayerData.battleData.enemyarmy[i].yPosition*.75f),
                new Vector3(0, 0, 0), PlayerData.battleData.enemyarmy[i].teamIndex);
        }
    }

    public void AddUnitDeploymentStats(UnitStatusController unit)
    {
        if (battleEnded) { return; }
        if (battleDeploymentStats.Count >= unit.GetTeamID())
        {

            battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]++;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]++;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]++;
            if (battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }
        }
        else
        {
            //battleDeploymentStats.Add(new BattleDeploymentStats(UnitSpawner.Instance.database));
            battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]++;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]++;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]++;
            if (battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }
        }
        unitDescriptionBattlePanel.RefreshTargeting();

        StartCoroutine(CalcStatsDelay());
    }

    public void RemoveUnitDeploymentStats(UnitStatusController unit)
    {
        if (battleEnded) { return; }
        if(battleDeploymentStats.Count >= unit.GetTeamID())
        {

            battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]--;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]--;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]--;
            if(battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }
        }
        else
        {
            //battleDeploymentStats.Add(new BattleDeploymentStats(UnitSpawner.Instance.database));
            battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]--;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]--;
            battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]--;
            if (battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].classCount[unit.unitClass.classTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitClass.classTypeID];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType1.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType1.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }

            if (battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID] > 0)
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(true);
                b.ChangeCount(battleDeploymentStats[unit.GetTeamID()].typeCount[unit.unitType2.elementTypeID]);
            }
            else
            {
                BonusIcon b = battleDeploymentStats[unit.GetTeamID()].iconHolder[unit.unitType2.elementTypeID + battleDeploymentStats[unit.GetTeamID()].classCount.Count];
                b.gameObject.SetActive(false);
            }
        }

        unitDescriptionBattlePanel.RefreshTargeting();
        if (!battleEnded)
        {
            StartCoroutine(CalcStatsDelay());
        }
    }

    public IEnumerator CalcStatsDelay()
    {
        yield return new WaitForSeconds(.01f);
        CalcStats();
    }

    public void CalcStats()
    {
        foreach(var unit in deployedUnits)
        {
            unit.BuildStats(battleDeploymentStats);
        }
    }

    public void EndBattleUpdateStats()
    {
        CheckTrianerAndQusetUpdate();
        captureButtonMovement.gameObject.SetActive(false);
        for(int i = 0; i < deployedUnits.Count; i++)
        {
            for(int j = 0; j < PlayerData.unitsInParty.Count; j++)
            {
                if(PlayerData.unitsInParty[j].teamIndex == deployedUnits[i].GetTeamID() && PlayerData.unitsInParty[j].UnitDTO.iD == deployedUnits[i].GetUnitID())
                {
                    PlayerData.unitsInParty[j].UnitDTO.currentHealthPrecent = deployedUnits[i].GetComponent<UnitHealthController>().GetNewCurrentHealth();
                }
            }
        }
        StartCoroutine(BuildEndPanelDelay());
    }

    public IEnumerator BuildEndPanelDelay()
    {
        yield return new WaitForSeconds(1.5f);
        BuildEndPanel();
    }


    public void BuildEndPanel()
    {
        endPanel.SetActive(true);
        endPanel.transform.DOPunchScale(new Vector3(.1f,.1f,.1f), .5f);
        endPanel.GetComponent<BattleEndController>().LoadEndBattlePanel(isVictory);
    }

    IEnumerator LeaveBattleScene()
    {
        foreach (var unit in deployedUnits)
        {
            //RemoveUnitToStatsList(unit);
            RemoveUnitToDeployedList(unit);
        }
        yield return new WaitForSeconds(1.5f);
        //TransitionController.instance.ChangeScene("ID", 0);
    }

    public IEnumerator waitForSound()
    {
        //Wait Until Sound has finished playing
        while (AudioManager.instance.bgm[PlayerData.battleMusicIntro].isPlaying)
        {
            yield return null;
        }

        //Auidio has finished playing, disable GameObject
        AudioManager.instance.PlayBGM(PlayerData.battleMusic);
    }

    public void PlayButtonSound(int buttonSound)
    {
        AudioManager.instance.PlaySFX(buttonSound);
    }

    public void TransistionBackDelay()
    {
        StartCoroutine(TransistionBack());
    }

    public IEnumerator TransistionBack()
    {
        battleUnitStatsUIHolder.gameObject.SetActive(false);
        DOTween.KillAll();
        ScreenFade.instance.gameObject.SetActive(true);
        ScreenFade.instance.FadeToBlack();
        yield return new WaitForSeconds(1f);
        PlayerController.instance.gameObject.SetActive(true);
        PlayerController.instance.battleOver();
        GameMenu.instance.ToggleTouchControls();
        SceneManager.LoadScene(PlayerData.sceneName);
    }

    public IEnumerator DelayHUDStart()
    {
        yield return new WaitForSeconds(2.5f);
        ScreenFade.instance.gameObject.SetActive(false);
        selectorMovement.TriggerDescriptionPanel();
        gridMovement.TriggerDescriptionPanel();
        
    }

    public void EndBattleOnCapture()
    {
        battleEnded = true;
        isVictory = true;
        onBattleEnded?.Invoke(true);
        EndBattleUpdateStats();
    }

    public void CheckTrianerAndQusetUpdate()
    {
        if (PlayerData.battleData.isTrainer)
        {
            if (isVictory)
            {
                PlayerPrefs.SetInt("TrainerMarker_" + PlayerData.battleData.trainerID, 1);
                Debug.Log("Set Id to 1");
            }
        }
        if (PlayerData.battleData.hasQuest)
        {
            if (isVictory)
            {
                //PlayerPrefs.SetInt("TrainerMarker_" + PlayerData.battleData.trainerID, 1);
                Debug.Log("remeber to update the quest after battle " + PlayerData.battleData.QuestID);
            }
        }
    }

    public void CreateBonusIcons(BattleDeploymentStats battleDeploymentStats, Transform parent)
    {
        for(int i = 0; i < battleDeploymentStats.classCount.Count; i++)
        {
            GameObject p = Instantiate(bonusIconPref, parent);
            BonusIcon bi = p.GetComponent<BonusIcon>();
            UnitClass_SO unitClass_SO = UnitSpawner.Instance.database.classTypes[i];
            bi.SetData(unitClass_SO.classidTypeIconSmall, i, false, popup);
            battleDeploymentStats.iconHolder.Add(bi);
            bi.gameObject.SetActive(false);
        }
        for (int i = 0; i < battleDeploymentStats.typeCount.Count; i++)
        {
            GameObject p = Instantiate(bonusIconPref, parent);
            BonusIcon bi = p.GetComponent<BonusIcon>();
            ElementType_SO unitClass_SO = UnitSpawner.Instance.database.elementTypes[i];
            bi.SetData(unitClass_SO.elementTypeIcon, i, true, popup);
            battleDeploymentStats.iconHolder.Add(bi);
            bi.gameObject.SetActive(false);
        }
    }

    public void TriggerBonusIconMovement()
    {
        transformFriendly.DOLocalMoveY(900, .95f, true);
        transformEnemy.DOLocalMoveY(900, .95f, true);
        startBattleButton.DOLocalMoveX(2100, .95f, true);
        hidePanelButton.DOLocalMoveX(-2100, .95f, true);
    }
}
