using CapsuleWars.Combat.Deployment;
using CapsuleWars.Data.Arena;
using UnityEngine;

namespace CapsuleWars.UI.Arena
{
    /// <summary>
    /// Pure placement math for the runtime arena builder — checkerboard parity, terrain → block role, and
    /// cell → world centers. No scene dependencies, so it's EditMode-testable; <c>ArenaBuilder</c> consumes it.
    /// The floor is one block per grid cell aligned 1:1 with <see cref="DeploymentGridConfig"/> cells, so every
    /// visual tile IS a deployment cell and the CellState highlight overlays the same grid.
    /// </summary>
    public static class ArenaLayout
    {
        /// <summary>Light checkerboard cell when (col+row) is even, dark when odd (chess-style).</summary>
        public static bool IsFloorA(GridCoord c) => ((c.col + c.row) & 1) == 0;

        /// <summary>The floor/obstacle role for a cell: Impassable → Obstacle, otherwise a checkerboard floor tile.</summary>
        public static ArenaBlock BlockFor(TerrainType terrain, GridCoord c)
        {
            if (terrain == TerrainType.Impassable) return ArenaBlock.Obstacle;
            return IsFloorA(c) ? ArenaBlock.FloorA : ArenaBlock.FloorB;
        }

        /// <summary>Hazard cells keep a walkable floor tile plus an overlaid marker.</summary>
        public static bool NeedsHazardMarker(TerrainType terrain) => terrain == TerrainType.Hazard;

        /// <summary>World-space center of a cell (XZ from the grid; Y at the grid plane).</summary>
        public static Vector3 CellCenter(DeploymentGridConfig config, GridCoord c) =>
            config != null ? config.CellToWorld(c) : Vector3.zero;
    }
}
