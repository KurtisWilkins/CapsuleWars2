using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GetAllTarget", menuName = "Abilities/Targeting/GetAllTargets", order = 0)]
public class GetAllTargets : AbilityTargetingStrategy
{
    public override void StartTargeting(AbilityData data)
    {
        data.targets.Clear();
        foreach (UnitStatusController unit in BattleController.deployedUnits)
        {
            data.targets.Add(unit);
        }
    }
}
