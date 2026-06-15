using System;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// One legacy (persistent) unit. The Id field must match a unit
    /// prefab's <c>UnitRoot.unitId</c> for stats to flow from battles
    /// into this profile.
    /// M9+ will expand this with parts, palette, and abilities; the
    /// <see cref="UnitDefinitionId"/> reference for spawn-from-DTO landed
    /// with the draft-into-run flow.
    /// </summary>
    [Serializable]
    public class LegacyUnitDTO
    {
        /// <summary>Stable identifier; matches UnitRoot.unitId on the prefab.</summary>
        public string Id;

        public string DisplayName;

        /// <summary>
        /// ID of the unit's <c>UnitDefinition_SO</c> (visuals: parts + palette),
        /// captured at promote time via <c>UnitFactory.FromUnit</c>. Used by the
        /// draft-into-run flow to reconstruct the unit. Optional and backward
        /// compatible: absent in pre-draft save files (deserializes to null),
        /// in which case a drafted unit keeps its identity but uses the base
        /// prefab's default visuals.
        /// </summary>
        public string UnitDefinitionId;

        /// <summary>ISO 8601 timestamp of when this unit was first promoted to legacy.</summary>
        public string CreatedUtc;

        public LifetimeStatsDTO Lifetime = new LifetimeStatsDTO();

        public LegacyUnitDTO() { }

        public LegacyUnitDTO(string id, string displayName, string unitDefinitionId = null)
        {
            Id = id;
            DisplayName = displayName;
            UnitDefinitionId = unitDefinitionId;
            CreatedUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
