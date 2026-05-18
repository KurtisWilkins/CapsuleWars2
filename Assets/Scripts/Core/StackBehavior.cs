namespace CapsuleWars.Core
{
    /// <summary>
    /// What happens when a status effect is re-applied to a unit that
    /// already has it active.
    /// </summary>
    public enum StackBehavior
    {
        /// <summary>Reset duration to max; ignore the new application's source. (Default.)</summary>
        Refresh = 0,
        /// <summary>Add the new duration to remaining, capped at maxStacks × defaultDuration.</summary>
        Add = 1,
        /// <summary>Allow multiple independent instances, each ticking separately.</summary>
        Independent = 2
    }
}
