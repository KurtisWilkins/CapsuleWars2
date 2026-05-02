using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleDeploymentStats
{
    [SerializeField] public int teamID;

    [SerializeField] public List<int> classCount = new List<int>();
    [SerializeField] public List<int> typeCount = new List<int>();
    [SerializeField] public List<BonusIcon> iconHolder = new List<BonusIcon>();

    public BattleDeploymentStats(Database database)
    {
        foreach (var d in database.classTypes)
        {
            classCount.Add(0);
        }

        foreach (var d in database.elementTypes)
        {
            typeCount.Add(0);
        }
    }
}
