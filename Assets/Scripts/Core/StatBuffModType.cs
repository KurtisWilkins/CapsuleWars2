namespace CapsuleWars.Core
{
    /// <summary>How a stat modifier composes with the base stat.</summary>
    public enum StatBuffModType
    {
        /// <summary>Added to the base after percent mods.</summary>
        Flat = 0,
        /// <summary>Percent of base. Stacks additively (10% + 10% = 20%, not 21%).</summary>
        Percent = 1
    }
}
