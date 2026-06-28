using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tint milestone data layer: the luminance-ramp derivation, per-part accent overrides on EquipmentInstance,
    /// and TintPreset capture/apply round-trip. Pure/EditMode — the shader + on-screen result are editor/Play-gated.
    /// </summary>
    public class TintSystemTests
    {
        [Test]
        public void Ramp_Untinted_IsIdentity_GrayscalePreserved()
        {
            TintRamp.FromColor(Color.clear, out var s, out var m, out var h);
            Assert.AreEqual(TintRamp.IdentityShadow, s);
            Assert.AreEqual(TintRamp.IdentityMid, m);
            Assert.AreEqual(TintRamp.IdentityHigh, h);
            Assert.IsFalse(TintRamp.IsTinted(Color.clear));
        }

        [Test]
        public void Ramp_Color_DerivesDarkToLight_KeepingHue()
        {
            var green = Color.green; // (0,1,0,1)
            TintRamp.FromColor(green, out var s, out var m, out var h);
            Assert.IsTrue(TintRamp.IsTinted(green));
            Assert.AreEqual(green.g, m.g, 1e-5f, "mid == the tint color");
            Assert.Less(s.g, m.g, "shadow is darker than mid");
            Assert.Greater(h.r, m.r, "high lifts toward white");
            Assert.IsTrue(m.g > m.r && m.g > m.b, "green stays the dominant channel");
        }

        [Test]
        public void Instance_DefaultUntinted_AndAccentsOverridePrimary()
        {
            var inst = new EquipmentInstance();
            Assert.IsFalse(TintRamp.IsTinted(inst.primaryTint), "default instance is untinted");

            inst.primaryTint = Color.red;
            Assert.AreEqual(Color.red, inst.TintFor(PartSlot.Body), "no accent → primary");

            inst.SetAccent(PartSlot.LeftHand, Color.blue);
            Assert.AreEqual(Color.blue, inst.TintFor(PartSlot.LeftHand), "accent overrides the primary");
            Assert.AreEqual(Color.red, inst.TintFor(PartSlot.RightHand), "unaccented slot stays primary");

            inst.SetAccent(PartSlot.LeftHand, Color.green); // overwrite, not duplicate
            Assert.AreEqual(Color.green, inst.TintFor(PartSlot.LeftHand));
            Assert.AreEqual(1, inst.accentTints.Count, "overwrite does not duplicate the slot");

            Assert.IsTrue(inst.ClearAccent(PartSlot.LeftHand));
            Assert.AreEqual(Color.red, inst.TintFor(PartSlot.LeftHand), "cleared accent → back to primary");
        }

        [Test]
        public void TintPreset_RegionModel_StoresColorsAndMask()
        {
            // Region-tint model (ADR-040): primary/secondary/accent color slots + an optional grayscale region mask.
            var preset = ScriptableObject.CreateInstance<TintPreset>();
            Assert.IsFalse(preset.HasMask, "no mask by default → solid primary");

            preset.SetColors(Color.red, Color.black, Color.yellow);
            Assert.AreEqual(Color.red, preset.PrimaryColor);
            Assert.AreEqual(Color.black, preset.SecondaryColor);
            Assert.AreEqual(Color.yellow, preset.AccentColor);

            var mask = new Texture2D(2, 2);
            preset.SetMask(mask);
            Assert.IsTrue(preset.HasMask, "mask set → patterned");
            Assert.AreSame(mask, preset.RegionMask);

            Object.DestroyImmediate(mask);
            Object.DestroyImmediate(preset);
        }
    }
}
