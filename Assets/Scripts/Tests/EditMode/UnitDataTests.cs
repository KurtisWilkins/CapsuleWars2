using System;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Sanity tests for M1 data types. Verifies enum surface and that the
    /// SO types instantiate cleanly. Does not exercise UnitCustomization
    /// (that requires a scene; covered in PlayMode tests in M2).
    /// </summary>
    public class UnitDataTests
    {
        [Test]
        public void PartSlot_HasSixSlots()
        {
            Assert.AreEqual(6, Enum.GetNames(typeof(PartSlot)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.Body));
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.LeftHand));
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.RightHand));
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.LeftFoot));
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.RightFoot));
            Assert.IsTrue(Enum.IsDefined(typeof(PartSlot), PartSlot.HeadProp));
        }

        [Test]
        public void PaletteRole_HasFourValues()
        {
            Assert.AreEqual(4, Enum.GetNames(typeof(PaletteRole)).Length);
            Assert.AreEqual(0, (int)PaletteRole.None);
        }

        [Test]
        public void BodyPart_SO_InstantiatesWithDefaults()
        {
            var part = ScriptableObject.CreateInstance<BodyPart_SO>();
            Assert.IsNotNull(part);
            Assert.AreEqual(PartSlot.Body, part.Slot);
            Assert.IsNull(part.Mesh);
            ScriptableObject.DestroyImmediate(part);
        }

        [Test]
        public void Palette_SO_InstantiatesWithWhiteDefaults()
        {
            var palette = ScriptableObject.CreateInstance<Palette_SO>();
            Assert.IsNotNull(palette);
            Assert.AreEqual(Color.white, palette.Body);
            Assert.AreEqual(Color.white, palette.Limbs);
            Assert.AreEqual(Color.white, palette.Accent);
            ScriptableObject.DestroyImmediate(palette);
        }

        [Test]
        public void UnitDefinition_SO_InstantiatesWithEmptyParts()
        {
            var def = ScriptableObject.CreateInstance<UnitDefinition_SO>();
            Assert.IsNotNull(def);
            Assert.IsNotNull(def.Parts);
            Assert.AreEqual(0, def.Parts.Count);
            Assert.IsNull(def.Palette);
            ScriptableObject.DestroyImmediate(def);
        }
    }
}
