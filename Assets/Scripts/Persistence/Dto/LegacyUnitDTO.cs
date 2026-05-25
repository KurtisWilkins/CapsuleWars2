using System;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// One legacy (persistent) unit. The Id field must match a unit
    /// prefab's <c>UnitRoot.unitId</c> for stats to flow from battles
    /// into this profile.
    /// M9+ will expand this with parts, palette, abilities, and a
    /// proper UnitDefinition reference for spawn-from-DTO.
    /// </summary>
    [Serializable]
    public class LegacyUnitDTO
    {
        /// <summary>Stable identifier; matches UnitRoot.unitId on the prefab.</summary>
        public string Id;

        public string DisplayName;

        /// <summary>ISO 8601 timestamp of when this unit was first promoted to legacy.</summary>
        public string CreatedUtc;

        public LifetimeStatsDTO Lifetime = new LifetimeStatsDTO();

        public LegacyUnitDTO() { }

        public LegacyUnitDTO(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
            CreatedUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
