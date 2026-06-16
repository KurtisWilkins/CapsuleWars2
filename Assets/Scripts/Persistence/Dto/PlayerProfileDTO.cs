using System;
using System.Collections.Generic;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// Account-wide meta-progression (Docs/12_RoguelikeRun.md §82, 09/13). Holds
    /// the player's unlock-point balance and the set of customization options
    /// (parts, palettes) they own. Lives inside <see cref="LegacyProfileDTO"/>
    /// and is account-wide, not per-unit.
    /// Points are earned on run completion and spent in the customization screen;
    /// the random unit generator and customization UI draw only from unlocked ids.
    /// </summary>
    [Serializable]
    public class PlayerProfileDTO
    {
        public int UnlockPoints;
        public List<string> UnlockedPartIds = new List<string>();
        public List<string> UnlockedPaletteIds = new List<string>();

        public bool HasPart(string partId) =>
            !string.IsNullOrEmpty(partId) && UnlockedPartIds != null && UnlockedPartIds.Contains(partId);

        public bool HasPalette(string paletteId) =>
            !string.IsNullOrEmpty(paletteId) && UnlockedPaletteIds != null && UnlockedPaletteIds.Contains(paletteId);

        /// <summary>Grant unlock points (e.g. on run completion). Ignores non-positive amounts.</summary>
        public void AddPoints(int amount)
        {
            if (amount > 0) UnlockPoints += amount;
        }

        /// <summary>
        /// Grant a part for free (e.g. starter seeding) without spending points.
        /// Idempotent; ignores null/empty ids.
        /// </summary>
        public void GrantPart(string partId)
        {
            if (string.IsNullOrEmpty(partId)) return;
            if (UnlockedPartIds == null) UnlockedPartIds = new List<string>();
            if (!UnlockedPartIds.Contains(partId)) UnlockedPartIds.Add(partId);
        }

        public void GrantPalette(string paletteId)
        {
            if (string.IsNullOrEmpty(paletteId)) return;
            if (UnlockedPaletteIds == null) UnlockedPaletteIds = new List<string>();
            if (!UnlockedPaletteIds.Contains(paletteId)) UnlockedPaletteIds.Add(paletteId);
        }

        /// <summary>
        /// Spend points to unlock a part. Returns false (no spend) if the id is
        /// empty, already owned, the cost is negative, or there aren't enough
        /// points. On success deducts the cost and records the part.
        /// </summary>
        public bool TryUnlockPart(string partId, int cost)
        {
            if (string.IsNullOrEmpty(partId) || cost < 0 || HasPart(partId) || UnlockPoints < cost) return false;
            UnlockPoints -= cost;
            GrantPart(partId);
            return true;
        }

        public bool TryUnlockPalette(string paletteId, int cost)
        {
            if (string.IsNullOrEmpty(paletteId) || cost < 0 || HasPalette(paletteId) || UnlockPoints < cost) return false;
            UnlockPoints -= cost;
            GrantPalette(paletteId);
            return true;
        }
    }
}
