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
    /// Verifies UnitStatusController.OnStatsChanged fires when equipment or
    /// synergy buffs change, so the inspection/customization UI can refresh live.
    /// </summary>
    public class UnitStatusEventsTests
    {
        private GameObject go;
        private UnitStatusController status;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            status = go.AddComponent<UnitStatusController>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        private static Equipment_SO MakeItem(EquipmentSlot slot)
        {
            var item = ScriptableObject.CreateInstance<Equipment_SO>();
            typeof(Equipment_SO).GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(item, slot);
            return item;
        }

        [Test]
        public void Equip_FiresOnStatsChanged_Once()
        {
            int fired = 0;
            status.OnStatsChanged += () => fired++;

            var item = MakeItem(EquipmentSlot.RightHand);
            status.Equip(EquipmentSlot.RightHand, item);

            Assert.AreEqual(1, fired);   // remove-then-add fires exactly once
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Unequip_FiresOnlyWhenSomethingRemoved()
        {
            int fired = 0;
            status.OnStatsChanged += () => fired++;

            status.UnequipSlot(EquipmentSlot.Chest);   // empty slot -> no change
            Assert.AreEqual(0, fired);

            var item = MakeItem(EquipmentSlot.Chest);
            status.Equip(EquipmentSlot.Chest, item);    // +1
            status.UnequipSlot(EquipmentSlot.Chest);    // +1
            Assert.AreEqual(2, fired);

            Object.DestroyImmediate(item);
        }

        [Test]
        public void SetSynergyBuffs_FiresOnStatsChanged()
        {
            int fired = 0;
            status.OnStatsChanged += () => fired++;

            status.SetSynergyBuffs(new List<StatBuff>());
            Assert.AreEqual(1, fired);
        }
    }
}
