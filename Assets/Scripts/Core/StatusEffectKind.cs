namespace CapsuleWars.Core
{
    /// <summary>
    /// Category of a status effect. Drives UI presentation and helps
    /// systems route specific kinds (e.g. cleanse abilities removing
    /// only Debuff/Control).
    /// </summary>
    public enum StatusEffectKind
    {
        Buff = 0,
        Debuff = 1,
        Control = 2,
        DoT = 3,
        HoT = 4
    }
}
