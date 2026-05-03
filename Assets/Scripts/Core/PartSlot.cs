namespace CapsuleWars.Core
{
    /// <summary>
    /// Mounting slots on a unit. Each slot can hold one BodyPart_SO at a time.
    /// Hands hold weapons, shields, or part meshes (claws, fists, hooks).
    /// Feet hold part meshes (boots, peg legs, hooves).
    /// HeadProp is optional (hat, horns, halo).
    /// </summary>
    public enum PartSlot
    {
        Body = 0,
        LeftHand = 1,
        RightHand = 2,
        LeftFoot = 3,
        RightFoot = 4,
        HeadProp = 5
    }
}
