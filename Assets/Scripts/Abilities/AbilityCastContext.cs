using CapsuleWars.Core;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Runtime payload passed through the strategy chain during one ability
    /// cast. Contains the source unit and the ability being cast; the
    /// candidate-list grows and shrinks through Targeting → Filter → Effect.
    /// </summary>
    public readonly struct AbilityCastContext
    {
        public readonly IUnitRef Source;
        public readonly Ability_SO Ability;

        public AbilityCastContext(IUnitRef source, Ability_SO ability)
        {
            Source = source;
            Ability = ability;
        }
    }
}
