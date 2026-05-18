using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Single-target: the casting unit itself. Used for self-buffs and self-heals.</summary>
    [CreateAssetMenu(fileName = "GetSelfTarget", menuName = "CapsuleWars/Abilities/Targeting/Self", order = 41)]
    public class GetSelfTarget_SO : AbilityTargetingStrategy
    {
        public override void Collect(AbilityCastContext ctx, List<IUnitRef> output)
        {
            if (ctx.Source != null) output.Add(ctx.Source);
        }
    }
}
