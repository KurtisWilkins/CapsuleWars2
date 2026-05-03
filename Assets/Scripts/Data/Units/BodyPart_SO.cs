using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// A single swappable part (body shape, hand variant, foot variant, head prop).
    /// Holds the mesh and a default material set; palette tinting is applied
    /// at runtime by UnitCustomization based on the slot mount's PaletteRole.
    /// </summary>
    [CreateAssetMenu(fileName = "BodyPart", menuName = "CapsuleWars/Body Part", order = 10)]
    public class BodyPart_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and the Database lookup.")]
        [SerializeField] private string partId;

        [Tooltip("I2 term key for the part's display name (e.g. Part.Hand.CrabClaw.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("Which slot this part is allowed to be assigned to.")]
        [SerializeField] private PartSlot slot = PartSlot.Body;

        [Tooltip("Mesh applied to the slot's MeshFilter at spawn time.")]
        [SerializeField] private Mesh mesh;

        [Tooltip("Default materials. UnitCustomization may override _BaseColor via the palette.")]
        [SerializeField] private Material[] defaultMaterials;

        public string PartId => partId;
        public string NameTermKey => nameTermKey;
        public PartSlot Slot => slot;
        public Mesh Mesh => mesh;
        public IReadOnlyList<Material> DefaultMaterials => defaultMaterials;
    }
}
