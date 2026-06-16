using System;
using System.Collections.Generic;
using CapsuleWars.Core;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>One serialized slot→part assignment (by stable part id).</summary>
    [Serializable]
    public class UnitPartDTO
    {
        public PartSlot slot;
        public string partId;

        public UnitPartDTO() { }
        public UnitPartDTO(PartSlot slot, string partId) { this.slot = slot; this.partId = partId; }
    }

    /// <summary>One serialized slot→equipment assignment (by stable equipment id).</summary>
    [Serializable]
    public class UnitEquipmentDTO
    {
        public EquipmentSlot slot;
        public string equipmentId;

        public UnitEquipmentDTO() { }
        public UnitEquipmentDTO(EquipmentSlot slot, string equipmentId) { this.slot = slot; this.equipmentId = equipmentId; }
    }

    /// <summary>
    /// Serialized form of a unit (Docs/02_UnitSystem.md, Docs/14_Persistence.md).
    /// Holds identity plus visuals by stable id — either a whole-unit
    /// <c>UnitDefinition_SO</c> reference (<see cref="UnitDefinitionId"/>) OR an
    /// explicit per-slot part list (<see cref="Parts"/>) + <see cref="PaletteId"/>
    /// for generated/customized units. No Unity object references. Convert with
    /// <c>UnitFactory</c>.
    ///
    /// Adding <see cref="Parts"/>/<see cref="PaletteId"/> is backward compatible
    /// (absent in pre-M9 saves → empty → fall back to the definition's visuals),
    /// so <see cref="SaveVersion"/> stays at 1 (no migration needed).
    ///
    /// STILL DEFERRED (no runtime home yet): class/element/equipment/ability ids,
    /// level / evolutionTier.
    /// </summary>
    [Serializable]
    public class UnitDTO : IEquatable<UnitDTO>
    {
        /// <summary>Save schema version (Docs/14_Persistence.md). Bump on breaking change.</summary>
        public int SaveVersion = 1;

        /// <summary>Stable unit-instance identifier; matches <c>UnitRoot.UnitId</c> / <c>LegacyUnitDTO.Id</c>.</summary>
        public string Id;

        /// <summary>Display name (may be a player rename); falls back to the definition's name when empty.</summary>
        public string DisplayName;

        /// <summary>ID of the unit's <c>UnitDefinition_SO</c> (resolved via the unit-definition database). Used when <see cref="Parts"/> is empty.</summary>
        public string UnitDefinitionId;

        /// <summary>
        /// Explicit per-slot parts for a generated/customized unit. When non-empty,
        /// these take precedence over <see cref="UnitDefinitionId"/> for visuals.
        /// </summary>
        public List<UnitPartDTO> Parts = new List<UnitPartDTO>();

        /// <summary>Stable <c>Palette_SO</c> id for a generated/customized unit (optional).</summary>
        public string PaletteId;

        /// <summary>
        /// Equipped items by slot, as stable equipment ids (run-scoped loot).
        /// Resolved to <c>Equipment_SO</c> via an <c>IEquipmentDatabase</c> at
        /// spawn time (<c>UnitFactory</c>). Empty for unequipped units. Backward
        /// compatible: absent in pre-equipment saves → empty, so
        /// <see cref="SaveVersion"/> stays at 1 (no migration needed).
        /// </summary>
        public List<UnitEquipmentDTO> Equipment = new List<UnitEquipmentDTO>();

        public UnitDTO() { }

        public UnitDTO(string id, string displayName, string unitDefinitionId)
        {
            Id = id;
            DisplayName = displayName;
            UnitDefinitionId = unitDefinitionId;
        }

        /// <summary>
        /// Build the run-time identity DTO for a drafted legacy unit. Carries the
        /// legacy unit's Id (so battle stats flow back to the right profile via
        /// LegacyService) and its UnitDefinitionId (so visuals can be rebuilt).
        /// Returns null for a null legacy unit.
        /// </summary>
        public static UnitDTO FromLegacy(LegacyUnitDTO legacy)
        {
            if (legacy == null) return null;
            var dto = new UnitDTO(legacy.Id, legacy.DisplayName, legacy.UnitDefinitionId)
            {
                PaletteId = legacy.PaletteId,
            };
            if (legacy.Parts != null)
                foreach (var p in legacy.Parts)
                    if (p != null) dto.Parts.Add(new UnitPartDTO(p.slot, p.partId));
            return dto;
        }

        // Value equality over all serialized fields — lets round-trip tests
        // assert a DTO -> Unit -> DTO cycle reproduces the original. Reference
        // equality (e.g. the null checks in UnitFactory) is intentionally left
        // to the default == operator, which is not overloaded here.
        public bool Equals(UnitDTO other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return SaveVersion == other.SaveVersion
                && Id == other.Id
                && DisplayName == other.DisplayName
                && UnitDefinitionId == other.UnitDefinitionId;
        }

        public override bool Equals(object obj) => Equals(obj as UnitDTO);

        public override int GetHashCode() =>
            HashCode.Combine(SaveVersion, Id, DisplayName, UnitDefinitionId);
    }
}
