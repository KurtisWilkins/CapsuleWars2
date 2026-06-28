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
        public void Preset_CaptureThenApply_RoundTripsToSameLook()
        {
            var src = new EquipmentInstance { primaryTint = Color.green };
            src.SetAccent(PartSlot.HeadProp, Color.magenta);

            var preset = ScriptableObject.CreateInstance<TintPreset>();
            preset.CaptureFrom(src);
            Assert.AreEqual(Color.green, preset.Mid, "captured mid == the painted primary");
            Assert.AreEqual(1, preset.AccentTints.Count);

            var dst = new EquipmentInstance();
            preset.ApplyTo(dst);
            Assert.AreEqual(src.primaryTint, dst.primaryTint, "primary transfers across units");
            Assert.AreEqual(Color.magenta, dst.TintFor(PartSlot.HeadProp), "accent transfers across units");

            // same primary → applier derives the same ramp → same on-screen look
            TintRamp.FromColor(src.primaryTint, out var s1, out var m1, out var h1);
            TintRamp.FromColor(dst.primaryTint, out var s2, out var m2, out var h2);
            Assert.AreEqual(m1, m2);
            Assert.AreEqual(s1, s2);
            Assert.AreEqual(h1, h2);

            Object.DestroyImmediate(preset);
        }

        [Test]
        public void Preset_AppliedToTwoUnits_ProducesIdenticalPrimary()
        {
            var preset = ScriptableObject.CreateInstance<TintPreset>();
            preset.CaptureFrom(new EquipmentInstance { primaryTint = new Color(0.1f, 0.6f, 0.9f, 1f) });

            var a = new EquipmentInstance();
            var b = new EquipmentInstance();
            preset.ApplyTo(a);
            preset.ApplyTo(b);
            Assert.AreEqual(a.primaryTint, b.primaryTint);

            Object.DestroyImmediate(preset);
        }
    }
}
