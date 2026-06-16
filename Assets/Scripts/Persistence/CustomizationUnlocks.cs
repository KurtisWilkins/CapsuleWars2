using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence.Dto;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// Bridges the <see cref="PartCatalog_SO"/> (Data: what exists + costs) with a
    /// <see cref="PlayerProfileDTO"/> (save: what's owned). Lives in Persistence
    /// because it's the assembly that sees both. Used by the customization screen
    /// (unlock/spend) and the random unit generator (draw from owned parts).
    /// </summary>
    public static class CustomizationUnlocks
    {
        /// <summary>Grant every starter part/palette in the catalog (free). Idempotent.</summary>
        public static void SeedStarters(PartCatalog_SO catalog, PlayerProfileDTO profile)
        {
            if (catalog == null || profile == null) return;
            foreach (var id in catalog.StarterPartIds()) profile.GrantPart(id);
            foreach (var id in catalog.StarterPaletteIds()) profile.GrantPalette(id);
        }

        /// <summary>Unlock a catalog part by spending its catalog cost. False if not in catalog or unaffordable/owned.</summary>
        public static bool TryUnlockPart(PartCatalog_SO catalog, PlayerProfileDTO profile, string partId)
        {
            if (catalog == null || profile == null) return false;
            int cost = catalog.GetPartCost(partId);
            if (cost < 0) return false;
            return profile.TryUnlockPart(partId, cost);
        }

        public static bool TryUnlockPalette(PartCatalog_SO catalog, PlayerProfileDTO profile, string paletteId)
        {
            if (catalog == null || profile == null) return false;
            int cost = catalog.GetPaletteCost(paletteId);
            if (cost < 0) return false;
            return profile.TryUnlockPalette(paletteId, cost);
        }

        /// <summary>Catalog parts for a slot that the profile owns (for the customization picker + random generator).</summary>
        public static List<BodyPart_SO> UnlockedPartsForSlot(PartCatalog_SO catalog, PlayerProfileDTO profile, PartSlot slot)
        {
            var result = new List<BodyPart_SO>();
            if (catalog == null || profile == null) return result;
            foreach (var part in catalog.PartsForSlot(slot))
                if (part != null && profile.HasPart(part.PartId)) result.Add(part);
            return result;
        }
    }
}
