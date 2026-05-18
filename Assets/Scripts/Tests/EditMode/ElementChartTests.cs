using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Verifies the 5-family wheel matchup math. Defaults from Docs/08:
    /// strong ×1.5, neutral ×1.0, weak ×0.67.
    /// </summary>
    public class ElementChartTests
    {
        private ElementChart_SO chart;

        [SetUp]
        public void Setup()
        {
            chart = ScriptableObject.CreateInstance<ElementChart_SO>();
        }

        [TearDown]
        public void Teardown()
        {
            ScriptableObject.DestroyImmediate(chart);
        }

        [Test]
        public void Fire_BeatsAirAndSpirit()
        {
            Assert.Greater(chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Air), 1f);
            Assert.Greater(chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Spirit), 1f);
        }

        [Test]
        public void Fire_LosesToWaterAndEarth()
        {
            Assert.Less(chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Water), 1f);
            Assert.Less(chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Earth), 1f);
        }

        [Test]
        public void SameFamily_IsNeutral()
        {
            Assert.AreEqual(1f, chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Fire), 0.0001f);
            Assert.AreEqual(1f, chart.GetMultiplier(ElementFamily.Water, ElementFamily.Water), 0.0001f);
            Assert.AreEqual(1f, chart.GetMultiplier(ElementFamily.Earth, ElementFamily.Earth), 0.0001f);
            Assert.AreEqual(1f, chart.GetMultiplier(ElementFamily.Spirit, ElementFamily.Spirit), 0.0001f);
            Assert.AreEqual(1f, chart.GetMultiplier(ElementFamily.Air, ElementFamily.Air), 0.0001f);
        }

        [Test]
        public void WheelIsCyclic_EachFamilyHasTwoStrongTwoWeak()
        {
            for (int a = 0; a < 5; a++)
            {
                int strong = 0, weak = 0, neutral = 0;
                for (int d = 0; d < 5; d++)
                {
                    float m = chart.GetMultiplier((ElementFamily)a, (ElementFamily)d);
                    if (m > 1f) strong++;
                    else if (m < 1f) weak++;
                    else neutral++;
                }
                Assert.AreEqual(2, strong, $"Family {(ElementFamily)a} should beat 2 others");
                Assert.AreEqual(2, weak, $"Family {(ElementFamily)a} should lose to 2 others");
                Assert.AreEqual(1, neutral, $"Family {(ElementFamily)a} should be neutral vs 1 (itself)");
            }
        }

        [Test]
        public void Multipliers_RespectConfiguredValues()
        {
            SetField(chart, "strongMultiplier", 2.0f);
            SetField(chart, "weakMultiplier", 0.5f);
            SetField(chart, "neutralMultiplier", 1.0f);

            Assert.AreEqual(2.0f, chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Air), 0.0001f);
            Assert.AreEqual(0.5f, chart.GetMultiplier(ElementFamily.Fire, ElementFamily.Water), 0.0001f);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
