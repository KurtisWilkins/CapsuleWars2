namespace CapsuleWars.Data.Arena
{
    /// <summary>
    /// The visual role a block plays on the runtime arena floor. Pure data (Data layer) so both the
    /// theme block set (which maps role → prefab/material) and the builder can reference it without a
    /// dependency on the Combat terrain types.
    /// </summary>
    public enum ArenaBlock
    {
        /// <summary>Checkerboard floor tile, light parity ((col+row) even).</summary>
        FloorA,
        /// <summary>Checkerboard floor tile, dark parity ((col+row) odd).</summary>
        FloorB,
        /// <summary>Raised, non-walkable block on an Impassable cell (rock/wall/water).</summary>
        Obstacle,
        /// <summary>Marker overlaid on a Hazard cell's floor tile (trap/lava-pool); the tile stays walkable.</summary>
        HazardMarker,
    }
}
