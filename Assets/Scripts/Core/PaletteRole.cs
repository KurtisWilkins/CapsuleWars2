namespace CapsuleWars.Core
{
    /// <summary>
    /// Which palette color a slot mount should pick up. None means the mount
    /// keeps the BodyPart_SO's default materials and is not tinted.
    /// </summary>
    public enum PaletteRole
    {
        None = 0,
        Body = 1,
        Limbs = 2,
        Accent = 3
    }
}
