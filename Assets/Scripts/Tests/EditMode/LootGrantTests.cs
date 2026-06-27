using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// LootGrant (BTS-G): rolls a LootTable and deposits the drops into RunState's loose inventory as
    /// save-shaped UnitEquipmentDTOs. The testable seam the reward hook calls before RunSession.Save().
    /// </summary>
    public class LootGrantTests
    {
        private static void SetField(object t, string f, object v) =>
            t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, v);

        private static Equipment_SO MakeDef(string id, EquipmentSlot slot)
        {
            var e = ScriptableObject.CreateInstance<Equipment_SO>();
            SetField(e, "equipmentId", id);
            SetField(e, "slot", slot);
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
            c.tiers = new List<EquipmentRollConfig.TierRule> { new EquipmentRollConfig.TierRule { modifierCount = 1, magnitudeScale = 1f } };
            return c;
        }

        private static LootTable_SO MakeTable(int min, int max, Equipment_SO def, EquipmentRollConfig cfg)
        {
            var t = ScriptableObject.CreateInstance<LootTable_SO>();
            SetField(t, "minDrops", min);
            SetField(t, "maxDrops", max);
            SetField(t, "items", new List<LootTable_SO.WeightedDrop> { new LootTable_SO.WeightedDrop { item = def, weight = 1f } });
            SetField(t, "tiers", new List<LootTable_SO.WeightedTier> { new LootTable_SO.WeightedTier { tier = 0, weight = 1f } });
            SetField(t, "rollConfig", cfg);
            return t;
        }

        private static RunState NewRun() => new RunState(new RunMap(new List<MapNode>()), 0, 1);

        private static void Cleanup(params Object[] objs) { foreach (var o in objs) if (o != null) Object.DestroyImmediate(o); }

        [Test]
        public void GrantTo_AddsRolledItems_ToInventory()
        {
            var def = MakeDef("iron_helm", EquipmentSlot.Helmet);
            var cfg = MakeConfig();
            var table = MakeTable(2, 2, def, cfg);   // exactly 2 drops
            var state = NewRun();

            int n = LootGrant.GrantTo(state, table, 123);

            Assert.AreEqual(2, n);
            Assert.AreEqual(2, state.Inventory.Count);
            Assert.AreEqual("iron_helm", state.Inventory[0].equipmentId);
            Assert.AreEqual(EquipmentSlot.Helmet, state.Inventory[0].slot);

            Cleanup(def, cfg, table);
        }

        [Test]
        public void GrantTo_IsDeterministic_ForSameSeed()
        {
            var def = MakeDef("iron_helm", EquipmentSlot.Helmet);
            var cfg = MakeConfig();
            var table = MakeTable(1, 1, def, cfg);
            var a = NewRun(); var b = NewRun();

            LootGrant.GrantTo(a, table, 555);
            LootGrant.GrantTo(b, table, 555);

            Assert.AreEqual(a.Inventory.Count, b.Inventory.Count);
            Assert.AreEqual(a.Inventory[0].seed, b.Inventory[0].seed);
            Assert.AreEqual(a.Inventory[0].modifiers.Count, b.Inventory[0].modifiers.Count);

            Cleanup(def, cfg, table);
        }

        [Test]
        public void GrantTo_NullTableOrState_GrantsNothing()
        {
            var state = NewRun();
            Assert.AreEqual(0, LootGrant.GrantTo(state, null, 1));
            Assert.AreEqual(0, state.Inventory.Count);
            Assert.AreEqual(0, LootGrant.GrantTo(null, null, 1));
        }
    }
}
