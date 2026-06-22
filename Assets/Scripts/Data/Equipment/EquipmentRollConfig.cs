using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Data-driven pool for rolling equipment stats at runtime (ADR-019): which stat types can roll,
    /// their magnitude ranges + weights + name suffix, and per-tier rules (how many stats / how strong).
    /// Tuned in the inspector so rolls change without code. Consumed by <see cref="EquipmentRoller"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentRollConfig", menuName = "CapsuleWars/Equipment/Roll Config", order = 93)]
    public class EquipmentRollConfig : ScriptableObject
    {
        [Serializable]
        public class RollableStat
        {
            public StatType stat = StatType.MaxHp;
            public StatBuffModType modType = StatBuffModType.Flat;
            [Min(0f)] public float minMagnitude = 5f;
            [Min(0f)] public float maxMagnitude = 15f;
            [Min(0f)] public float weight = 1f;
            [Tooltip("Name suffix when this is the dominant stat, e.g. 'Health' → '… of Health'.")]
            public string nameSuffix = "";
        }

        [Serializable]
        public class TierRule
        {
            [Min(1)] public int modifierCount = 1;
            [Min(0f)] public float magnitudeScale = 1f;
        }

        [Tooltip("Stats that can roll: magnitude range, selection weight, and dominant-stat name suffix.")]
        public List<RollableStat> pool = new List<RollableStat>();

        [Tooltip("Per-tier rules (index = tier): higher tiers roll more / stronger stats. Tier clamps to the last entry.")]
        public List<TierRule> tiers = new List<TierRule> { new TierRule() };

        public TierRule TierRuleFor(int tier)
        {
            if (tiers == null || tiers.Count == 0) return new TierRule();
            return tiers[Mathf.Clamp(tier, 0, tiers.Count - 1)];
        }
    }
}
