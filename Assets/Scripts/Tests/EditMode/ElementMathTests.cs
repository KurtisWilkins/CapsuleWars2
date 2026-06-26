using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Elements;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Build-to-spec (Docs/08) — the central element multiplier + the dual-element "least favorable for
    /// attacker" rule. Uses the real ElementChart_SO (default 1.5 / 1.0 / 0.67) and ElementType_SO instances
    /// with families set by reflection.
    /// </summary>
    public class ElementMathTests
    {
        private ElementChart_SO chart;
        private readonly List<Object> created = new List<Object>();

        [SetUp]
        public void SetUp() => chart = ScriptableObject.CreateInstance<ElementChart_SO>();

        [TearDown]
        public void TearDown()
        {
            if (chart != null) Object.DestroyImmediate(chart);
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
            created.Clear();
        }

        private ElementType_SO Element(ElementFamily family)
        {
            var e = ScriptableObject.CreateInstance<ElementType_SO>();
            typeof(ElementType_SO).GetField("family", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(e, family);
            created.Add(e);
            return e;
        }

        [Test]
        public void SingleElement_UsesPrimary()
        {
            var fire = Element(ElementFamily.Fire);
            var air = Element(ElementFamily.Air);     // Fire is strong vs Air → 1.5
            var water = Element(ElementFamily.Water); // Fire is weak vs Water → 0.67

            Assert.AreEqual(1.5f, ElementMath.Multiplier(chart, fire, air, null), 0.001f);
            Assert.AreEqual(0.67f, ElementMath.Multiplier(chart, fire, water, null), 0.001f);
            Assert.AreEqual(1.0f, ElementMath.Multiplier(chart, fire, fire, null), 0.001f); // same family neutral
        }

        [Test]
        public void DualElement_TakesLeastFavorableForAttacker()
        {
            var fire = Element(ElementFamily.Fire);
            var air = Element(ElementFamily.Air);     // 1.5
            var water = Element(ElementFamily.Water); // 0.67

            // Best defense = lowest multiplier for the attacker → min(1.5, 0.67) = 0.67.
            Assert.AreEqual(0.67f, ElementMath.Multiplier(chart, fire, air, water), 0.001f);
            Assert.AreEqual(0.67f, ElementMath.Multiplier(chart, fire, water, air), 0.001f); // order-independent
        }

        [Test]
        public void DualElement_BothNeutral_StaysNeutral()
        {
            var fire = Element(ElementFamily.Fire);
            var fire2 = Element(ElementFamily.Fire);  // neutral
            var earthDefendsStrong = Element(ElementFamily.Earth); // Fire weak vs Earth → 0.67
            // min(neutral 1.0, 0.67) = 0.67
            Assert.AreEqual(0.67f, ElementMath.Multiplier(chart, fire, fire2, earthDefendsStrong), 0.001f);
        }

        [Test]
        public void NullChartOrAttacker_OrNoDefenderElements_ReturnsOne()
        {
            var fire = Element(ElementFamily.Fire);
            Assert.AreEqual(1f, ElementMath.Multiplier(null, fire, fire, null));
            Assert.AreEqual(1f, ElementMath.Multiplier(chart, null, fire, fire));
            Assert.AreEqual(1f, ElementMath.Multiplier(chart, fire, null, null));
        }
    }
}
