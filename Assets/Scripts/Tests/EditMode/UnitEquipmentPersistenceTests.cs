using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Units.Controllers;
using CapsuleWars.Units.Customization;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Round-trip + guard tests for run-scoped equipment in the
    /// UnitDTO &lt;-&gt; runtime conversion layer (UnitFactory). Equipment ids
    /// resolve through an in-memory <see cref="EquipmentDatabase"/>.
    /// </summary>
    public class UnitEquipmentPersistenceTests
    {
        private GameObject go;
        private UnitRoot root;
        private UnitStatusController status;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestUnit");
            root = go.AddComponent<UnitRoot>();
            status = go.AddComponent<UnitStatusController>();
            go.AddComponent<UnitCustomization>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        /// <summary>Equipment_SO's id/slot/buffs are private [SerializeField]; set via reflection for tests.</summary>
        private static Equipment_SO MakeItem(string id, EquipmentSlot slot, IEnumerable<StatBuff> buffs = null)
        {
            var item = ScriptableObject.CreateInstance<Equipment_SO>();
            SetField(item, "equipmentId", id);
            SetField(item, "slot", slot);
            if (buffs != null) SetField(item, "statBuffs", new List<StatBuff>(buffs));
            return item;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(target, value);
        }

        [Test]
        public void FromDTO_AppliesEquipment_FromIds_AndFoldsIntoStats()
        {
            var sword = MakeItem("iron_sword", EquipmentSlot.RightHand,
                new[] { new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 10 } });
            var db = new EquipmentDatabase(new[] { sword });
            var dto = new UnitDTO("u1", "Conan", null);
            dto.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "iron_sword"));

            int baseAtk = status.Atk;
            UnitFactory.FromDTO(dto, root, null, null, db);

            Assert.AreEqual(1, status.Equipment.Count);
            Assert.AreSame(sword, status.Equipment[0].item);
            Assert.AreEqual(baseAtk + 10, status.Atk);

            Object.DestroyImmediate(sword);
        }

        [Test]
        public void FromUnit_CapturesEquipment()
        {
            var sword = MakeItem("iron_sword", EquipmentSlot.RightHand);
            status.Equip(EquipmentSlot.RightHand, sword);
            root.SetIdentity("u2", "Merlin");

            var dto = UnitFactory.FromUnit(root);

            Assert.AreEqual(1, dto.Equipment.Count);
            Assert.AreEqual(EquipmentSlot.RightHand, dto.Equipment[0].slot);
            Assert.AreEqual("iron_sword", dto.Equipment[0].equipmentId);

            Object.DestroyImmediate(sword);
        }

        [Test]
        public void RoundTrip_DtoToUnitToDto_PreservesEquipment()
        {
            var sword = MakeItem("iron_sword", EquipmentSlot.RightHand);
            var shield = MakeItem("oak_shield", EquipmentSlot.LeftHand);
            var db = new EquipmentDatabase(new[] { sword, shield });
            var original = new UnitDTO("u3", "Robin", null);
            original.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "iron_sword"));
            original.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.LeftHand, "oak_shield"));

            UnitFactory.FromDTO(original, root, null, null, db);
            var result = UnitFactory.FromUnit(root);

            Assert.AreEqual(2, result.Equipment.Count);
            CollectionAssert.AreEquivalent(
                new[] { "iron_sword", "oak_shield" },
                result.Equipment.ConvertAll(e => e.equipmentId));

            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(shield);
        }

        [Test]
        public void FromDTO_UnknownEquipmentId_SkippedGracefully()
        {
            var db = new EquipmentDatabase();
            var dto = new UnitDTO("u4", "Nameless", null);
            dto.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "ghost_blade"));

            Assert.DoesNotThrow(() => UnitFactory.FromDTO(dto, root, null, null, db));
            Assert.AreEqual(0, status.Equipment.Count);
        }

        [Test]
        public void FromDTO_NullEquipmentDatabase_IgnoresEquipment()
        {
            var dto = new UnitDTO("u5", "Solo", null);
            dto.Equipment.Add(new UnitEquipmentDTO(EquipmentSlot.RightHand, "iron_sword"));

            Assert.DoesNotThrow(() => UnitFactory.FromDTO(dto, root, null));
            Assert.AreEqual(0, status.Equipment.Count);
        }

        [Test]
        public void EquipmentDatabase_ResolvesById_AndGuardsBadInput()
        {
            var sword = MakeItem("iron_sword", EquipmentSlot.RightHand);
            var db = new EquipmentDatabase();
            db.Register(sword);
            db.Register(null);                                 // ignored
            db.Register(MakeItem(string.Empty, EquipmentSlot.Chest)); // ignored (no id)

            Assert.AreSame(sword, db.GetEquipment("iron_sword"));
            Assert.IsNull(db.GetEquipment("nope"));
            Assert.IsNull(db.GetEquipment(null));
            Assert.IsNull(db.GetEquipment(string.Empty));

            Object.DestroyImmediate(sword);
        }
    }
}
