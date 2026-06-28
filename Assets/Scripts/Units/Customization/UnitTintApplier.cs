using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Equipment;
using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Pushes a unit's runtime TINT to its part renderers (tint milestone). Derives a 3-stop luminance ramp from the
    /// tint (<see cref="TintRamp"/>) and writes <c>_TintShadow/_TintMid/_TintHigh</c> to each part's renderer via a
    /// <see cref="MaterialPropertyBlock"/> — so NO per-unit material instances are created (one shared material per
    /// part keeps the asset library as the single source of truth; color is render-time only).
    ///
    /// Per-slot accents override the primary on specific <see cref="PartSlot"/>s; slot→renderer comes from
    /// <see cref="UnitCustomization.Mounts"/> (falls back to all child <see cref="MeshRenderer"/>s, primary-only).
    /// <c>[ExecuteAlways]</c> + OnValidate previews the serialized tint in the Scene view WITHOUT entering play mode;
    /// the serialized fields are what persist the previewed color (MPB values alone do not). Saving it as a reusable
    /// <see cref="TintPreset"/> asset is the editor's job (see UnitTintApplierEditor).
    ///
    /// TRADEOFF: MaterialPropertyBlock disables the SRP Batcher for the tinted renderers. Fine for v1 unit counts
    /// (a handful of player + enemy units on screen). If on-screen counts grow large, move to GPU-instanced
    /// per-instance color properties — a documented follow-up, not implemented here.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UnitTintApplier : MonoBehaviour
    {
        private static readonly int ShadowId = Shader.PropertyToID("_TintShadow");
        private static readonly int MidId = Shader.PropertyToID("_TintMid");
        private static readonly int HighId = Shader.PropertyToID("_TintHigh");

        [Tooltip("Primary tint for all parts. Color.clear (alpha 0) = untinted (grayscale passes through).")]
        [SerializeField] private Color primaryTint = Color.clear;

        [Tooltip("Optional per-part-slot accent overrides (override the primary on those slots).")]
        [SerializeField] private List<PartTint> accentTints = new List<PartTint>();

        [Tooltip("Source of slot→renderer mounts. Auto-found on this GameObject if left empty.")]
        [SerializeField] private UnitCustomization customization;

        public Color PrimaryTint => primaryTint;
        public IReadOnlyList<PartTint> AccentTints => accentTints;

        private void OnEnable() => Apply();

        private void OnValidate()
        {
            // Live editor preview as the inspector tint changes — but only for a real scene instance,
            // never during prefab-asset import (gameObject.scene is invalid there), to avoid import-time MPB churn.
            if (isActiveAndEnabled && gameObject.scene.IsValid()) Apply();
        }

        /// <summary>Push the current tint to all part renderers via MaterialPropertyBlock.</summary>
        public void Apply()
        {
            if (customization == null) customization = GetComponent<UnitCustomization>();

            if (customization != null)
            {
                var mounts = customization.Mounts;
                for (int i = 0; i < mounts.Count; i++)
                {
                    var m = mounts[i];
                    if (m == null || m.Renderer == null) continue;
                    PushRamp(m.Renderer, TintForSlot(m.Slot));
                }
                return;
            }

            // Fallback: no mounts → tint every child renderer with the primary (no per-slot accents possible).
            var renderers = GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++) PushRamp(renderers[i], primaryTint);
        }

        /// <summary>The effective tint for a slot — its accent override if set, else the primary.</summary>
        public Color TintForSlot(PartSlot slot)
        {
            for (int i = 0; i < accentTints.Count; i++)
                if (accentTints[i].slot == slot) return accentTints[i].color;
            return primaryTint;
        }

        private void PushRamp(MeshRenderer renderer, Color tint)
        {
            TintRamp.FromColor(tint, out var shadow, out var mid, out var high);
            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(ShadowId, shadow);
            mpb.SetColor(MidId, mid);
            mpb.SetColor(HighId, high);
            renderer.SetPropertyBlock(mpb);
        }

        // ---------------- live recolor + per-unit data bridge ----------------

        /// <summary>Recolor the whole unit live (clears accents are untouched).</summary>
        public void Recolor(Color primary)
        {
            primaryTint = primary;
            Apply();
        }

        /// <summary>Set (or overwrite) a per-slot accent and re-apply.</summary>
        public void SetAccent(PartSlot slot, Color color)
        {
            for (int i = 0; i < accentTints.Count; i++)
                if (accentTints[i].slot == slot) { accentTints[i] = new PartTint(slot, color); Apply(); return; }
            accentTints.Add(new PartTint(slot, color));
            Apply();
        }

        /// <summary>Runtime equip/spawn path: copy an EquipmentInstance's tint onto this applier and apply.</summary>
        public void ApplyFrom(EquipmentInstance instance)
        {
            if (instance == null) return;
            primaryTint = instance.primaryTint;
            accentTints = instance.accentTints != null ? new List<PartTint>(instance.accentTints) : new List<PartTint>();
            Apply();
        }

        /// <summary>Write this applier's current tint back onto an EquipmentInstance (the per-unit data side).</summary>
        public void CaptureInto(EquipmentInstance instance)
        {
            if (instance == null) return;
            instance.primaryTint = primaryTint;
            instance.accentTints = new List<PartTint>(accentTints);
        }

        /// <summary>Capture the current tint into a (reusable) TintPreset asset — bridges via a temp instance.</summary>
        public void SaveToPreset(TintPreset preset)
        {
            if (preset == null) return;
            var temp = new EquipmentInstance();
            CaptureInto(temp);
            preset.CaptureFrom(temp);
        }

        /// <summary>Load a TintPreset onto this applier and apply (preview/equip).</summary>
        public void LoadPreset(TintPreset preset)
        {
            if (preset == null) return;
            var temp = new EquipmentInstance();
            preset.ApplyTo(temp);
            ApplyFrom(temp);
        }
    }
}
