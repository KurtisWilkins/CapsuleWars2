using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>The source unit's current movement/attack target (Docs/05). Empty if it has none or it's downed.</summary>
    [CreateAssetMenu(fileName = "GetCurrentTarget", menuName = "CapsuleWars/Abilities/Targeting/Current Target", order = 43)]
    public class GetCurrentTarget_SO : AbilityTargetingStrategy
    {
        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            if (ctx.Source == null || ctx.Source.GameObject == null) return;
            var root = ctx.Source.GameObject.GetComponentInParent<UnitRoot>();
            var target = root != null && root.Movement != null ? root.Movement.CurrentTarget : null;
            if (target != null && !target.IsDowned) output.Add(target);
        }
    }
}
