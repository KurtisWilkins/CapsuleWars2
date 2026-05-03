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

        [Tooltip("One mount per visual slot on the prefab. Add only the slots this unit exposes.")]
        [SerializeField] private List<SlotMount> mounts = new();

        private void Start()
        {
            if (definition != null) Apply(definition);
        }

        /// <summary>
        /// Apply a definition to the configured mounts. Mounts whose slot is
        /// not assigned in the definition are cleared.
        /// </summary>
        public void Apply(UnitDefinition_SO def)
        {
            if (def == null) return;

            // Reset all mounts first so unassigned slots end up empty.
            foreach (var mount in mounts) mount.Clear();

            foreach (var assignment in def.Parts)
            {
                var mount = FindMount(assignment.slot);
                if (mount == null) continue;
                mount.Apply(assignment.part, def.Palette);
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
