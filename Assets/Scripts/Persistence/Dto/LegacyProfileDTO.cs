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

        /// <summary>
        /// Soft cap on stored legacy units (Docs/13_LegacyMode.md). The field
        /// initializer (100) is the backward-compatible default: old save files
        /// that predate this field leave it at 100 on load (the JSON key is
        /// simply absent). Recruiting past the cap requires releasing a unit.
        /// </summary>
        public int RosterCap = 100;

        /// <summary>
        /// Account-wide meta-progression (unlock points + owned customization
        /// options). The initializer is the backward-compatible default: old
        /// saves without this key load a fresh profile. See Docs/13_LegacyMode.md.
        /// </summary>
        public PlayerProfileDTO PlayerProfile = new PlayerProfileDTO();

        public List<LegacyUnitDTO> Units = new List<LegacyUnitDTO>();

        public LegacyProfileDTO()
        {
            CreatedUtc = DateTime.UtcNow.ToString("o");
        }

        /// <summary>Effective cap, guarding a zero/negative value from a malformed save.</summary>
        public int EffectiveCap => RosterCap > 0 ? RosterCap : 100;

        public int Count => Units?.Count ?? 0;

        public bool IsAtCap => Count >= EffectiveCap;

        public bool Contains(string id) => FindById(id) != null;

        public LegacyUnitDTO FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || Units == null) return null;
            for (int i = 0; i < Units.Count; i++)
            {
                if (Units[i] != null && Units[i].Id == id) return Units[i];
            }
            return null;
        }

        /// <summary>
        /// Add a unit to the roster. Returns false (no-op) if the unit is null,
        /// already present (by Id), or the roster is at its cap — callers should
        /// surface a "release a unit" prompt on a cap rejection.
        /// </summary>
        public bool TryAdd(LegacyUnitDTO unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.Id)) return false;
            if (Units == null) Units = new List<LegacyUnitDTO>();
            if (Contains(unit.Id)) return false;
            if (IsAtCap) return false;
            Units.Add(unit);
            return true;
        }

        /// <summary>Remove a unit by Id. Returns true if one was removed.</summary>
        public bool Release(string id)
        {
            if (string.IsNullOrEmpty(id) || Units == null) return false;
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                if (Units[i] != null && Units[i].Id == id)
                {
                    Units.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}
