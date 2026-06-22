using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// ADR-019: stats live on a runtime EquipmentInstance, not the SO. Verifies the headline claim
    /// (two instances from ONE definition give different stats) plus the roller (determinism, name, tier).
    /// </summary>
    public class EquipmentRollerTests
    {
        [Test]
        public void TwoInstances_SameDefinition_DifferentStats()
        {
            var def = ScriptableObject.CreateInstance<Equipment_SO>();   // one shared "helmet" identity

            var health = new EquipmentInstance(def,
                new[] { new StatBuff { stat = StatType.MaxHp, modType = StatBuffModType.Flat, amount = 50 } }, "Helmet of Health");
            var attack = new EquipmentInstance(def,
                new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 15 } }, "Helmet of Power");

            var (go1, s1) = NewUnit();
            var (go2, s2) = NewUnit();
            int baseHp = s1.MaxHp, baseAtk = s1.Atk;

            s1.Equip(EquipmentSlot.Helmet, health);
            s2.Equip(EquipmentSlot.Helmet, attack);

            Assert.AreEqual(baseHp + 50, s1.MaxHp, "health helmet raises HP");
            Assert.AreEqual(baseAtk, s1.Atk, "health helmet leaves Atk");
            Assert.AreEqual(baseAtk + 15, s2.Atk, "attack helmet raises Atk");
            Assert.AreEqual(baseHp, s2.MaxHp, "attack helmet leaves HP");

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void Roll_SameSeed_IsDeterministic()
        {
            var def = ScriptableObject.CreateInstance<Equipment_SO>();
            var config = MakeConfig();

            var a = EquipmentRoller.Roll(def, config, tier: 0, seed: 1234);
            var b = EquipmentRoller.Roll(def, config, tier: 0, seed: 1234);

            Assert.AreEqual(a.modifiers.Count, b.modifiers.Count);
            for (int i = 0; i < a.modifiers.Count; i++)
            {
                Assert.AreEqual(a.modifiers[i].stat, b.modifiers[i].stat);
                Assert.AreEqual(a.modifiers[i].amount, b.modifiers[i].amount);
            }
            Assert.AreEqual(a.displayName, b.displayName);
        }

        [Test]
        public void Roll_HigherTier_RollsMoreStats()
        {
            var def = ScriptableObject.CreateInstance<Equipment_SO>();
            var config = MakeConfig();

            Assert.AreEqual(1, EquipmentRoller.Roll(def, config, tier: 0, seed: 7).modifiers.Count);
            Assert.AreEqual(2, EquipmentRoller.Roll(def, config, tier: 1, seed: 7).modifiers.Count);
        }

        [Test]
        public void Explicit_GeneratesNameFromDominantStat()
        {
            var def = ScriptableObject.CreateInstance<Equipment_SO>();
            SetField(def, "equipmentId", "Helmet");

            var inst = EquipmentRoller.Explicit(def,
                new[] { new StatBuff { stat = StatType.MaxHp, modType = StatBuffModType.Flat, amount = 30 } });

            Assert.AreEqual("Helmet of Health", inst.displayName);
        }

        // -----------------------------------------------------------------

        private static (GameObject, UnitStatusController) NewUnit()
        {
            var go = new GameObject("TestUnit");
            var s = go.AddComponent<UnitStatusController>();
            go.AddComponent<UnitHealthController>();
            return (go, s);
        }

        private static EquipmentRollConfig MakeConfig()
        {
            var c = ScriptableObject.CreateInstance<EquipmentRollConfig>();
            c.pool = new List<EquipmentRollConfig.RollableStat>
            {
                new EquipmentRollConfig.RollableStat { stat = StatType.MaxHp, modType = StatBuffModType.Flat, minMagnitude = 10, maxMagnitude = 10, weight = 1, nameSuffix = "Health" },
                new EquipmentRollConfig.RollableStat { stat = StatType.Atk,   modType = StatBuffModType.Flat, minMagnitude = 5,  maxMagnitude = 5,  weight = 1, nameSuffix = "Power" },
            };
            c.tiers = new List<EquipmentRollConfig.TierRule>
            {
                new EquipmentRollConfig.TierRule { modifierCount = 1, magnitudeScale = 1f },
                new EquipmentRollConfig.TierRule { modifierCount = 2, magnitudeScale = 2f },
            };
            return c;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
