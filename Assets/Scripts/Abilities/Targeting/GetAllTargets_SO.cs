using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Every living unit on the battlefield — both teams, including the source (Docs/05).</summary>
    [CreateAssetMenu(fileName = "GetAllTargets", menuName = "CapsuleWars/Abilities/Targeting/All", order = 41)]
    public class GetAllTargets_SO : AbilityTargetingStrategy
    {
        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            var registry = CombatServices.Registry;
            if (registry == null) return;

            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null || u.IsDowned) continue;
                output.Add(u);
            }
        }
    }
}
