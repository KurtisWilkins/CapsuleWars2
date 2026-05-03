namespace CapsuleWars.Core
{
    /// <summary>
    /// Fired by UnitHealthController.OnDamageTaken when a unit takes any
    /// non-zero damage. Consumed by stats aggregators, UI popups, audio.
    /// </summary>
    public readonly struct DamageEvent
    {
        public readonly IUnitRef Source;
        public readonly IUnitRef Target;
        public readonly int Amount;

        public DamageEvent(IUnitRef source, IUnitRef target, int amount)
        {
            Source = source;
            Target = target;
            Amount = amount;
        }
    }

    /// <summary>
    /// Fired when a unit's HP reaches zero and it enters the downed state
    /// for the rest of the battle.
    /// </summary>
    public readonly struct DownedEvent
    {
        public readonly IUnitRef Source;
        public readonly IUnitRef Target;

        public DownedEvent(IUnitRef source, IUnitRef target)
        {
            Source = source;
            Target = target;
        }
    }

    /// <summary>
    /// Fired when a unit's downed event credits a kill to a source.
    /// In M2, equivalent to DownedEvent — they may diverge later if we
    /// distinguish between assists / killing blows.
    /// </summary>
    public readonly struct KillEvent
    {
        public readonly IUnitRef Source;
        public readonly IUnitRef Target;

        public KillEvent(IUnitRef source, IUnitRef target)
        {
            Source = source;
            Target = target;
        }
    }
}
