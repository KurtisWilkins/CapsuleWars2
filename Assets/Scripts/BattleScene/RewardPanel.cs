using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanel : MonoBehaviour
{
    [SerializeField] public Text goldAmount;
    [SerializeField] public Image equipmentIcon;
    [SerializeField] public Image runeIcon;
    [SerializeField] public Text level;
    [SerializeField] public Text slot;
    [SerializeField] public Text rarity;

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

    [SerializeField] public List<Image> stars = new List<Image>();

    public void LoadReward(EquipmentDTO equipment, int x)
    {
        equipmentIcon.sprite = UnitSpawner.Instance.database.unitEquipment[equipment.equiptmentSOID].equipmentSpriteR[0].equipmentSprites[0];
        runeIcon.sprite = UnitSpawner.Instance.database.runes[equipment.runeID].runeIcon;
        level.text = equipment.equiptmentLevel.ToString();
        slot.text = UnitSpawner.Instance.database.unitEquipment[equipment.equiptmentSOID].equipmentSlot.ToString();
        rarity.text = equipment.equiptmentRarityIndex.ToString();

        attack.text = string.Format("{0:#,0}", equipment.attack).ToString();
        attackElement.text = string.Format("{0:#,0}", equipment.attackElement).ToString();
        defence.text = string.Format("{0:#,0}", equipment.defense).ToString();
        defenceElement.text = string.Format("{0:#,0}", equipment.defenseElement).ToString();
        health.text = string.Format("{0:#,0}", equipment.health).ToString();
        speed.text = string.Format("{0:#,0}", equipment.speed).ToString();
        accuracy.text = string.Format("{0:#,0}", equipment.accuracy).ToString();
        resistence.text = string.Format("{0:#,0}", equipment.resistence).ToString();
        criticalRate.text = string.Format("{0:#,0}", equipment.critrate).ToString();
        criticalDamage.text = string.Format("{0:#,0}", equipment.critDamage).ToString();
        mass.text = string.Format("{0:#,0}", equipment.mass).ToString();

        for (int i = 0; i < equipment.equiptmentGradeIndex; i++)
        {
            stars[i].gameObject.SetActive(true);
        }

        goldAmount.text = "+" + string.Format("{0:#,0}", x);
    }
}
