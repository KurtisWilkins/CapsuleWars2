using System.Collections.Generic;
using UnityEngine;

namespace CapsuleWars.Data.Equipment
{
    /// <summary>
    /// A reusable, saveable tint as a project asset (the persistence layer for the tint milestone). Stores the
    /// explicit 3-stop ramp (<see cref="shadow"/>/<see cref="mid"/>/<see cref="high"/>) the URP luminance-ramp shader
    /// consumes, plus an optional per-part accent map mirroring <see cref="EquipmentInstance"/>'s tint shape. Presets
    /// are referenced across units now and by ThemeProfile in the NEXT milestone.
    ///
    /// Color NEVER lives on the equipment Definition — only here (reusable) and on the Instance (per-unit). A captured
    /// preset's ramp is derived from an instance's <c>primaryTint</c> via <see cref="TintRamp"/>, so capture→apply
    /// round-trips to the same look (the instance is single-color; <see cref="ApplyTo"/> uses <c>mid</c> as the
    /// representative primary).
    /// </summary>
    [CreateAssetMenu(fileName = "TintPreset", menuName = "CapsuleWars/Tint Preset", order = 20)]
    public class TintPreset : ScriptableObject
    {
        [SerializeField] private Color shadow = TintRamp.IdentityShadow;
        [SerializeField] private Color mid = TintRamp.IdentityMid;
        [SerializeField] private Color high = TintRamp.IdentityHigh;

        [Tooltip("Optional per-part-slot accent overrides (mirrors EquipmentInstance.accentTints).")]
        [SerializeField] private List<PartTint> accentTints = new List<PartTint>();

        public Color Shadow => shadow;
        public Color Mid => mid;
        public Color High => high;
        public IReadOnlyList<PartTint> AccentTints => accentTints;

        /// <summary>Apply this preset onto an instance's runtime tint: <c>primaryTint</c> = <see cref="mid"/> (the
        /// representative color the applier re-derives a ramp from), and the accent map is copied.</summary>
        public void ApplyTo(EquipmentInstance instance)
        {
            if (instance == null) return;
            instance.primaryTint = mid;
            instance.accentTints = new List<PartTint>(accentTints);
        }

        /// <summary>Capture an instance's current tint into this preset: the ramp is derived from
        /// <c>instance.primaryTint</c> via <see cref="TintRamp"/>, and the accent map is copied. This is what makes a
        /// previewed color "stick" as an asset.</summary>
        public void CaptureFrom(EquipmentInstance instance)
        {
            if (instance == null) return;
            TintRamp.FromColor(instance.primaryTint, out shadow, out mid, out high);
            accentTints = instance.accentTints != null
                ? new List<PartTint>(instance.accentTints)
                : new List<PartTint>();
        }

        /// <summary>Directly set the ramp (e.g. a hand-authored preset). Editor/tools use this; runtime uses capture.</summary>
        public void SetRamp(Color newShadow, Color newMid, Color newHigh)
        {
            shadow = newShadow;
            mid = newMid;
            high = newHigh;
        }
    }
}
