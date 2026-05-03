namespace CapsuleWars.Core
{
    /// <summary>
    /// Which side of a battle a unit fights on. Targets are filtered by team.
    /// Friendly-fire is opt-in per ability (default off).
    /// </summary>
    public enum Team
    {
        Player = 0,
        Enemy = 1
    }
}
