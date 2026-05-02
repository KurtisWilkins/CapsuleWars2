using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I2.Loc;

public class RewardRecruitmentPanel : MonoBehaviour
{
    [SerializeField] public UnitIcon warriorIcon;
    [SerializeField] public Image targetingIcon;
    [SerializeField] public Image basicAbilityIcon;
    [SerializeField] public Image passiveAbilityIcon;

    [SerializeField] public Text level;
    [SerializeField] public Text unitClass;
    [SerializeField] public Text rarity;
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

    public MovementTargeting targeting;
    public Ability_SO basicAbility;
    public Ability_SO passiveAbility;

    [SerializeField] public AbilityDescriptionPopup popup;


    public void LoadRecruit(UnitStatusController unitStatus, UnitDTO unitDTO)//, Texture rawImage)
    {
        warriorIcon.HydrateUnitIcon(unitDTO, UnitSpawner.Instance.database);

        targeting = UnitSpawner.Instance.database.movementTargetings[unitDTO.targetingID];
        basicAbility = UnitSpawner.Instance.database.baseAbilities[unitDTO.basicID];
        passiveAbility = UnitSpawner.Instance.database.passiveAbilities[unitDTO.passiveID];

        targetingIcon.sprite = targeting.icon;
        basicAbilityIcon.sprite = basicAbility.icon;
        passiveAbilityIcon.sprite = passiveAbility.icon;

        level.text = unitDTO.level.ToString();
        unitClass.text = LocalizationManager.GetTranslation(UnitSpawner.Instance.database.classTypes[unitDTO.classID].nameID);
        //rarity.text = unitDTO.rarity.ToString();
        UnitSpawner.Instance.database.rarity[unitDTO.rarity].SetTextForRarity(rarity);
        UnitSpawner.Instance.database.elementTypes[unitDTO.type1ID].SetTextForElement(element1);
        UnitSpawner.Instance.database.elementTypes[unitDTO.type2ID].SetTextForElement(element2);

        attack.text = string.Format("{0:#,0}", unitDTO.attackBase).ToString();
        attackElement.text = string.Format("{0:#,0}", unitDTO.attackElementBase).ToString();
        defence.text = string.Format("{0:#,0}", unitDTO.defenceBase).ToString();
        defenceElement.text = string.Format("{0:#,0}", unitDTO.defenceElementBase).ToString();
        health.text = string.Format("{0:#,0}", unitDTO.healthBase).ToString();
        speed.text = string.Format("{0:#,0}", unitDTO.speedBase).ToString();
        accuracy.text = string.Format("{0:#,0}", unitDTO.accuracryBase).ToString();
        resistence.text = string.Format("{0:#,0}", unitDTO.resistanceBase).ToString();
        criticalRate.text = string.Format("{0:#,0}", unitDTO.critRateBase).ToString();
        criticalDamage.text = string.Format("{0:#,0}", unitDTO.critDamageBase).ToString();
        mass.text = string.Format("{0:#,0}", unitDTO.mass).ToString();
    }

    public void TargetingButtonClicked()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(targeting.movementNameID, targeting.descriptionID, targeting.icon);
    }

    public void BasicAbilityButtonClick()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(basicAbility.abilityNameID, basicAbility.descriptionID, basicAbility.icon);
    }

    public void PassiveAbilityButtonClick()
    {
        popup.gameObject.SetActive(true);
        popup.ChangePopUP(passiveAbility.abilityNameID, passiveAbility.descriptionID, passiveAbility.icon);
    }
}
