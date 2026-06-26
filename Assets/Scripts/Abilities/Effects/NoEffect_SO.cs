using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Null op — for "trigger only" abilities that exist for their trigger side effects (Docs/05).</summary>
    [CreateAssetMenu(fileName = "NoEffect", menuName = "CapsuleWars/Abilities/Effects/None", order = 69)]
    public class NoEffect_SO : AbilityEffectStrategy
    {
        public override void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets) { }
    }
}
