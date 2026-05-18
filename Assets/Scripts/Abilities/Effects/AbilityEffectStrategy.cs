using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Applies an effect to the surviving target list. Effects compose:
    /// an ability can chain Damage → Knockback → ApplyStatus, etc.
    /// Effects are responsible for any per-target VFX or events they fire.
    /// </summary>
    public abstract class AbilityEffectStrategy : ScriptableObject
    {
        public abstract void Apply(AbilityCastContext ctx, IReadOnlyList<IUnitRef> targets);
    }
}
