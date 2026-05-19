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
    /// Verifies equipment stat buffs fold into modified stats, including
    /// the rarity multiplier.
    /// </summary>
    public class EquipmentStatTests
    {
        private GameObject go;
        private UnitStatusController status;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            status = go.AddComponent<UnitStatusController>();
            go.AddComponent<UnitHealthController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void EquippedItem_FlatAtkBuff_AddsToAtk()
        {
            int baseAtk = status.Atk;
            var item = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 10 } });
            status.Equip(EquipmentSlot.RightHand, item);

            Assert.AreEqual(baseAtk + 10, status.Atk);
        }

        [Test]
        public void RarityMultiplier_ScalesBuffAmount()
        {
            int baseAtk = status.Atk;
            var rarity = ScriptableObject.CreateInstance<Rarity_SO>();
            SetField(rarity, "statMultiplier", 2.0f);

            var item = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 5 } },
                rarity: rarity);
            status.Equip(EquipmentSlot.RightHand, item);

            // 5 × 2.0 = +10 Atk
            Assert.AreEqual(baseAtk + 10, status.Atk);
        }

        [Test]
        public void MultipleSlots_StackAdditively()
        {
            int baseAtk = status.Atk;
            int baseDef = status.Def;

            var sword = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 8 } });
            var shield = MakeItem(EquipmentSlot.LeftHand,
                buffs: new[] { new StatBuff { stat = StatType.Def, modType = StatBuffModType.Flat, amount = 4 } });

            status.Equip(EquipmentSlot.RightHand, sword);
            status.Equip(EquipmentSlot.LeftHand, shield);

            Assert.AreEqual(baseAtk + 8, status.Atk);
            Assert.AreEqual(baseDef + 4, status.Def);
        }

        [Test]
        public void Equip_ReplacesPreviousInSameSlot()
        {
            int baseAtk = status.Atk;
            var weak = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 5 } });
            var strong = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 20 } });

            status.Equip(EquipmentSlot.RightHand, weak);
            status.Equip(EquipmentSlot.RightHand, strong);

            Assert.AreEqual(baseAtk + 20, status.Atk);
            Assert.AreEqual(1, status.Equipment.Count);
        }

        [Test]
        public void UnequipSlot_RemovesBuff()
        {
            int baseAtk = status.Atk;
            var sword = MakeItem(EquipmentSlot.RightHand,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 10 } });
            status.Equip(EquipmentSlot.RightHand, sword);
            Assert.AreEqual(baseAtk + 10, status.Atk);

            status.UnequipSlot(EquipmentSlot.RightHand);
            Assert.AreEqual(baseAtk, status.Atk);
        }

        [Test]
        public void PercentBuff_AppliesPercentageOfBase()
        {
            // baseAtk = 20 (default). 50% percent buff = +10
            int baseAtk = status.Atk;
            var item = MakeItem(EquipmentSlot.Chest,
                buffs: new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Percent, amount = 50 } });
            status.Equip(EquipmentSlot.Chest, item);

            Assert.AreEqual(baseAtk + Mathf.RoundToInt(baseAtk * 0.5f), status.Atk);
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static Equipment_SO MakeItem(EquipmentSlot slot, IEnumerable<StatBuff> buffs = null, Rarity_SO rarity = null)
        {
            var item = ScriptableObject.CreateInstance<Equipment_SO>();
            SetField(item, "slot", slot);
            if (buffs != null) SetField(item, "statBuffs", new List<StatBuff>(buffs));
            if (rarity != null) SetField(item, "rarity", rarity);
            return item;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
