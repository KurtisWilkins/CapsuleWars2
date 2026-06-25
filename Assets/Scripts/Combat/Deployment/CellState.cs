namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// The placement state of a deployment-grid cell, used to drive UI feedback.
    /// </summary>
    public enum CellState
    {
        /// <summary>Outside the grid dimensions entirely.</summary>
        OutOfBounds,
        /// <summary>In bounds but Impassable terrain — blocks placement and pathing (rock/river/wall).</summary>
        Blocked,
        /// <summary>In bounds and unblocked, but not in the player's deploy zone.</summary>
        OutsideZone,
        /// <summary>A valid, empty cell the player can place a unit on.</summary>
        Empty,
        /// <summary>A valid cell already holding a unit.</summary>
        Occupied,
        /// <summary>Hazard terrain (lava/trap) — placeable per the grid's allowPlaceOnHazard rule, but harmful/avoid.</summary>
        Hazard
    }
}
