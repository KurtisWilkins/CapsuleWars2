namespace CapsuleWars.Combat.Deployment
{
    /// <summary>
    /// The placement state of a deployment-grid cell, used to drive UI feedback.
    /// </summary>
    public enum CellState
    {
        /// <summary>Outside the grid dimensions entirely.</summary>
        OutOfBounds,
        /// <summary>In bounds but explicitly blocked (obstacle / enemy half).</summary>
        Blocked,
        /// <summary>In bounds and unblocked, but not in the player's deploy zone.</summary>
        OutsideZone,
        /// <summary>A valid, empty cell the player can place a unit on.</summary>
        Empty,
        /// <summary>A valid cell already holding a unit.</summary>
        Occupied
    }
}
