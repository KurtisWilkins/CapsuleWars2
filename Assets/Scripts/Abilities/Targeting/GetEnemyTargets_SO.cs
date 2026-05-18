using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>All living enemy units (different team, not downed).</summary>
    [CreateAssetMenu(fileName = "GetEnemyTargets", menuName = "CapsuleWars/Abilities/Targeting/Enemies", order = 40)]
    public class GetEnemyTargets_SO : AbilityTargetingStrategy
    {
        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            var registry = CombatServices.Registry;
            if (registry == null || ctx.Source == null) return;

            var units = registry.Units;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (u == null) continue;
                if (ReferenceEquals(u, ctx.Source)) continue;
                if (u.Team == ctx.Source.Team) continue;
                if (u.IsDowned) continue;
                output.Add(u);
            }
        }
    }
}
