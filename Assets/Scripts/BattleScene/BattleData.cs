using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleData 
{
    [SerializeField] public int expReward;
    [SerializeField] public bool hasGoldReward;
    [SerializeField] public int goldReward;
    [SerializeField] public bool hasEquipmentReward;
    public EquipmentDTO _equipmentReward = new EquipmentDTO();
    [SerializeField] public EquipmentDTO equipmentReward
    {
        get
        {
            return _equipmentReward;
        }
        set
        {
            _equipmentReward = value;
        }
    }
    [SerializeField] public bool hasUnitReward;
    [SerializeField] public UnitDTO unitReward;
    [SerializeField] public List<UnitBattlePlacement> enemyarmy = new List<UnitBattlePlacement>();
    [SerializeField] public bool isTrainer = false;
    [SerializeField] public string trainerID;
    [SerializeField] public bool hasQuest = false;
    [SerializeField] public Quest_SO QuestID;
    [SerializeField] public bool hasEvent = false;
    [SerializeField] public string eventID;
    [SerializeField] public BattleFeild_SO battleMap;
    [SerializeField] public BattleWeather_SO battleWeather;


    public void SaveEquiptmentReward(/*EquipmentDTO equipmentReward*/)
    {
        //EquipmentDTO e = new EquipmentDTO();
        //e = equipmentReward;
        //equipmentReward = e;
    }

}
