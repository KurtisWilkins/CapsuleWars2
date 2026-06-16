using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Units
{
    /// <summary>
    /// Master list of customization options the player can own: every
    /// <see cref="BodyPart_SO"/> and <see cref="Palette_SO"/>, each with an
    /// unlock cost and a "starter" flag (owned from the start — Docs/02 §84:
    /// 2 bodies, 2 hands, 2 feet, 1 palette). Pure data + lookups; the
    /// profile-aware operations (seeding, filtering by ownership) live in
    /// Persistence where both the catalog and the save profile are visible.
    /// </summary>
    [CreateAssetMenu(fileName = "PartCatalog", menuName = "CapsuleWars/Part Catalog", order = 4)]
    public class PartCatalog_SO : ScriptableObject, IPartDatabase
    {
        [System.Serializable]
        public class PartEntry
        {
            public BodyPart_SO part;
            [Min(0)] public int cost = 2;
            [Tooltip("Owned from the start, not purchased.")]
            public bool starter;
        }

        [System.Serializable]
        public class PaletteEntry
        {
            public Palette_SO palette;
            [Min(0)] public int cost = 1;
            public bool starter;
        }

        [SerializeField] private List<PartEntry> parts = new List<PartEntry>();
        [SerializeField] private List<PaletteEntry> palettes = new List<PaletteEntry>();

        public IReadOnlyList<PartEntry> Parts => parts;
        public IReadOnlyList<PaletteEntry> Palettes => palettes;

        // --- IPartDatabase ---

        public BodyPart_SO GetPart(string partId)
        {
            if (string.IsNullOrEmpty(partId)) return null;
            for (int i = 0; i < parts.Count; i++)
                if (parts[i]?.part != null && parts[i].part.PartId == partId) return parts[i].part;
            return null;
        }

        public Palette_SO GetPalette(string paletteId)
        {
            if (string.IsNullOrEmpty(paletteId)) return null;
            for (int i = 0; i < palettes.Count; i++)
                if (palettes[i]?.palette != null && palettes[i].palette.PaletteId == paletteId) return palettes[i].palette;
            return null;
        }

        /// <summary>Cost to unlock a part, or -1 if the id isn't in the catalog.</summary>
        public int GetPartCost(string partId)
        {
            for (int i = 0; i < parts.Count; i++)
                if (parts[i]?.part != null && parts[i].part.PartId == partId) return parts[i].cost;
            return -1;
        }

        public int GetPaletteCost(string paletteId)
        {
            for (int i = 0; i < palettes.Count; i++)
                if (palettes[i]?.palette != null && palettes[i].palette.PaletteId == paletteId) return palettes[i].cost;
            return -1;
        }

        /// <summary>All parts authored for the given slot (ownership-agnostic).</summary>
        public IEnumerable<BodyPart_SO> PartsForSlot(PartSlot slot)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i]?.part;
                if (p != null && p.Slot == slot) yield return p;
            }
        }

        public IEnumerable<string> StarterPartIds()
        {
            for (int i = 0; i < parts.Count; i++)
                if (parts[i] != null && parts[i].starter && parts[i].part != null) yield return parts[i].part.PartId;
        }

        public IEnumerable<string> StarterPaletteIds()
        {
            for (int i = 0; i < palettes.Count; i++)
                if (palettes[i] != null && palettes[i].starter && palettes[i].palette != null) yield return palettes[i].palette.PaletteId;
        }
    }
}
