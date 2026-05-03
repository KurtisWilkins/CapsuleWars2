namespace CapsuleWars.Core
{
    /// <summary>
    /// How a weapon occupies hand slots. Drives equipment validation:
    /// a TwoHanded weapon locks both hands; a Shield must pair with
    /// OneHanded (or empty) in the other hand; Dual requires both.
    /// </summary>
    public enum WeaponHandedness
    {
        OneHanded = 0,
        TwoHanded = 1,
        Dual = 2,
        Shield = 3,
        OffHand = 4
    }
}
