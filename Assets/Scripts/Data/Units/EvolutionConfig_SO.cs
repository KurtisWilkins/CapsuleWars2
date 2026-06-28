using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Tunable evolution curve (BTS-H): the cumulative XP needed to reach each evolution tier, the per-tier stat
    /// growth, and the XP awarded per battle win. A unit earns XP after each battle; crossing a threshold raises its
    /// tier, which scales its BASE stats by (1 + tier * statGrowthPerTier). Data-only; the math lives in the pure
    /// <see cref="UnitEvolution"/> helper so it's testable without a unit. First-pass numbers, tunable in the inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "EvolutionConfig", menuName = "CapsuleWars/Evolution Config", order = 5)]
    public class EvolutionConfig_SO : ScriptableObject
    {
        [Tooltip("Cumulative XP required for tier 1, 2, 3, … (ascending). A unit's tier = how many thresholds its XP has reached.")]
        [SerializeField] private List<int> xpThresholds = new() { 100, 250, 450, 700 };

        [Tooltip("Fractional base-stat growth per evolution tier (0.12 = +12% per tier; tier 2 = +24%).")]
        [SerializeField, Min(0f)] private float statGrowthPerTier = 0.12f;

        [Tooltip("XP awarded to each surviving party unit after a won battle (first-pass).")]
        [SerializeField, Min(0)] private int xpPerBattleWin = 60;

        public IReadOnlyList<int> XpThresholds => xpThresholds;
        public float StatGrowthPerTier => statGrowthPerTier;
        public int XpPerBattleWin => xpPerBattleWin;
    }
}
