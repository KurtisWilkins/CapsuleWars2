using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;
using System;
using DG.Tweening;
using I2.Loc;
using Unity.VisualScripting;

public class UnitSelectorButton : MonoBehaviour
{
    [SerializeField] public Text unitName;
    [SerializeField] public Text unitLevel;
    [SerializeField] public Text unitClass;
    [SerializeField] public Text unitRarity;
    [SerializeField] public Text unitRace;
    [SerializeField] public Text element1;
    [SerializeField] public Text element2;
    [SerializeField] public Text unitDead;
    [SerializeField] public Image currentHealth;
    [SerializeField] public Image currentXP;

    [SerializeField] public UnitDTO unitDTO;
    public BattleUnitStats battleUnitStats;

    [SerializeField] public UnitIcon unitIcon;

    [SerializeField] private Color goodHealthColor;
    [SerializeField] private Color okayHealthColor;
    [SerializeField] private Color badHealthColor;
    [SerializeField] private Color faintedHealthColor;

    [SerializeField] private Button button;

    public int unitID;
    public bool isDeployed = false;
    bool dead = false;
    bool won = false;
    int excessExp = 0;

    List<Tweener> t = new List<Tweener>();
    TweenCallback tweenCallback;

    public static event Action<UnitSelectorButton> OnUnitSelected;

    public int buttonSoundDeploy = 1;
    public int buttonSoundRemove = 5;
    public int buttonSoundNotAllowed = 3;

    [SerializeField] public SkillWindowManger skillWindowManger;

    private void Start()
    {
        DeploymentGridButton.OnDeployUnit += UnitDeployed;
    }

    private void OnDestroy()
    {
        DeploymentGridButton.OnDeployUnit -= UnitDeployed;
        BattleEndController.onExpReward -= IncreaseExp;
        foreach (var l in t)
        {
            l.Kill();
        }
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void HydrateSelectorButton(UnitDTO unitDTO, int x)//, Texture texture)
    {
        unitID = x;
        unitName.text = unitDTO.name;
        unitLevel.text = unitDTO.level.ToString();
        unitClass.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.classTypes[unitDTO.classID].nameID);
        unitRarity.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.rarity[unitDTO.rarity].nameID);
        unitRarity.color = UnitSpawner.Instance.database.rarity[unitDTO.rarity].rarityTextColor;
        unitRace.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.unitBodys[unitDTO.raceID].raceNameID);

        element1.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.elementTypes[unitDTO.type1ID].nameID);
        element1.color = UnitSpawner.Instance.database.elementTypes[unitDTO.type1ID].elementTextColor;
        element2.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.elementTypes[unitDTO.type2ID].nameID);
        element2.color = UnitSpawner.Instance.database.elementTypes[unitDTO.type2ID].elementTextColor;

        currentHealth.fillAmount = unitDTO.currentHealthPrecent;
        currentXP.fillAmount = unitDTO.currentExpPrecent;

        unitIcon.HydrateUnitIcon(unitDTO, UnitSpawner.Instance.database);

        if(unitDTO.currentHealthPrecent < .3f)
        {
            currentHealth.color = badHealthColor;
        }
        else if (unitDTO.currentHealthPrecent < .6f)
        {
            currentHealth.color = okayHealthColor;
        }
        else
        {
            currentHealth.color = goodHealthColor;
        }

        //unitIconRaw.texture = texture;

        if(unitDTO.currentHealthPrecent == 0)
        {
            //unitIconRaw.color = faintedHealthColor;
            button.interactable = false;
        }
        //UnitSpawner.Instance.BuildIcon(unitDTO,unitIconRaw);
        //unitIcon.HydrateUnitIcon(unitDTO, UnitSpawner.Instance.database);
    }

    public void SelectThisUnit()
    {
        if (isDeployed)
        {
            AudioManager.instance.PlaySFX(buttonSoundNotAllowed);
            return;
        }
        AudioManager.instance.PlaySFX(buttonSoundDeploy);
        OnUnitSelected?.Invoke(this);
    }

    public void UnitDeployed(DeploymentGridButton x)
    {
        if(x.unitDeployedID == unitID)
        {
            isDeployed = !isDeployed;
            CheckButtonStatus();
        }
    }

    public void CheckButtonStatus()
    {
        button.interactable = !isDeployed;
    }

    public void LoadUnitExpPanel(UnitDTO _unitDTO, UnitDTO saveDTO, BattleUnitStats unitStats, bool wonBattle)//, Texture texture)
    {
        button.enabled = false;
        battleUnitStats = unitStats;
        won = wonBattle;

        if (_unitDTO.currentHealthPrecent == 0)
        {
            unitDead.gameObject.SetActive(true);
            dead = true;

            unitIcon.MarkDead();

            t.Add(unitDead.transform.DOPunchPosition(Vector3.up, 1f, 5, .5f, false).SetLoops(-1, LoopType.Restart));
        }
        currentHealth.fillAmount = _unitDTO.currentHealthPrecent;
        currentXP.fillAmount = _unitDTO.currentExpPrecent;
        unitDTO = _unitDTO;
        HydrateSelectorButton(unitDTO, 0);

        BattleEndController.onExpReward += IncreaseExp;
        //UnitLevelUP levelUP = new UnitLevelUP();
        
        //saveDTO = _unitDTO;
    }

    public void IncreaseExp(int e)
    {
        UnitLevelUP levelUP = new UnitLevelUP();
        UnitDTO afterExp = levelUP.LevelUP(unitDTO, e);

        //Debug.Log("Leve Before: " + unitDTO.level + " Level After: " + afterExp.level);
        if (afterExp.level > unitDTO.level)
        {
            currentXP.DOFillAmount(1f, .5f);

            StartCoroutine(LevelUpVisuals(afterExp));


        }
        else
        {
            currentXP.DOFillAmount(afterExp.currentExpPrecent, .5f);
        }

        unitDTO.level = afterExp.level;
        unitDTO.currentExpPrecent = afterExp.currentExpPrecent;
        foreach (UnitBattlePlacement uBP in PlayerData.unitsInParty)
        {
            if (uBP.UnitDTO.iD == unitDTO.iD)
            {
                uBP.UnitDTO.level = unitDTO.level;
                uBP.UnitDTO.currentExpPrecent = unitDTO.currentExpPrecent;
                uBP.UnitDTO.currentHealthPrecent = unitDTO.currentHealthPrecent;
                uBP.UnitDTO.RecordUnitStats(battleUnitStats,won,dead);
                if (UnitSpawner.Instance.database.evolutions[unitDTO.evolutionStratID].EvolutionCheck(uBP.UnitDTO, battleUnitStats) && unitDTO.evolution < 2)
                {
                    uBP.UnitDTO.evolution++;
                    Debug.Log("You need to implement visuals here for Evolution!!!!");
                }
            }
        }

        
    }

    public IEnumerator LevelUpVisuals(UnitDTO dTO)
    {
        //currentXP.DOFillAmount(1f, .5f);
        yield return new WaitForSeconds(.55f);
        unitLevel.text = dTO.level.ToString();
        t.Add(unitLevel.transform.DOPunchPosition(Vector3.up*10, .2f, 5, .5f, false).SetLoops(-1, LoopType.Restart));
        currentXP.fillAmount = 0;
        currentXP.DOFillAmount(dTO.currentExpPrecent, .5f);
    }

    public void WindowSkillsSelectThisUnit()
    {
        AudioManager.instance.PlaySFX(buttonSoundDeploy);
        skillWindowManger.SelectThisUnit(unitID);
    }
}
