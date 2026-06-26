using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// If the source has a current movement target and it is among the candidates, keep only it (Docs/05:
    /// "if attacker has a movement target, prefer it"). No current target, or it isn't a candidate → no change.
    /// </summary>
    [CreateAssetMenu(fileName = "KeepCurrentTargetFilter", menuName = "CapsuleWars/Abilities/Filters/Keep Current Target", order = 54)]
    public class KeepCurrentTargetFilter_SO : AbilityFilterStrategy
    {
        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            var root = ctx.Source != null && ctx.Source.GameObject != null
                ? ctx.Source.GameObject.GetComponentInParent<UnitRoot>() : null;
            var current = root != null && root.Movement != null ? root.Movement.CurrentTarget : null;
            if (current == null) return;

            if (candidates.Contains(current))
            {
                candidates.Clear();
                candidates.Add(current);
            }
        }
    }
}
