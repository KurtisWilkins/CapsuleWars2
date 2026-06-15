using System;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// Serialized form of a unit (Docs/02_UnitSystem.md, Docs/14_Persistence.md).
    /// Holds identity plus a by-ID reference to the unit's
    /// <c>UnitDefinition_SO</c> (visuals: parts, palette, default name) — no
    /// Unity object references, per the SO-reference-by-ID rule. Convert with
    /// <c>UnitFactory</c>.
    ///
    /// SCOPE NOTE: this is the M8-keystone slice. The following doc-specified
    /// fields are intentionally deferred, because the runtime/data layer can't
    /// yet source them and adding dead fields would be misleading:
    ///   - class, element(s), equipment, abilities by ID — the referenced SOs
    ///     have no stable IDs yet (only UnitDefinition_SO does).
    ///   - level / evolutionTier — no runtime home exists (no level field on
    ///     any controller yet).
    /// Add these alongside the SO-ID + level work, bumping <see cref="SaveVersion"/>
    /// and writing a migration when the schema changes.
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

        /// <summary>ID of the unit's <c>UnitDefinition_SO</c> (resolved via the unit-definition database).</summary>
        public string UnitDefinitionId;

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
            return new UnitDTO(legacy.Id, legacy.DisplayName, legacy.UnitDefinitionId);
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
