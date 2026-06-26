using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Classes;
using CapsuleWars.Data.Elements;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

namespace CapsuleWars.Units.Controllers
{
    /// <summary>
    /// Owns a unit's stat block and its active status effects.
    /// Modified stat getters fold in stat-buff modifiers from three
    /// sources: active status effects, equipped items (× rarity
    /// multiplier), and class synergy buffs pushed in by SynergyResolver.
    /// </summary>
    public class UnitStatusController : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField, Min(1)] private int baseMaxHp = 100;
        [SerializeField, Min(0)] private int baseAtk = 20;
        [SerializeField, Min(0)] private int baseDef = 5;
        [SerializeField, Min(0f)] private float baseSpeed = 3.5f;
        [Tooltip("Hit chance baseline (%). Accuracy/CritRate/CritDmg/Resistance fold buffs from statuses, equipment, and synergies.")]
        [SerializeField, Min(0)] private int baseAccuracy = 100;
        [SerializeField, Min(0)] private int baseCritRate = 0;
        [SerializeField, Min(0)] private int baseCritDmg = 50;
        [Tooltip("Resistance to status application (rolled against an effect's accuracy) + elemental resistance.")]
        [SerializeField, Min(0)] private int baseResistance = 0;

        [Header("Element")]
        [Tooltip("Primary element of the unit. Drives element multiplier on dealt/received damage.")]
        [SerializeField] private ElementType_SO primaryElement;

        [Tooltip("Optional secondary element. Dual-element defenders give the attacker the LEAST favorable " +
                 "matchup of their two elements — best defense (Docs/08).")]
        [SerializeField] private ElementType_SO secondaryElement;

        public ElementType_SO PrimaryElement => primaryElement;
        public ElementType_SO SecondaryElement => secondaryElement;

        [Header("Class")]
        [Tooltip("Unit class. Drives class synergy bonuses when N+ same-class units are deployed on the team.")]
        [SerializeField] private UnitClass_SO unitClass;

        public UnitClass_SO UnitClass => unitClass;

        [Header("Equipment")]
        [Tooltip("Equipped items by slot. Stat buffs are folded into modified stats with rarity multiplier applied.")]
        [SerializeField] private List<EquippedItem> equipment = new();

        public IReadOnlyList<EquippedItem> Equipment => equipment;

        public int MaxHp => GetModifiedStat(StatType.MaxHp, baseMaxHp);
        public int Atk => GetModifiedStat(StatType.Atk, baseAtk);
        public int Def => GetModifiedStat(StatType.Def, baseDef);
        public float Speed => GetModifiedStatF(StatType.Speed, baseSpeed);
        public int Accuracy => GetModifiedStat(StatType.Accuracy, baseAccuracy);
        public int CritRate => GetModifiedStat(StatType.CritRate, baseCritRate);
        public int CritDmg => GetModifiedStat(StatType.CritDmg, baseCritDmg);
        public int Resistance => GetModifiedStat(StatType.Resistance, baseResistance);

        // -----------------------------------------------------------------
        // Active status effects
        // -----------------------------------------------------------------

        [Tooltip("Active status effects on this unit. Read-only at runtime — apply via ApplyStatus().")]
        [SerializeField] private List<ActiveStatusEffect> active = new();
        public IReadOnlyList<ActiveStatusEffect> ActiveEffects => active;

        // Synergy buffs pushed in by SynergyResolver each recompute. Not serialized.
        [NonSerialized] private List<StatBuff> synergyBuffs = new();
        public IReadOnlyList<StatBuff> SynergyBuffs => synergyBuffs;

        public event Action<StatusEffect_SO> OnStatusApplied;
        public event Action<StatusEffect_SO> OnStatusExpired;

        /// <summary>
        /// Raised whenever something that affects the modified stat getters
        /// changes — equipping/unequipping items or receiving new synergy buffs.
        /// The inspection/customization UI subscribes to refresh live. (Status
        /// effects already raise OnStatusApplied/OnStatusExpired.)
        /// </summary>
        public event Action OnStatsChanged;

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

            if (effect.Resistance == ResistanceCheck.RollOnApply && IsResisted(effect)) return;

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

        /// <summary>Called by SynergyResolver to push the unit's current synergy buffs.</summary>
        public void SetSynergyBuffs(IList<StatBuff> buffs)
        {
            synergyBuffs.Clear();
            if (buffs != null) synergyBuffs.AddRange(buffs);
            OnStatsChanged?.Invoke();
        }

        /// <summary>Equip a runtime instance — its modifiers feed the modified-stat getters.</summary>
        public void Equip(EquipmentSlot slot, EquipmentInstance instance)
        {
            RemoveSlot(slot);
            equipment.Add(new EquippedItem { slot = slot, instance = instance });
            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// Compat/migration overload: equip a definition directly. Builds a default instance from the
        /// definition's LEGACY baked stats (StatBuffs × rarity), so existing callers and old saves keep
        /// their stats while stats move onto instances.
        /// </summary>
        public void Equip(EquipmentSlot slot, Equipment_SO definition)
            => Equip(slot, EquipmentInstance.FromDefinitionDefault(definition));

        public void UnequipSlot(EquipmentSlot slot)
        {
            if (RemoveSlot(slot)) OnStatsChanged?.Invoke();
        }

        // Removes any item(s) in the slot without raising OnStatsChanged, so
        // Equip (remove-then-add) fires exactly once. Returns true if it removed
        // anything.
        private bool RemoveSlot(EquipmentSlot slot)
        {
            bool removed = false;
            for (int i = equipment.Count - 1; i >= 0; i--)
            {
                if (equipment[i].slot == slot) { equipment.RemoveAt(i); removed = true; }
            }
            return removed;
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

                if (e.Effect.TickAmount != 0)
                {
                    e.TickAccum += dt;
                    while (e.TickAccum >= e.Effect.TickInterval)
                    {
                        e.TickAccum -= e.Effect.TickInterval;
                        ApplyTick(e);
                    }
                }

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
            if (e.Effect.Resistance == ResistanceCheck.RollPerTick && IsResisted(e.Effect)) return;   // resisted this tick
            int amount = e.Effect.TickAmount;
            if (e.Effect.TickIsPercentOfMaxHp)
            {
                amount = Mathf.RoundToInt(MaxHp * (amount / 100f));
            }
            if (amount < 0)
            {
                health.TakeDamage(-amount, e.Source);
            }
            else if (amount > 0)
            {
                int newHp = Mathf.Min(MaxHp, health.CurrentHp + amount);
                float ratio = (float)newHp / Mathf.Max(1, MaxHp);
                health.RestoreToPercent(ratio);
            }
        }

        // -----------------------------------------------------------------
        // Stat math — folds in status effects, equipment (× rarity), and synergy buffs.
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

            // Status effects
            for (int i = 0; i < active.Count; i++)
            {
                SumBuffs(active[i].Effect.StatBuffs, type, ref flatMod, ref percentMod, 1f);
            }

            // Equipment — the equipped instance's runtime modifiers (tier/rarity is already baked into
            // the rolled magnitudes, or folded in by the default-instance migration; no extra multiplier).
            for (int i = 0; i < equipment.Count; i++)
            {
                var inst = equipment[i].instance;
                if (inst == null) continue;
                SumBuffs(inst.modifiers, type, ref flatMod, ref percentMod, 1f);
            }

            // Class synergy buffs (pushed in by SynergyResolver)
            for (int i = 0; i < synergyBuffs.Count; i++)
            {
                var b = synergyBuffs[i];
                if (b.stat != type) continue;
                if (b.modType == StatBuffModType.Flat) flatMod += b.amount;
                else percentMod += b.amount;
            }
        }

        private static void SumBuffs(IReadOnlyList<StatBuff> buffs, StatType type, ref float flatMod, ref float percentMod, float scale)
        {
            if (buffs == null) return;
            for (int j = 0; j < buffs.Count; j++)
            {
                var b = buffs[j];
                if (b.stat != type) continue;
                if (b.modType == StatBuffModType.Flat) flatMod += b.amount * scale;
                else percentMod += b.amount * scale;
            }
        }

        // Resistance roll (Docs/10): apply chance = (effect accuracy − this unit's Resistance) / 1000, clamped 0–1.
        // Returns true when the effect is RESISTED (the roll meets/exceeds the apply chance).
        private bool IsResisted(StatusEffect_SO effect)
        {
            float applyChance = Mathf.Clamp01((effect.Accuracy - Resistance) / 1000f);
            return UnityEngine.Random.value >= applyChance;
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

    /// <summary>One slot → equipped runtime instance, serializable for the inspector.</summary>
    [Serializable]
    public struct EquippedItem
    {
        public EquipmentSlot slot;
        public EquipmentInstance instance;

        /// <summary>The definition (identity/visuals) backing this instance. Null-safe — kept so
        /// existing visual/label readers (UnitEquipmentVisuals, inspection, customization) work unchanged.</summary>
        public Equipment_SO item => instance?.definition;
    }

    /// <summary>
    /// Runtime state for one applied status effect. Tracks duration and
    /// DoT/HoT tick accumulator.
    /// </summary>
    [Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffect_SO Effect;
        [NonSerialized] public IUnitRef Source;
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
