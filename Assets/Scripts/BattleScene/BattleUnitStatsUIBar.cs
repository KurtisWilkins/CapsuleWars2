using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitStatsUIBar : MonoBehaviour
{
    [SerializeField] public Text unitName;
    [SerializeField] public Text rank;
    [SerializeField] public Text statNumber;
    [SerializeField] public Image bar;
    public UnitStatusController unitStatusController;
    BattleUnitStats unitStats;
    BattleController battleController;
    int metricId;
    float statnum;

    public BattleUnitStatsUIBar LoadData(UnitStatusController _unitStatusController, BattleUnitStats _unitStats, BattleController _battleController, Color barColor, int metricNum)
    {
        unitStatusController = _unitStatusController;
        unitStats = _unitStats;
        battleController = _battleController;
        metricId = metricNum;

        unitName.text = _unitStatusController.dTO.name;
        rank.text = 0.ToString();
        statNumber.text = 0.ToString();
        bar.color = barColor;
        bar.fillAmount = 1;

        BattleController.onStatsChange += UpdateUI;
        UnitStatusController.OnUnitStatusDespawned += UnitWasDespawned;
        BattleController.onBattleEnded += BattleEnded;
        return this;
    }

    public void UpdateUI()
    {
        if(gameObject == null){ return; }
        if (1 == metricId)
        {
            statnum = unitStats.totalDamageDealt;
            statNumber.text = unitStats.totalDamageDealt.ToString("N2");
            bar.fillAmount = (unitStats.totalDamageDealt / battleController.unitBattleStats[unitStatusController.teamID].totalDamageDealtMax);
        }
        else if (2 == metricId)
        {
            statnum = unitStats.totalDamageRecieved;
            statNumber.text = unitStats.totalDamageRecieved.ToString("N2");
            bar.fillAmount = (unitStats.totalDamageRecieved / battleController.unitBattleStats[unitStatusController.teamID].totalDamageRecievedMax);
        }
        else if (3 == metricId)
        {
            statnum = unitStats.totalHealingDone;
            statNumber.text = unitStats.totalHealingDone.ToString("N2");
            bar.fillAmount = (unitStats.totalHealingDone / battleController.unitBattleStats[unitStatusController.teamID].totalHealingDoneMax);
        }
    }

    public void RankChange(int x)
    {
        rank.text = x.ToString();
    }

    void OnDestroy()
    {
        BattleController.onStatsChange -= UpdateUI;
        DOTween.Kill(gameObject.GetComponent<RectTransform>());
        UnitStatusController.OnUnitStatusDespawned -= UnitWasDespawned;
        BattleController.onBattleEnded -= BattleEnded;
    }

    public void UnitWasDespawned(UnitStatusController unit)
    {
        if (unit.dTO.iD == unitStatusController.dTO.iD)
        {
            BattleController.onStatsChange -= UpdateUI;
            DOTween.Kill(gameObject.GetComponent<RectTransform>());
            UnitStatusController.OnUnitStatusDespawned -= UnitWasDespawned;
            BattleController.onBattleEnded -= BattleEnded;
        }
    }

    public void BattleEnded(bool x)
    {
        if (x)
        {
            BattleController.onStatsChange -= UpdateUI;
            DOTween.Kill(gameObject.GetComponent<RectTransform>());
            UnitStatusController.OnUnitStatusDespawned -= UnitWasDespawned;
            BattleController.onBattleEnded -= BattleEnded;
        }
    }

    public int StatID()
    {
        return metricId;
    }

    public float StatValue()
    {
        return statnum;
    }
}
