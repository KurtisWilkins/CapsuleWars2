using System;
using System.Collections.Generic;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// Top-level persisted profile. One file at
    /// <c>Application.persistentDataPath/legacy.json</c>.
    /// SaveVersion is bumped on schema changes; migrations live in
    /// <c>Persistence.Migrations</c> (added when first needed).
    /// </summary>
    [Serializable]
    public class LegacyProfileDTO
    {
        public int SaveVersion = 1;
        public string CreatedUtc;
        public List<LegacyUnitDTO> Units = new List<LegacyUnitDTO>();

        public LegacyProfileDTO()
        {
            CreatedUtc = DateTime.UtcNow.ToString("o");
        }

        public LegacyUnitDTO FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || Units == null) return null;
            for (int i = 0; i < Units.Count; i++)
            {
                if (Units[i] != null && Units[i].Id == id) return Units[i];
            }
            return null;
        }
    }
}
