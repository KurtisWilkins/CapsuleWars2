using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Owns a unit's stat block and its active status effects.
    /// Modified stat getters fold in stat-buff modifiers from active
    /// effects each call. M6 will layer in equipment + class synergies
    /// on top; the public surface stays the same.
    /// </summary>
    public class UnitStatusController : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField, Min(1)] private int baseMaxHp = 100;
        [SerializeField, Min(0)] private int baseAtk = 20;
        [SerializeField, Min(0)] private int baseDef = 5;
        [SerializeField, Min(0f)] private float baseSpeed = 3.5f;

        [Header("Element")]
        [Tooltip("Primary element of the unit. Drives element multiplier on dealt/received damage.")]
        [SerializeField] private ElementType_SO primaryElement;

        public ElementType_SO PrimaryElement => primaryElement;

        public int MaxHp => GetModifiedStat(StatType.MaxHp, baseMaxHp);
        public int Atk => GetModifiedStat(StatType.Atk, baseAtk);
        public int Def => GetModifiedStat(StatType.Def, baseDef);
        public float Speed => GetModifiedStatF(StatType.Speed, baseSpeed);

        // -----------------------------------------------------------------
        // Active status effects
        // -----------------------------------------------------------------

        private readonly List<ActiveStatusEffect> active = new();
        public IReadOnlyList<ActiveStatusEffect> ActiveEffects => active;

        public event Action<StatusEffect_SO> OnStatusApplied;
        public event Action<StatusEffect_SO> OnStatusExpired;

        public bool CannotAct => HasFlag(static e => e.PreventsAction);
        public bool CannotMove => HasFlag(static e => e.PreventsMovement);
        public bool CannotUseAbilities => HasFlag(static e => e.PreventsAbilities) || CannotAct;

        /// <summary>
        /// Try to apply a status effect to this unit. Respects resistance,
        /// stacking, and idempotence per the effect's configuration.
        /// </summary>
        public void ApplyStatus(StatusEffect_SO effect, IUnitRef source)
        {
            if (effect == null) return;

            if (effect.Resistance == ResistanceCheck.RollOnApply)
            {
                // Placeholder roll: M5 uses a coin-flip until accuracy stats
                // are wired into the StatCalculator in M6.
                if (UnityEngine.Random.value < 0.0f) return;
            }

            // Stacking
            switch (effect.StackBehavior)
            {
                case StackBehavior.Refresh:
                {
                    var existing = FindActive(effect);
                    if (existing != null)
                    {
                        existing.RemainingDuration = effect.DefaultDuration;
                        return;
                    }
                    break;
                }
                case StackBehavior.Add:
                {
                    var existing = FindActive(effect);
                    if (existing != null)
                    {
                        float cap = effect.DefaultDuration * effect.MaxStacks;
                        existing.RemainingDuration = Mathf.Min(cap, existing.RemainingDuration + effect.DefaultDuration);
                        return;
                    }
                    break;
                }
                case StackBehavior.Independent:
                    // Always add a new instance, no merging.
                    break;
            }

            active.Add(new ActiveStatusEffect(effect, source, effect.DefaultDuration));
            OnStatusApplied?.Invoke(effect);
        }

        public void ClearStatus(StatusEffect_SO effect)
        {
            if (effect == null) return;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i].Effect == effect)
                {
                    active.RemoveAt(i);
                    OnStatusExpired?.Invoke(effect);
                }
            }
        }

        public void ClearAllStatuses()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var e = active[i].Effect;
                active.RemoveAt(i);
                OnStatusExpired?.Invoke(e);
            }
        }

        // -----------------------------------------------------------------
        // Tick
        // -----------------------------------------------------------------

        private UnitHealthController health;

        private void Awake()
        {
            health = GetComponent<UnitHealthController>();
        }

        private void Update()
        {
            if (CombatServices.Phase != BattlePhase.Active) return;
            TickStatuses(Time.deltaTime);
        }

        private void TickStatuses(float dt)
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var e = active[i];

                // DoT / HoT
                if (e.Effect.TickAmount != 0)
                {
                    e.TickAccum += dt;
                    while (e.TickAccum >= e.Effect.TickInterval)
                    {
                        e.TickAccum -= e.Effect.TickInterval;
                        ApplyTick(e);
                    }
                }

                // Duration (skip if -1 = permanent)
                if (e.Effect.DefaultDuration > 0f)
                {
                    e.RemainingDuration -= dt;
                    if (e.RemainingDuration <= 0f)
                    {
                        var expired = e.Effect;
                        active.RemoveAt(i);
                        OnStatusExpired?.Invoke(expired);
                    }
                }
            }
        }

        private void ApplyTick(ActiveStatusEffect e)
        {
            if (health == null) return;
            int amount = e.Effect.TickAmount;
            if (e.Effect.TickIsPercentOfMaxHp)
            {
                amount = Mathf.RoundToInt(MaxHp * (amount / 100f));
            }
            if (amount < 0)
            {
                // DoT — damage
                health.TakeDamage(-amount, e.Source);
            }
            else if (amount > 0)
            {
                // HoT — heal
                int newHp = Mathf.Min(MaxHp, health.CurrentHp + amount);
                float ratio = (float)newHp / Mathf.Max(1, MaxHp);
                health.RestoreToPercent(ratio);
            }
        }

        // -----------------------------------------------------------------
        // Stat math
        // -----------------------------------------------------------------

        private int GetModifiedStat(StatType type, int baseValue)
        {
            ComputeMods(type, out float flatMod, out float percentMod);
            int result = baseValue + Mathf.RoundToInt(flatMod) + Mathf.RoundToInt(baseValue * percentMod / 100f);
            return Mathf.Max(0, result);
        }

        private float GetModifiedStatF(StatType type, float baseValue)
        {
            ComputeMods(type, out float flatMod, out float percentMod);
            float result = baseValue + flatMod + baseValue * percentMod / 100f;
            return Mathf.Max(0f, result);
        }

        private void ComputeMods(StatType type, out float flatMod, out float percentMod)
        {
            flatMod = 0f;
            percentMod = 0f;
            for (int i = 0; i < active.Count; i++)
            {
                var buffs = active[i].Effect.StatBuffs;
                if (buffs == null) continue;
                for (int j = 0; j < buffs.Count; j++)
                {
                    var b = buffs[j];
                    if (b.stat != type) continue;
                    if (b.modType == StatBuffModType.Flat) flatMod += b.amount;
                    else percentMod += b.amount;
                }
            }
        }

        private ActiveStatusEffect FindActive(StatusEffect_SO effect)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Effect == effect) return active[i];
            }
            return null;
        }

        private bool HasFlag(Func<StatusEffect_SO, bool> predicate)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (predicate(active[i].Effect)) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Runtime state for one applied status effect. Tracks duration and
    /// DoT/HoT tick accumulator.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffect_SO Effect { get; }
        public IUnitRef Source { get; }
        public float RemainingDuration;
        public float TickAccum;

        public ActiveStatusEffect(StatusEffect_SO effect, IUnitRef source, float duration)
        {
            Effect = effect;
            Source = source;
            RemainingDuration = duration;
            TickAccum = 0f;
        }
    }
}
