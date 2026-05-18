namespace CapsuleWars.Core
{
    /// <summary>
    /// When the target's Resistance stat is rolled against the status.
    /// </summary>
    public enum ResistanceCheck
    {
        /// <summary>No roll; always applies.</summary>
        None = 0,
        /// <summary>Single roll at apply time; pass → no effect.</summary>
        RollOnApply = 1,
        /// <summary>Roll each tick; pass → skip that tick.</summary>
        RollPerTick = 2
    }
}
