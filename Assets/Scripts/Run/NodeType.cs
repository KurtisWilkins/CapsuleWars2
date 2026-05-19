namespace CapsuleWars.Run
{
    /// <summary>
    /// What kind of encounter sits at a map node. All seven types from
    /// Docs/12_RoguelikeRun.md are represented; M7 reuses panels for
    /// Event/Rest/Treasure and the battle scene for Combat/Elite/Boss.
    /// </summary>
    public enum NodeType
    {
        Combat = 0,
        Elite = 1,
        Shop = 2,
        Event = 3,
        Rest = 4,
        Treasure = 5,
        Boss = 6
    }
}
