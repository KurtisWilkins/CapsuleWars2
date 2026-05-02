using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UnitStatsCard : MonoBehaviour
{
    [SerializeField] public Text unitName;
    [SerializeField] public Text totalDamage;
    [SerializeField] public Text totalHealing;
    [SerializeField] public Text debuffsApplied;
    [SerializeField] public Text buffsApplied;
    [SerializeField] public Text faintedCount;
    [SerializeField] public Text koCount;
    [SerializeField] public Text helaerAward;
    [SerializeField] public Text damageAward;
    [SerializeField] public Text tankAward;
    [SerializeField] public Text koKingAward;
    [SerializeField] public Image totalHealingBar;
    [SerializeField] public Image totalDamageBar;

    public UnitDTO unitDTO;
    public BattleUnitStats unitStats;
    public BattleController battleController;
    [SerializeField] public UnitIcon unitIcon;

    [SerializeField] private Color goodHealthColor;
    [SerializeField] private Color okayHealthColor;
    [SerializeField] private Color badHealthColor;
    [SerializeField] private Color faintedHealthColor;

    public UnitStaticsDetailedBattlePopUp unitStaticsDetailedBattlePopUp;
    public string unitID;
    public int unitTeamID;

    List<Tweener> t = new List<Tweener>();
    TweenCallback tweenCallback;

    public static event Action<UnitSelectorButton> OnUnitSelected;

    public int buttonSoundDeploy = 1;
    public int buttonSoundRemove = 5;
    public int buttonSoundNotAllowed = 3;

    public void HydrateSelectorButton(UnitDTO _unitDTO, int teamID, BattleUnitStats _unitStats, BattleController _battleController, UnitStaticsDetailedBattlePopUp _unitStaticsDetailedBattlePopUp)
    {
        unitID = _unitStats.unitID;
        unitTeamID = teamID;
        unitName.text = _unitDTO.name;
        totalDamage.text = _unitStats.totalDamageDealt.ToString("N2");
        totalHealing.text = _unitStats.totalHealingDone.ToString("N2");
        debuffsApplied.text = _unitStats.debuffsApplied.ToString();
        buffsApplied.text = _unitStats.buffsApplied.ToString();
        koCount.text = _unitStats.totalKOs.ToString();
        faintedCount.text = _unitStats.revives.ToString();

        totalHealingBar.fillAmount = _unitStats.totalHealingDone / MathF.Max(1f, _battleController.unitBattleStats[teamID].totalHealingDoneMax);
        totalDamageBar.fillAmount = _unitStats.totalHealingDone / MathF.Max(1f, _battleController.unitBattleStats[teamID].totalHealingDoneMax);

        HandleBars(totalHealingBar);
        HandleBars(totalDamageBar);

        unitIcon.HydrateUnitIcon(_unitDTO, UnitSpawner.Instance.database);

        unitDTO = _unitDTO;
        unitStats = _unitStats;
        battleController = _battleController;
        unitStaticsDetailedBattlePopUp = _unitStaticsDetailedBattlePopUp;
        AwardButtonCheck(teamID, _unitStats, battleController);
    }


    public void HandleBars(Image bar)
    {
        if(bar.fillAmount < .3f)
        {
            bar.color = badHealthColor;
        }
        else if (bar.fillAmount < .6f)
        {
            bar.color = okayHealthColor;
        }
        else
        {
            bar.color = goodHealthColor;
        }
    }

    public void MoreStatsButtonClick()
    {
        unitStaticsDetailedBattlePopUp.gameObject.SetActive(true);
        unitStaticsDetailedBattlePopUp.LoadData(unitDTO, unitTeamID ,unitStats, battleController);
    }

    public void AwardButtonCheck(int teamID, BattleUnitStats _unitStats, BattleController battleController)
    {
        if (_unitStats.totalHealingDone >= battleController.unitBattleStats[teamID].totalHealingDoneMax && _unitStats.totalHealingDone != 0f)
        {
            AwardTweenStarter(helaerAward);
            helaerAward.gameObject.SetActive(true);
        }
        else
        {
            helaerAward.gameObject.SetActive(false);
        }

        if (_unitStats.totalDamageDealt >= battleController.unitBattleStats[teamID].totalDamageDealtMax && _unitStats.totalDamageDealt != 0f)
        {
            AwardTweenStarter(damageAward);
            damageAward.gameObject.SetActive(true);
        }
        else
        {
            damageAward.gameObject.SetActive(false);
        }

        if (_unitStats.totalDamageRecieved >= battleController.unitBattleStats[teamID].totalDamageRecievedMax && _unitStats.buffsApplied != 0)
        {
            AwardTweenStarter(tankAward);
            tankAward.gameObject.SetActive(true);
        }
        else
        {
            tankAward.gameObject.SetActive(false);
        }
        
        if (_unitStats.totalKOs >= battleController.unitBattleStats[teamID].totalKOsMax && _unitStats.totalKOs != 0)
        {
            AwardTweenStarter(koKingAward);
            koKingAward.gameObject.SetActive(true);
        }
        else
        {
            koKingAward.gameObject.SetActive(false);
        }
    }

    public void AwardTweenStarter(Text awardText)
    {
        t.Add(awardText.transform.DOPunchPosition(Vector3.up, 1f, 5, .5f, false).SetLoops(-1, LoopType.Restart));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    private void OnDestroy()
    {
        foreach (var tw in t)
        {
            tw.Kill();
        }
    }
}
