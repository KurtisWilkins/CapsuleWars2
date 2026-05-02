using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New KeepCurrentTargetFilter", menuName = "Abilities/Filtering/KeepCurrentTarget", order = 0)]
public class KeepCurrentTargetFilter : AbilityFilterStrategy
{
    public override void StartFilter(AbilityData data)
    {
        if(data.primaryTarget != null)
        {
            data.targets.Clear();
            data.targets.Add(data.primaryTarget);
        }
    }
}
