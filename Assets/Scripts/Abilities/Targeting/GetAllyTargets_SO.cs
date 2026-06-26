using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Living allied units (same team as the source), including the source itself (Docs/05).</summary>
    [CreateAssetMenu(fileName = "GetAllyTargets", menuName = "CapsuleWars/Abilities/Targeting/Allies", order = 42)]
    public class GetAllyTargets_SO : AbilityTargetingStrategy
    {
        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            var registry = CombatServices.Registry;
            if (registry == null || ctx.Source == null) return;

            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null || u.IsDowned) continue;
                if (u.Team != ctx.Source.Team) continue;
                output.Add(u);   // includes self per Docs/05
            }
        }
    }
}
