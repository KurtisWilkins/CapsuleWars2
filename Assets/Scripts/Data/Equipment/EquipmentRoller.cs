using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.StatusEffects;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// Builds an <see cref="EquipmentInstance"/> from a definition + a stat spec (ADR-019). The spec is
    /// either explicit (caller hands the modifiers) or rolled from an <see cref="EquipmentRollConfig"/> +
    /// tier, deterministically from a seed. UI-free and unit-testable. Generates a display name from the
    /// dominant stat ("… of Health"). Supports a single stat, a combination, or all stats on one item.
    /// </summary>
    public static class EquipmentRoller
    {
        /// <summary>Explicit instance — caller supplies the modifiers (and optional name).</summary>
        public static EquipmentInstance Explicit(Equipment_SO definition, IEnumerable<StatBuff> modifiers,
                                                 string displayName = null, int tier = 0, int seed = 0)
        {
            var inst = new EquipmentInstance(definition, modifiers, displayName, tier, seed);
            if (string.IsNullOrEmpty(inst.displayName))
                inst.displayName = GenerateName(definition, inst.modifiers, null);
            return inst;
        }

        /// <summary>
        /// Roll an instance from <paramref name="config"/> at <paramref name="tier"/>, deterministically
        /// from <paramref name="seed"/>: picks the tier's modifierCount distinct stats by weight, each with
        /// a magnitude in [min,max] × the tier's scale, plus a generated name. Same seed → same roll.
        /// </summary>
        public static EquipmentInstance Roll(Equipment_SO definition, EquipmentRollConfig config, int tier, int seed)
        {
            var mods = new List<StatBuff>();
            if (config != null && config.pool != null && config.pool.Count > 0)
            {
                var rng = new System.Random(seed);
                var rule = config.TierRuleFor(tier);
                var available = new List<EquipmentRollConfig.RollableStat>(config.pool);
                int count = Mathf.Clamp(rule.modifierCount, 1, available.Count);

                for (int i = 0; i < count; i++)
                {
                    int idx = PickWeighted(available, rng);
                    var pick = available[idx];
                    available.RemoveAt(idx);   // distinct stat per modifier

                    float t = (float)rng.NextDouble();
                    float mag = Mathf.Lerp(pick.minMagnitude, pick.maxMagnitude, t) * Mathf.Max(0f, rule.magnitudeScale);
                    mods.Add(new StatBuff { stat = pick.stat, modType = pick.modType, amount = Mathf.Round(mag) });
                }
            }

            return new EquipmentInstance(definition, mods, GenerateName(definition, mods, config), tier, seed);
        }

        // Dominant (largest |amount|) modifier → "{base name} of {suffix}".
        private static string GenerateName(Equipment_SO definition, List<StatBuff> mods, EquipmentRollConfig config)
        {
            string baseName = definition != null
                ? (string.IsNullOrEmpty(definition.NameTermKey) ? definition.EquipmentId : definition.NameTermKey)
                : "Item";
            if (mods == null || mods.Count == 0) return baseName;

            int best = 0;
            for (int i = 1; i < mods.Count; i++)
                if (Mathf.Abs(mods[i].amount) > Mathf.Abs(mods[best].amount)) best = i;

            string suffix = SuffixFor(mods[best].stat, config);
            return string.IsNullOrEmpty(suffix) ? baseName : $"{baseName} of {suffix}";
        }

        private static string SuffixFor(StatType stat, EquipmentRollConfig config)
        {
            if (config != null && config.pool != null)
                foreach (var p in config.pool)
                    if (p.stat == stat && !string.IsNullOrEmpty(p.nameSuffix)) return p.nameSuffix;

            switch (stat)   // sensible defaults if the config has no suffix
            {
                case StatType.MaxHp: return "Health";
                case StatType.Atk: return "Power";
                case StatType.Def: return "Warding";
                case StatType.Speed: return "Swiftness";
                default: return stat.ToString();
            }
        }

        private static int PickWeighted(List<EquipmentRollConfig.RollableStat> items, System.Random rng)
        {
            float total = 0f;
            for (int i = 0; i < items.Count; i++) total += Mathf.Max(0f, items[i].weight);
            if (total <= 0f) return rng.Next(items.Count);

            double r = rng.NextDouble() * total;
            for (int i = 0; i < items.Count; i++)
            {
                r -= Mathf.Max(0f, items[i].weight);
                if (r <= 0) return i;
            }
            return items.Count - 1;
        }
    }
}
