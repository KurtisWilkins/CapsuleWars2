using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.StatusEffects
{
    // The five damage-pipeline status behaviors (Docs/10). Authored as assets in BTS-D and referenced from the
    // matching StatusEffect_SO via its behaviorSO field. Numbers are first-pass / tunable.

    /// <summary>Marked — takes extra damage from ALL sources.</summary>
    [CreateAssetMenu(fileName = "Behavior_Marked", menuName = "CapsuleWars/Status Effects/Behaviors/Marked")]
    public class MarkedBehavior : StatusEffectBehavior
    {
        [SerializeField, Min(0f)] private float extraDamagePercent = 25f;
        public override int ModifyIncomingDamage(StatusDamageContext ctx, int amount)
            => Mathf.RoundToInt(amount * (1f + extraDamagePercent / 100f));
    }

    /// <summary>Frozen — physical damage taken is amplified (elemental/true pass through).</summary>
    [CreateAssetMenu(fileName = "Behavior_Frozen", menuName = "CapsuleWars/Status Effects/Behaviors/Frozen")]
    public class FrozenBehavior : StatusEffectBehavior
    {
        [SerializeField, Min(1f)] private float physicalMultiplier = 1.5f;
        public override int ModifyIncomingDamage(StatusDamageContext ctx, int amount)
            => ctx.Kind == DamageKind.Physical ? Mathf.RoundToInt(amount * physicalMultiplier) : amount;
    }

    /// <summary>Protected — the next incoming hit is fully negated, then the status is consumed (idempotent).</summary>
    [CreateAssetMenu(fileName = "Behavior_Protected", menuName = "CapsuleWars/Status Effects/Behaviors/Protected")]
    public class ProtectedBehavior : StatusEffectBehavior
    {
        public override int ModifyIncomingDamage(StatusDamageContext ctx, int amount)
        {
            ctx.Consume = true;
            return 0;
        }
    }

    /// <summary>Shield — absorbs flat damage from a pool (BehaviorValue) before HP; removed when the pool is spent.</summary>
    [CreateAssetMenu(fileName = "Behavior_Shield", menuName = "CapsuleWars/Status Effects/Behaviors/Shield")]
    public class ShieldBehavior : StatusEffectBehavior
    {
        public override int ModifyIncomingDamage(StatusDamageContext ctx, int amount)
        {
            if (ctx.BehaviorValue <= 0f) return amount;
            int absorbed = Mathf.Min(amount, Mathf.RoundToInt(ctx.BehaviorValue));
            ctx.BehaviorValue -= absorbed;
            if (ctx.BehaviorValue <= 0f) ctx.Consume = true;
            return amount - absorbed;
        }
    }

    /// <summary>LastStand — below an HP threshold, incoming damage is reduced. (The +Atk half is a conditional
    /// stat buff handled elsewhere; this is only the damage-reduction hook.)</summary>
    [CreateAssetMenu(fileName = "Behavior_LastStand", menuName = "CapsuleWars/Status Effects/Behaviors/LastStand")]
    public class LastStandBehavior : StatusEffectBehavior
    {
        [SerializeField, Range(0f, 1f)] private float hpThreshold = 0.2f;
        [SerializeField, Range(0f, 1f)] private float damageTakenMultiplier = 0.5f;
        public override int ModifyIncomingDamage(StatusDamageContext ctx, int amount)
            => ctx.TargetHpFraction < hpThreshold ? Mathf.RoundToInt(amount * damageTakenMultiplier) : amount;
    }
}
