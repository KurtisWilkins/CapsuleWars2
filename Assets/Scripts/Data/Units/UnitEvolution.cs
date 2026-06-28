using System.Collections.Generic;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Pure evolution math (BTS-H): XP → evolution tier (via ascending cumulative thresholds) and tier → base-stat
    /// growth multiplier. No Unity references, deterministic — the single source of truth shared by the stat layer
    /// (which scales base stats) and the EditMode tests.
    /// </summary>
    public static class UnitEvolution
    {
        /// <summary>Evolution tier = the number of ascending thresholds the XP has reached or passed.</summary>
        public static int TierFor(int xp, IReadOnlyList<int> thresholds)
        {
            if (thresholds == null) return 0;
            int tier = 0;
            for (int i = 0; i < thresholds.Count; i++)
            {
                if (xp >= thresholds[i]) tier = i + 1;
                else break;   // ascending → once one isn't met, none beyond it are
            }
            return tier;
        }

        /// <summary>Base-stat multiplier for a tier: 1 + tier * growthPerTier (tier 0 / no growth = 1.0).</summary>
        public static float GrowthMultiplier(int tier, float growthPerTier)
        {
            if (tier <= 0 || growthPerTier <= 0f) return 1f;
            return 1f + tier * growthPerTier;
        }

        public static int TierFor(int xp, EvolutionConfig_SO config) =>
            config != null ? TierFor(xp, config.XpThresholds) : 0;

        /// <summary>The base-stat multiplier a unit with this XP has earned under <paramref name="config"/>.</summary>
        public static float GrowthMultiplier(int xp, EvolutionConfig_SO config) =>
            config != null ? GrowthMultiplier(TierFor(xp, config.XpThresholds), config.StatGrowthPerTier) : 1f;
    }
}
