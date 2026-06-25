namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// Per-cell terrain on the deployment/battle board — generalizes the old binary
    /// "blocked" flag (blocked == <see cref="Impassable"/>). Pure data, used by the
    /// deployment grid for placement validation, by the renderer/theming (Slice B) to
    /// skin props, and by the encounter builder (Slice C) for obstacle-aware enemy
    /// placement. The NavMesh carver reads the Impassable cells to cut the baked mesh.
    /// </summary>
    public enum TerrainType
    {
        /// <summary>Normal ground: placeable and walkable.</summary>
        Passable = 0,
        /// <summary>Blocks placement AND pathing (rock, river, wall) — carved out of the NavMesh.</summary>
        Impassable = 1,
        /// <summary>Walkable, but harmful / to be avoided (lava, trap). Placeable per the grid's allowPlaceOnHazard rule;
        /// the actual harm is applied later (Slice C / combat), not by this data layer.</summary>
        Hazard = 2,
    }
}
