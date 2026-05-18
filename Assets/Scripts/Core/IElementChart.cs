namespace CapsuleWars.Core
{
    /// <summary>
    /// Element matchup lookup, implemented by ElementChart_SO in Data.
    /// Defined in Core so Units / Abilities can consult the chart through
    /// CombatServices without taking a direct dependency on Data.
    /// </summary>
    public interface IElementChart
    {
        /// <summary>
        /// Damage multiplier when an attacker of <paramref name="attacker"/>
        /// hits a defender of <paramref name="defender"/>. Default 1.0
        /// (neutral); 1.5 for strong, 0.67 for weak per Docs/08.
        /// </summary>
        float GetMultiplier(ElementFamily attacker, ElementFamily defender);
    }
}
