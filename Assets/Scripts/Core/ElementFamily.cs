namespace CapsuleWars.Core
{
    /// <summary>
    /// The 5 element families that form CapsuleWars' rock-paper-scissors
    /// wheel. Each family has 3 sub-type ElementTypes; matchups are
    /// resolved at the family level (see Docs/08_ElementSystem.md).
    /// </summary>
    public enum ElementFamily
    {
        Fire = 0,
        Water = 1,
        Earth = 2,
        Spirit = 3,
        Air = 4
    }
}
