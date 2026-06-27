using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence.Dto;

namespace CapsuleWars.Persistence
{
    /// <summary>
    /// In-run random unit generation (Docs/12_RoguelikeRun.md §54) — the real
    /// version of the M8 <c>RoguelikeRecruitGenerator</c> stub. Builds a
    /// roguelike-only <see cref="UnitDTO"/> by drawing one part per slot from the
    /// player's UNLOCKED pool (+ an owned palette). Seeded via the passed
    /// <see cref="Random"/> so a run's units are reproducible.
    ///
    /// Class/element/abilities/equipment rolls are still TODO (those DTO fields
    /// don't exist yet); this covers the visual identity that M9 unlocks gate.
    /// </summary>
    public static class RandomUnitGenerator
    {
        // Head is included so generated units get a head from the unlocked pool (the default sphere is a
        // starter, so it's always available). HeadProp stays excluded — head props are optional.
        private static readonly PartSlot[] BuildSlots =
        {
            PartSlot.Body, PartSlot.Head, PartSlot.LeftHand, PartSlot.RightHand, PartSlot.LeftFoot, PartSlot.RightFoot
        };

        private static readonly string[] Names =
        {
            "Ash", "Bryn", "Cael", "Dax", "Eira", "Finn", "Gale", "Hale", "Iris", "Jmet"
        };

        public static UnitDTO Generate(PartCatalog_SO catalog, PlayerProfileDTO profile,
                                       System.Random rng, int floor, int index)
        {
            string id = $"rogue_{floor}_{index}";
            string name = Names[Math.Abs(floor * 7 + index) % Names.Length];
            var dto = new UnitDTO(id, name, null);

            if (catalog == null || profile == null) return dto;
            var r = rng ?? new System.Random(floor * 100 + index);

            foreach (var slot in BuildSlots)
            {
                var options = CustomizationUnlocks.UnlockedPartsForSlot(catalog, profile, slot);
                if (options.Count == 0) continue;
                var pick = options[r.Next(options.Count)];
                dto.Parts.Add(new UnitPartDTO(slot, pick.PartId));
            }

            var palettes = new List<string>();
            foreach (var pe in catalog.Palettes)
                if (pe?.palette != null && profile.HasPalette(pe.palette.PaletteId)) palettes.Add(pe.palette.PaletteId);
            if (palettes.Count > 0) dto.PaletteId = palettes[r.Next(palettes.Count)];

            return dto;
        }
    }
}
