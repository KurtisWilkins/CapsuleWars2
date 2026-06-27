using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// LootRoller (BTS-G): deterministic, layering-clean drop generation from a LootTable_SO — drop count
    /// within [min,max], weighted item/tier selection, graceful empty cases. Pure Data logic (no Play).
    /// </summary>
    public class LootRollerTests
    {
        private static void SetField(object t, string f, object v) =>
            t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, v);

        private static Equipment_SO MakeDef(string id)
        {
            var e = ScriptableObject.CreateInstance<Equipment_SO>();
            SetField(e, "equipmentId", id);
            return e;
        }

        private static EquipmentRollConfig MakeConfig()
        {
            var c = ScriptableObject.CreateInstance<EquipmentRollConfig>();
            c.pool = new List<EquipmentRollConfig.RollableStat>
            {
                new EquipmentRollConfig.RollableStat
                {
                    stat = StatType.MaxHp, modType = StatBuffModType.Flat,
                    minMagnitude = 5, maxMagnitude = 15, weight = 1, nameSuffix = "Health"
                },
            };
            c.tiers = new List<EquipmentRollConfig.TierRule>
            {
                new EquipmentRollConfig.TierRule { modifierCount = 1, magnitudeScale = 1f },
                new EquipmentRollConfig.TierRule { modifierCount = 2, magnitudeScale = 1.5f },
            };
            return c;
        }

        private static LootTable_SO MakeTable(int min, int max, List<LootTable_SO.WeightedDrop> items,
                                              List<LootTable_SO.WeightedTier> tiers, EquipmentRollConfig cfg)
        {
            var t = ScriptableObject.CreateInstance<LootTable_SO>();
            SetField(t, "minDrops", min);
            SetField(t, "maxDrops", max);
            SetField(t, "items", items);
            SetField(t, "tiers", tiers);
            SetField(t, "rollConfig", cfg);
            return t;
        }

        private static List<LootTable_SO.WeightedDrop> Drops(params Equipment_SO[] defs)
        {
            var list = new List<LootTable_SO.WeightedDrop>();
            foreach (var d in defs) list.Add(new LootTable_SO.WeightedDrop { item = d, weight = 1f });
            return list;
        }

        private static List<LootTable_SO.WeightedTier> Tiers(params int[] ts)
        {
            var list = new List<LootTable_SO.WeightedTier>();
            foreach (var t in ts) list.Add(new LootTable_SO.WeightedTier { tier = t, weight = 1f });
            return list;
        }

        private static void Cleanup(params Object[] objs)
        {
            foreach (var o in objs) if (o != null) Object.DestroyImmediate(o);
        }

        [Test]
        public void Roll_IsDeterministic_ForSameSeed()
        {
            var def = MakeDef("iron_helm");
            var cfg = MakeConfig();
            var table = MakeTable(1, 3, Drops(def), Tiers(0), cfg);

            var a = LootRoller.Roll(table, 12345);
            var b = LootRoller.Roll(table, 12345);

            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreSame(a[i].definition, b[i].definition);
                Assert.AreEqual(a[i].tier, b[i].tier);
                Assert.AreEqual(a[i].modifiers.Count, b[i].modifiers.Count);
                for (int j = 0; j < a[i].modifiers.Count; j++)
                    Assert.AreEqual(a[i].modifiers[j].amount, b[i].modifiers[j].amount);
            }

            Cleanup(def, cfg, table);
        }

        [Test]
        public void Roll_DropCount_WithinMinMax()
        {
            var def = MakeDef("iron_helm");
            var cfg = MakeConfig();
            var table = MakeTable(2, 4, Drops(def), Tiers(0), cfg);

            for (int seed = 0; seed < 25; seed++)
            {
                int c = LootRoller.Roll(table, seed).Count;
                Assert.GreaterOrEqual(c, 2);
                Assert.LessOrEqual(c, 4);
            }
            Cleanup(def, cfg, table);
        }

        [Test]
        public void Roll_EmptyItemPool_YieldsNoDrops()
        {
            var cfg = MakeConfig();
            var table = MakeTable(1, 3, new List<LootTable_SO.WeightedDrop>(), Tiers(0), cfg);
            Assert.AreEqual(0, LootRoller.Roll(table, 1).Count);
            Cleanup(cfg, table);
        }

        [Test]
        public void Roll_ZeroDrops_AndNullTable_YieldEmpty()
        {
            var def = MakeDef("iron_helm");
            var cfg = MakeConfig();
            var table = MakeTable(0, 0, Drops(def), Tiers(0), cfg);
            Assert.AreEqual(0, LootRoller.Roll(table, 1).Count);
            Assert.AreEqual(0, LootRoller.Roll(null, 1).Count);
            Cleanup(def, cfg, table);
        }

        [Test]
        public void Roll_TierSelection_UsesWeightedTier()
        {
            var def = MakeDef("iron_helm");
            var cfg = MakeConfig();
            var table = MakeTable(1, 1, Drops(def), Tiers(1), cfg);   // only tier 1 weighted

            for (int seed = 0; seed < 10; seed++)
            {
                var drops = LootRoller.Roll(table, seed);
                Assert.AreEqual(1, drops.Count);
                Assert.AreEqual(1, drops[0].tier, "every drop should use the only weighted tier");
            }
            Cleanup(def, cfg, table);
        }
    }
}
