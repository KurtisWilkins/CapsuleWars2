using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using UnityEngine;

namespace CapsuleWars.Units.Customization
{
    /// <summary>
    /// Applies a UnitDefinition_SO to a 3D capsule prefab by swapping meshes
    /// and tinting materials per slot mount. One MonoBehaviour per unit root.
    ///
    /// Wire SlotMounts in the inspector — each mount points to a MeshFilter
    /// and MeshRenderer that lives on a child transform (typically a bone or
    /// bone-child for floating-limb rigs). Apply() runs in Start by default
    /// and can be re-run any time the definition changes (equip swap, etc.).
    /// </summary>
    public class UnitCustomization : MonoBehaviour
    {
        [Tooltip("Default unit definition applied in Start. Optional — can be applied later via code.")]
        [SerializeField] private UnitDefinition_SO definition;

        /// <summary>
        /// The definition currently applied to this unit — the inspector
        /// default, or whatever was last passed to <see cref="Apply"/>.
        /// Read by UnitFactory.FromUnit to capture the unit's visual identity.
        /// </summary>
        public UnitDefinition_SO Definition => definition;

        [Tooltip("One mount per visual slot on the prefab. Add only the slots this unit exposes.")]
        [SerializeField] private List<SlotMount> mounts = new();

        // The parts currently applied (non-null only), recorded every Apply/ApplyParts so callers
        // (e.g. the customization screen) can read the live loadout back to edit it incrementally
        // and capture it for persistence — UnitCustomization is otherwise write-only into the mounts.
        private readonly List<PartAssignment> appliedParts = new();
        private Palette_SO appliedPalette;

        /// <summary>
        /// The part assignments currently applied to this unit (non-null parts only). Read back by the
        /// customization screen to show the current body loadout, edit it, and persist it (dto.Parts).
        /// </summary>
        public IReadOnlyList<PartAssignment> AppliedParts => appliedParts;

        /// <summary>The palette last passed to <see cref="ApplyParts"/> (so edits re-apply with the same tint).</summary>
        public Palette_SO AppliedPalette => appliedPalette;

        /// <summary>
        /// The <see cref="PartSlot"/>s this prefab actually exposes a mount for — i.e. the slots that can
        /// show a part. The customization screen renders cosmetic slots only for these.
        /// </summary>
        public IEnumerable<PartSlot> MountedSlots
        {
            get
            {
                for (int i = 0; i < mounts.Count; i++) yield return mounts[i].Slot;
            }
        }

        private void Awake()
        {
            // Apply in Awake (not Start) so meshes are present before the
            // first render. Late assignment caused visible 1-frame artifacts:
            // empty slot MeshFilters at frame 0, then bones positioned by the
            // first Animator tick at locations the missing meshes would have
            // anchored, then meshes pop in at frame 1.
            if (definition != null) Apply(definition);
        }

        /// <summary>
        /// Apply a definition to the configured mounts. Mounts whose slot is
        /// not assigned in the definition are cleared.
        /// </summary>
        public void Apply(UnitDefinition_SO def)
        {
            if (def == null) return;

            // Track the applied definition so it can be read back (e.g. by
            // UnitFactory.FromUnit) and re-applied after an equip/customization change.
            definition = def;
            ApplyParts(def.Parts, def.Palette);
        }

        /// <summary>
        /// Apply an explicit set of slot→part assignments + palette to the mounts
        /// (for generated/customized units built from a UnitDTO's part ids, rather
        /// than a whole-unit definition). Unassigned slots are cleared.
        /// </summary>
        public void ApplyParts(IReadOnlyList<PartAssignment> parts, Palette_SO palette)
        {
            // Record the applied set for read-back (capture/incremental editing). Re-applying a
            // smaller set is how a slot gets "unequipped": the missing slot is cleared below.
            appliedParts.Clear();
            appliedPalette = palette;

            // Reset all mounts first so unassigned slots end up empty.
            foreach (var mount in mounts) mount.Clear();
            if (parts == null) return;

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].part != null) appliedParts.Add(parts[i]);
                var mount = FindMount(parts[i].slot);
                if (mount == null) continue;
                mount.Apply(parts[i].part, palette);
            }
        }

        private SlotMount FindMount(PartSlot slot)
        {
            for (int i = 0; i < mounts.Count; i++)
            {
                if (mounts[i].Slot == slot) return mounts[i];
            }
            return null;
        }
    }

    /// <summary>
    /// One visual mount point on a unit prefab. Holds references to a
    /// MeshFilter (gets the part's mesh) and MeshRenderer (gets palette tint
    /// via _BaseColor on a MaterialPropertyBlock).
    /// </summary>
    [Serializable]
    public class SlotMount
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private PartSlot slot;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        [Tooltip("Which palette color this mount picks up, or None to keep the part's default materials.")]
        [SerializeField] private PaletteRole paletteRole = PaletteRole.None;

        public PartSlot Slot => slot;

        public void Clear()
        {
            if (meshFilter != null) meshFilter.sharedMesh = null;
        }

        public void Apply(BodyPart_SO part, Palette_SO palette)
        {
            if (part == null || meshFilter == null) return;

            meshFilter.sharedMesh = part.Mesh;

            if (meshRenderer != null && part.DefaultMaterials != null && part.DefaultMaterials.Count > 0)
            {
                var mats = new Material[part.DefaultMaterials.Count];
                for (int i = 0; i < mats.Length; i++) mats[i] = part.DefaultMaterials[i];
                meshRenderer.sharedMaterials = mats;
            }

            ApplyPaletteTint(palette);
        }

        private void ApplyPaletteTint(Palette_SO palette)
        {
            if (palette == null || paletteRole == PaletteRole.None || meshRenderer == null) return;

            var color = paletteRole switch
            {
                PaletteRole.Body => palette.Body,
                PaletteRole.Limbs => palette.Limbs,
                PaletteRole.Accent => palette.Accent,
                _ => Color.white
            };

            var mpb = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, color);
            meshRenderer.SetPropertyBlock(mpb);
        }
    }
}
