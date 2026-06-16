using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence.Dto;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Per-slot appearance editing for a <see cref="UnitDTO"/> — the data layer
    /// behind the asymmetric-mixing UI (Docs/02_UnitSystem.md §16): each slot
    /// (incl. LeftHand vs RightHand, LeftFoot vs RightFoot) is set independently.
    /// <see cref="TrySetUnlockedPart"/> enforces slot-match + ownership so the
    /// editor can only assign parts the player has unlocked.
    /// </summary>
    public static class UnitAppearance
    {
        /// <summary>The part id assigned to <paramref name="slot"/>, or null.</summary>
        public static string GetPart(UnitDTO dto, PartSlot slot)
        {
            if (dto?.Parts == null) return null;
            for (int i = 0; i < dto.Parts.Count; i++)
                if (dto.Parts[i] != null && dto.Parts[i].slot == slot) return dto.Parts[i].partId;
            return null;
        }

        /// <summary>
        /// Set (or clear, with a null/empty id) the part for one slot, replacing
        /// any existing assignment for that slot. Other slots are untouched — so
        /// left and right are fully independent.
        /// </summary>
        public static void SetPart(UnitDTO dto, PartSlot slot, string partId)
        {
            if (dto == null) return;
            if (dto.Parts == null) dto.Parts = new List<UnitPartDTO>();

            for (int i = dto.Parts.Count - 1; i >= 0; i--)
                if (dto.Parts[i] != null && dto.Parts[i].slot == slot) dto.Parts.RemoveAt(i);

            if (!string.IsNullOrEmpty(partId)) dto.Parts.Add(new UnitPartDTO(slot, partId));
        }

        /// <summary>
        /// Assign a part to a slot only if it exists in the catalog, is authored
        /// for that slot, and the player owns it. Returns false (no change) otherwise.
        /// </summary>
        public static bool TrySetUnlockedPart(UnitDTO dto, PlayerProfileDTO profile, PartCatalog_SO catalog,
                                              PartSlot slot, string partId)
        {
            if (dto == null || profile == null || catalog == null || string.IsNullOrEmpty(partId)) return false;

            var part = catalog.GetPart(partId);
            if (part == null || part.Slot != slot) return false;   // unknown or wrong slot
            if (!profile.HasPart(partId)) return false;            // not unlocked

            SetPart(dto, slot, partId);
            return true;
        }
    }
}
