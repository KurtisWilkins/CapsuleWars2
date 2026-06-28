using System.Collections.Generic;
using CapsuleWars.Data.Units;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// BTS-H evolution math: XP maps to an evolution tier via ascending cumulative thresholds, and tier maps to a
    /// base-stat growth multiplier. Pure logic — no scene / Play.
    /// </summary>
    public class UnitEvolutionTests
    {
        private static readonly List<int> Thresholds = new() { 100, 250, 450, 700 };

        [Test]
        public void TierFor_CrossesThresholdsInOrder()
        {
            Assert.AreEqual(0, UnitEvolution.TierFor(0, Thresholds));
            Assert.AreEqual(0, UnitEvolution.TierFor(99, Thresholds));
            Assert.AreEqual(1, UnitEvolution.TierFor(100, Thresholds));
            Assert.AreEqual(1, UnitEvolution.TierFor(249, Thresholds));
            Assert.AreEqual(2, UnitEvolution.TierFor(250, Thresholds));
            Assert.AreEqual(3, UnitEvolution.TierFor(450, Thresholds));
            Assert.AreEqual(4, UnitEvolution.TierFor(700, Thresholds));
        }

        [Test]
        public void TierFor_CapsAtThresholdCount_AndHandlesNull()
        {
            Assert.AreEqual(4, UnitEvolution.TierFor(999999, Thresholds), "tier caps at the number of thresholds");
            Assert.AreEqual(0, UnitEvolution.TierFor(500, (IReadOnlyList<int>)null), "null thresholds → tier 0");
            Assert.AreEqual(0, UnitEvolution.TierFor(500, new List<int>()), "empty thresholds → tier 0");
        }

        [Test]
        public void GrowthMultiplier_ScalesPerTier()
        {
            Assert.AreEqual(1f, UnitEvolution.GrowthMultiplier(0, 0.12f), 1e-5f, "tier 0 → no growth");
            Assert.AreEqual(1.12f, UnitEvolution.GrowthMultiplier(1, 0.12f), 1e-5f);
            Assert.AreEqual(1.24f, UnitEvolution.GrowthMultiplier(2, 0.12f), 1e-5f);
            Assert.AreEqual(1.48f, UnitEvolution.GrowthMultiplier(4, 0.12f), 1e-5f);
            Assert.AreEqual(1f, UnitEvolution.GrowthMultiplier(3, 0f), 1e-5f, "zero growth rate → no change");
        }

        [Test]
        public void GrowthMultiplier_FromXp_ComposesTierAndRate()
        {
            // xp 300 → tier 2 → 1 + 2*0.1 = 1.2
            Assert.AreEqual(1.2f, UnitEvolution.GrowthMultiplier(UnitEvolution.TierFor(300, Thresholds), 0.1f), 1e-5f);
        }
    }
}
