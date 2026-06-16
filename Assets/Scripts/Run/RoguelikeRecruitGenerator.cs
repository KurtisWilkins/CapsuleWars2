using CapsuleWars.Persistence.Dto;
using UnityEngine;

namespace CapsuleWars.Run
{
    /// <summary>
    /// STUB for M9's in-run random unit generator (Docs/12_RoguelikeRun.md §54).
    /// Produces a minimal roguelike-only <see cref="UnitDTO"/> so the end-of-run
    /// recruit flow is demonstrable now: a unique id, a name from a small pool,
    /// and one fixed <c>UnitDefinition</c> for visuals. M9 replaces this with the
    /// real class / element / parts / abilities / rarity rolls.
    /// </summary>
    public static class RoguelikeRecruitGenerator
    {
        // The one definition currently in the catalog; M9 will draw from the
        // unlocked part pool instead.
        private const string StubDefinitionId = "unit_sample";

        private static readonly string[] Names =
            { "Ash", "Bryn", "Cael", "Dax", "Eira", "Finn", "Gale", "Hale" };

        /// <summary>
        /// Build a roguelike-only unit. <paramref name="floor"/> + <paramref name="index"/>
        /// make the id unique within a run; the name is chosen deterministically
        /// from them (no RNG, so it's test-friendly).
        /// </summary>
        public static UnitDTO Generate(int floor, int index)
        {
            string id = $"rogue_{floor}_{index}";
            string name = Names[Mathf.Abs(floor * 3 + index) % Names.Length];
            return new UnitDTO(id, name, StubDefinitionId);
        }
    }
}
