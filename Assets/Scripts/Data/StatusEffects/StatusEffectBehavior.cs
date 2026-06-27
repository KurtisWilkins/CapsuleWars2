using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.StatusEffects
{
    /// <summary>
    /// Optional custom logic for a <see cref="StatusEffect_SO"/> (Docs/10) — the "complex status mechanism".
    /// Hooks the damage pipeline: <c>UnitStatusController.ModifyIncomingDamage</c> consults each active effect's
    /// behavior to adjust a hit BEFORE HP is reduced (Marked +25%, Frozen ×1.5 physical, Protected negate,
    /// Shield absorb, LastStand reduction).
    ///
    /// Behaviors are STATELESS shared assets; per-unit mutable state (e.g. a Shield's remaining absorb) lives on
    /// the active instance and is threaded through <see cref="StatusDamageContext.BehaviorValue"/>. Layer note:
    /// this lives in Data (below Units), so the context carries only Core/Data types — never a Units reference.
    /// </summary>
    public abstract class StatusEffectBehavior : ScriptableObject
    {
        /// <summary>
        /// Modify an incoming damage <paramref name="amount"/> on the affected unit. Return the new amount;
        /// set <see cref="StatusDamageContext.Consume"/> = true to remove this status after the hit
        /// (Protected, a fully-spent Shield).
        /// </summary>
        public virtual int ModifyIncomingDamage(StatusDamageContext ctx, int amount) => amount;
    }

    /// <summary>Mutable context handed to <see cref="StatusEffectBehavior.ModifyIncomingDamage"/>. Core/Data types only.</summary>
    public class StatusDamageContext
    {
        /// <summary>Who dealt the damage (may be null).</summary>
        public IUnitRef Source;
        /// <summary>Physical / Elemental / True.</summary>
        public DamageKind Kind;
        /// <summary>Affected unit's CurrentHp / MaxHp BEFORE this hit (for LastStand).</summary>
        public float TargetHpFraction;
        /// <summary>In/out per-instance state — e.g. a Shield's remaining absorb pool. Written back to the active effect.</summary>
        public float BehaviorValue;
        /// <summary>Out: the behavior asks to remove this status after the hit.</summary>
        public bool Consume;
    }
}
