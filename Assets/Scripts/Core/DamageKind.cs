namespace CapsuleWars.Core
{
    /// <summary>
    /// How a chunk of damage is categorized. Drives status-effect behaviors in the damage pipeline
    /// (e.g. Frozen amplifies only Physical hits) — see Docs/10 + StatusEffectBehavior.
    /// </summary>
    public enum DamageKind
    {
        /// <summary>Basic weapon attacks.</summary>
        Physical = 0,
        /// <summary>Ability / spell damage (element-typed).</summary>
        Elemental = 1,
        /// <summary>DoT ticks and other untyped/unblockable sources.</summary>
        True = 2
    }
}
