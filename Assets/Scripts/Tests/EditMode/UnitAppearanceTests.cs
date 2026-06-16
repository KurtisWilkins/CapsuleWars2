using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence;
using CapsuleWars.Persistence.Dto;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Asymmetric-mixing data layer: per-slot part editing (left/right independent)
    /// + unlock/slot validation.
    /// </summary>
    public class UnitAppearanceTests
    {
        private static BodyPart_SO MakePart(string id, PartSlot slot)
        {
            var p = ScriptableObject.CreateInstance<BodyPart_SO>();
            typeof(BodyPart_SO).GetField("partId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, id);
            typeof(BodyPart_SO).GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, slot);
            return p;
        }

        private static PartCatalog_SO MakeCatalog(List<PartCatalog_SO.PartEntry> parts)
        {
            var c = ScriptableObject.CreateInstance<PartCatalog_SO>();
            typeof(PartCatalog_SO).GetField("parts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, parts);
            return c;
        }

        [Test]
        public void SetPart_LeftAndRightHands_AreIndependent()
        {
            var dto = new UnitDTO("u", "U", null);
            UnitAppearance.SetPart(dto, PartSlot.LeftHand, "hand_a");
            UnitAppearance.SetPart(dto, PartSlot.RightHand, "hand_b");
            Assert.AreEqual("hand_a", UnitAppearance.GetPart(dto, PartSlot.LeftHand));
            Assert.AreEqual("hand_b", UnitAppearance.GetPart(dto, PartSlot.RightHand));
        }

        [Test]
        public void SetPart_ReplacesSameSlot()
        {
            var dto = new UnitDTO("u", "U", null);
            UnitAppearance.SetPart(dto, PartSlot.Body, "x");
            UnitAppearance.SetPart(dto, PartSlot.Body, "y");
            Assert.AreEqual("y", UnitAppearance.GetPart(dto, PartSlot.Body));
            Assert.AreEqual(1, dto.Parts.Count);
        }

        [Test]
        public void SetPart_NullClearsSlot()
        {
            var dto = new UnitDTO("u", "U", null);
            UnitAppearance.SetPart(dto, PartSlot.Body, "x");
            UnitAppearance.SetPart(dto, PartSlot.Body, null);
            Assert.IsNull(UnitAppearance.GetPart(dto, PartSlot.Body));
            Assert.AreEqual(0, dto.Parts.Count);
        }

        [Test]
        public void TrySetUnlockedPart_EnforcesSlotAndOwnership()
        {
            var part = MakePart("hand_a", PartSlot.LeftHand);
            var catalog = MakeCatalog(new List<PartCatalog_SO.PartEntry> { new PartCatalog_SO.PartEntry { part = part } });
            var profile = new PlayerProfileDTO();
            var dto = new UnitDTO("u", "U", null);

            Assert.IsFalse(UnitAppearance.TrySetUnlockedPart(dto, profile, catalog, PartSlot.LeftHand, "hand_a"), "not unlocked");
            profile.GrantPart("hand_a");
            Assert.IsFalse(UnitAppearance.TrySetUnlockedPart(dto, profile, catalog, PartSlot.RightHand, "hand_a"), "wrong slot");
            Assert.IsTrue(UnitAppearance.TrySetUnlockedPart(dto, profile, catalog, PartSlot.LeftHand, "hand_a"));
            Assert.AreEqual("hand_a", UnitAppearance.GetPart(dto, PartSlot.LeftHand));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(part);
        }
    }
}
