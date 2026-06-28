using System;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>A per-part-slot tint override — the accent-map entry shared by EquipmentInstance and TintPreset.</summary>
    [Serializable]
    public struct PartTint
    {
        public PartSlot slot;
        public Color color;

        public PartTint(PartSlot slot, Color color)
        {
            this.slot = slot;
            this.color = color;
        }
    }

    /// <summary>
    /// Pure derivation of a 3-stop tint ramp (shadow → mid → high) from a single tint color. Used by the runtime
    /// applier (EquipmentInstance.primaryTint → shader ramp) and by TintPreset capture, so both share one definition
    /// of "what a color's ramp is." The ramp drives the URP luminance-ramp shader: a grayscale part's luminance is
    /// remapped across these three colors. The IDENTITY ramp (black → mid-gray → white) reproduces the grayscale
    /// unchanged — so an UNTINTED part (primaryTint alpha ≤ 0) looks exactly as authored ("preserve at neutral").
    /// </summary>
    public static class TintRamp
    {
        /// <summary>The identity ramp — grayscale passes through unchanged (the shader's neutral default).</summary>
        public static readonly Color IdentityShadow = Color.black;
        public static readonly Color IdentityMid = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color IdentityHigh = Color.white;

        public const float ShadowMul = 0.30f;     // how dark the shadow stop goes (× the tint)
        public const float HighWhiteMix = 0.55f;  // how far the high stop lerps toward white

        /// <summary>Derive shadow/mid/high from one tint. Untinted (alpha ≤ 0) → the identity ramp.</summary>
        public static void FromColor(Color tint, out Color shadow, out Color mid, out Color high)
        {
            if (tint.a <= 0f)
            {
                shadow = IdentityShadow;
                mid = IdentityMid;
                high = IdentityHigh;
                return;
            }

            var c = new Color(tint.r, tint.g, tint.b, 1f);
            shadow = new Color(c.r * ShadowMul, c.g * ShadowMul, c.b * ShadowMul, 1f);
            mid = c;
            high = Color.Lerp(c, Color.white, HighWhiteMix);
            high.a = 1f;
        }

        /// <summary>True if this tint is "set" (opaque enough to override the grayscale).</summary>
        public static bool IsTinted(Color tint) => tint.a > 0f;
    }
}
