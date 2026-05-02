using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using I2.Loc;
using Unity.VisualScripting;

public class UnitDescriptionBattlePanel : MonoBehaviour
{
    [SerializeField] public RectTransform descriptionPanel;
    
    [SerializeField] public bool isOnScreen = false;

    [SerializeField] private float onScreenValue = -320f;
    [SerializeField] private float offScreenValue = 340f;

    [SerializeField] public UnitIcon warriorIcon;

    [SerializeField] public Image targetingIcon;
    [SerializeField] public Image basicAbilityIcon;
    [SerializeField] public Image passiveAbilityIcon;

    [SerializeField] public Text unitName;
    [SerializeField] public Text level;
    [SerializeField] public Text unitClass;
    [SerializeField] public Text rarity;
    [SerializeField] public Text race;
    [SerializeField] public Text element1;
    [SerializeField] public Text element2;

    [SerializeField] public Text attack;
    [SerializeField] public Text defence;
    [SerializeField] public Text attackElement;
    [SerializeField] public Text defenceElement;
    [SerializeField] public Text health;
    [SerializeField] public Text speed;
    [SerializeField] public Text accuracy;
    [SerializeField] public Text resistence;
    [SerializeField] public Text criticalRate;
    [SerializeField] public Text criticalDamage;
    [SerializeField] public Text mass;

    [SerializeField] public MovementTargeting targeting;
    [SerializeField] public Ability_SO baseAbility;
    [SerializeField] public Ability_SO passiveAbility;

    [SerializeField] public AbilityDescriptionPopup popup;

    public UnitMovementController currentSelected;
    public UnitMovementController currentTargeted;

    public void TriggerDescriptionPanel()
    {
        if (isOnScreen)
        {
            descriptionPanel.DOLocalMoveX(offScreenValue, .5f,true);
            isOnScreen = false;
            if (currentSelected != null)
            {
                currentSelected.ToggleOffHighlightSelected();
            }
            if (currentTargeted != null)
            {
                currentTargeted.ToggleOffHighlightTargeted();
            }
        }
        else
        {
            descriptionPanel.DOLocalMoveX(onScreenValue, .5f,true);
            isOnScreen = true;
        }
    }

    public void HydrateDescriptionPanel(UnitStatusController unitStatus, UnitMovementController unitMovement, UnitAttackController unitAttack, UnitHealthController unitHealth)
    {
        if(currentSelected != null)
        {
            currentSelected.ToggleOffHighlightSelected();
        }
        if(currentTargeted != null)
        {
            currentTargeted.ToggleOffHighlightTargeted();
        }
        
        warriorIcon.HydrateUnitIcon(unitStatus.dTO,UnitSpawner.Instance.database);
        
        unitName.text = unitStatus.unitName;
        level.text = unitStatus.level.ToString();
        unitClass.text = LocalizationManager.GetTranslation(unitStatus.unitClass.nameID);
        rarity.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.rarity[unitStatus.rarity].nameID);
        rarity.color = UnitSpawner.Instance.database.rarity[unitStatus.rarity].rarityTextColor;
        race.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.unitBodys[unitStatus.race].raceNameID);

        element1.text = LocalizationManager.GetTranslation(unitHealth.element1.nameID);
        element1.color = unitHealth.element1.elementTextColor;
        element2.text = LocalizationManager.GetTranslation(unitHealth.element2.nameID);
        element2.color = unitHealth.element2.elementTextColor;

        targeting = unitMovement.movementTargeting;
        targetingIcon.sprite = unitMovement.movementTargeting.icon;
        baseAbility = unitAttack.baseAbility;
        basicAbilityIcon.sprite = unitAttack.baseAbility.icon;
        passiveAbility = unitAttack.passiveAbility;
        passiveAbilityIcon.sprite = unitAttack.passiveAbility.icon;

        attack.text = unitStatus.GetAttack().ToString();
        defence.text = unitStatus.GetDefense().ToString();
        attackElement.text = unitStatus.GetAttackElement().ToString();
        defenceElement.text = unitStatus.GetDefenseElement().ToString();
        health.text = unitStatus.GetHealth().ToString();
        speed.text = unitStatus.GetSpeed().ToString();
        accuracy.text = unitStatus.GetAccuracy().ToString();
        resistence.text = unitStatus.GetResistance().ToString();
        criticalRate.text = unitStatus.GetCritRate().ToString();
        criticalDamage.text = unitStatus.GetCritDamage().ToString();
        mass.text = unitStatus.GetMass().ToString();

        currentSelected = unitMovement;
        currentSelected.ToggleOnHighlightSelected();
        currentTargeted = unitMovement.GetTargetForHighlight();
        if(currentTargeted != null)
        {
            currentTargeted.ToggleOnHighlightTargeted();
        }

    }

    public void FillDescription(GameObject unit)
    {
        if (!isOnScreen)
        {
            TriggerDescriptionPanel();
        }

        UnitStatusController unitStatus = unit.GetComponent<UnitStatusController>();
        UnitMovementController unitMovement = unit.GetComponent<UnitMovementController>();
        UnitAttackController unitAttack = unit.GetComponent<UnitAttackController>();
        UnitHealthController unitHealth = unit.GetComponent<UnitHealthController>();

        HydrateDescriptionPanel(unitStatus, unitMovement, unitAttack, unitHealth);
    }

    public void TargetingButtonClicked()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(targeting.movementNameID, targeting.descriptionID, targeting.icon);
    }

    public void BasicAbilityButtonClick()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(baseAbility.abilityNameID, baseAbility.descriptionID, baseAbility.icon);
    }

    public void PassiveAbilityButtonClick()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(passiveAbility.abilityNameID, passiveAbility.descriptionID, passiveAbility.icon);
    }

    public void RefreshTargeting()
    {
        if (currentSelected != null)
        {
            currentSelected.ToggleOnHighlightSelected();
            if (currentTargeted != null)
            {
                currentTargeted.ToggleOffHighlightTargeted();
            }
            currentTargeted = currentSelected.GetTargetForHighlight();
            if (currentTargeted != null)
            {
                currentTargeted.ToggleOnHighlightTargeted();
            }
        }
    }
}
