using System.Collections.Generic;
using CapsuleWars.Core;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Per-ability runtime state on a unit. Holds the cooldown timer (via
    /// <see cref="LastCastTime"/>) that triggers consult, plus the
    /// weapon-locked flag set at battle start when the unit's equipped
    /// weapon doesn't satisfy the ability's required classes.
    /// </summary>
    public class AbilityRuntime
    {
        public Ability_SO Ability { get; }
        public IUnitRef Source { get; }
        public bool IsLocked { get; set; }
        public float LastCastTime { get; private set; } = float.MinValue;

        private readonly List<IUnitRef> candidatesBuffer = new();

        public AbilityRuntime(Ability_SO ability, IUnitRef source)
        {
            Ability = ability;
            Source = source;
        }

        /// <summary>Returns true and casts the ability if its trigger fires; false otherwise.</summary>
        public bool Tick(float time)
        {
            if (IsLocked || Ability == null) return false;
            if (Ability.Trigger == null || Ability.Targeting == null) return false;

            var ctx = new AbilityCastContext(Source, Ability);
            if (!Ability.Trigger.ShouldFire(ctx, this, time)) return false;

            Cast(ctx);
            LastCastTime = time;
            return true;
        }

        private void Cast(AbilityCastContext ctx)
        {
            candidatesBuffer.Clear();
            Ability.Targeting.Collect(ctx, candidatesBuffer);

            var filters = Ability.Filters;
            if (filters != null)
            {
                for (int i = 0; i < filters.Count; i++)
                {
                    if (filters[i] != null) filters[i].Filter(ctx, candidatesBuffer);
                    if (candidatesBuffer.Count == 0) return;
                }
            }

            var effects = Ability.Effects;
            if (effects == null) return;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null) effects[i].Apply(ctx, candidatesBuffer);
            }
        }
    }
}
