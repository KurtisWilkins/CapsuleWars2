namespace CapsuleWars.Core
{
    /// <summary>
    /// Mounting slots on a unit. Each slot can hold one BodyPart_SO at a time.
    /// Hands hold weapons, shields, or part meshes (claws, fists, hooks).
    /// Feet hold part meshes (boots, peg legs, hooves).
    /// Head is the unit's head mesh — the default floating sphere (Rayman/Rabbids
    /// style), a swappable part distinct from the capsule Body.
    /// HeadProp is optional (hat, horns, halo) and seats on top of the Head.
    /// </summary>
    public enum PartSlot
    {
        Body = 0,
        LeftHand = 1,
        RightHand = 2,
        LeftFoot = 3,
        RightFoot = 4,
        HeadProp = 5,
        // APPEND-ONLY: PartSlot is serialized by integer in saves (UnitPartDTO.slot) and
        // .asset files (BodyPart_SO.slot via enumValueIndex). Never insert/reorder — only append.
        Head = 6
    }
}
