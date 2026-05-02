using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardEquipmentPanel : MonoBehaviour
{
    [SerializeField] public Image equipmentIcon;
    [SerializeField] public Image runeIcon;
    [SerializeField] public TextMeshProUGUI level;
    [SerializeField] public TextMeshProUGUI slot;
    [SerializeField] public TextMeshProUGUI rarity;

    [SerializeField] public TextMeshProUGUI attack;
    [SerializeField] public TextMeshProUGUI defence;
    [SerializeField] public TextMeshProUGUI attackElement;
    [SerializeField] public TextMeshProUGUI defenceElement;
    [SerializeField] public TextMeshProUGUI health;
    [SerializeField] public TextMeshProUGUI speed;
    [SerializeField] public TextMeshProUGUI accuracy;
    [SerializeField] public TextMeshProUGUI resistence;
    [SerializeField] public TextMeshProUGUI criticalRate;
    [SerializeField] public TextMeshProUGUI criticalDamage;
    [SerializeField] public TextMeshProUGUI mass;

    [SerializeField] public List<Image> stars = new List<Image>(); 
    
    
    public void LoadEquipment(EquipmentDTO equipment)
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

        for(int i = 0; i < equipment.equiptmentGradeIndex; i++)
        {
            stars[i].gameObject.SetActive(true);
        }
    }
}
