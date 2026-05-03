using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Authoring data for one unit configuration: which body part fills each
    /// slot, plus the palette. Used to spawn a customized unit instance.
    /// Runtime stats and abilities are layered on top by other systems.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDefinition", menuName = "CapsuleWars/Unit Definition", order = 1)]
    public class UnitDefinition_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and the Database lookup.")]
        [SerializeField] private string unitId;

        [Tooltip("I2 term key for the unit's display name. Random names use UnitNameGenerator instead.")]
        [SerializeField] private string nameTermKey;

        [Tooltip("Part assignments per slot. A unit may leave a slot empty (e.g. no head prop).")]
        [SerializeField] private List<PartAssignment> parts = new();

        [Tooltip("Palette applied to slot mounts that opt in via PaletteRole.")]
        [SerializeField] private Palette_SO palette;

        public string UnitId => unitId;
        public string NameTermKey => nameTermKey;
        public IReadOnlyList<PartAssignment> Parts => parts;
        public Palette_SO Palette => palette;
    }

    /// <summary>
    /// One slot-to-part assignment within a UnitDefinition_SO.
    /// </summary>
    [Serializable]
    public struct PartAssignment
    {
        public PartSlot slot;
        public BodyPart_SO part;
    }
}
