using System.Collections.Generic;

namespace CapsuleWars.Core
{
    /// <summary>
    /// A [code] behavioral class-synergy effect (Docs/09) — the non-StatBuff half of a synergy tier.
    /// Granted to same-class units when their tier is active; applied by the unit's <see cref="ISynergyBehaviorSink"/>.
    /// Magnitude meaning is per-kind. Extend this enum (+ the sink's handling) as more behaviors are wired.
    /// </summary>
    public enum SynergyEffectKind
    {
        /// <summary>Heal magnitude% of MaxHp when this unit scores a kill (Barbarian "heal 5% on kill").</summary>
        HealOnKill = 0,
        /// <summary>Heal magnitude% of MaxHp when this unit deals damage (Monk "heal on hit").</summary>
        HealOnHit = 1,
    }

    [System.Serializable]
    public struct SynergyEffect
    {
        public SynergyEffectKind kind;
        public float magnitude;   // percent of MaxHp for the heal kinds
    }

    /// <summary>
    /// Implemented by the component that runs a unit's active class-synergy behaviors (AbilityController).
    /// SynergyResolver pushes the active effect set each recompute through this Core seam, so Combat needn't
    /// reference the Abilities assembly.
    /// </summary>
    public interface ISynergyBehaviorSink
    {
        void SetSynergyEffects(IReadOnlyList<SynergyEffect> effects);
    }
}
