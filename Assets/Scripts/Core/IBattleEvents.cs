using System;

namespace CapsuleWars.Core
{
    /// <summary>
    /// Core-facing view of the battle event surface (implemented by Combat's BattleEventBus). Lets lower layers
    /// (e.g. Abilities) subscribe to combat events via <see cref="CombatServices.Events"/> without referencing the
    /// Combat assembly. Only the events ability triggers need are exposed here (battle-end stays on the concrete bus).
    /// </summary>
    public interface IBattleEvents
    {
        /// <summary>A unit dealt damage (payload Source = attacker, Target = victim).</summary>
        event Action<DamageEvent> OnDamageDealt;
        /// <summary>A unit took damage (payload Source = attacker, Target = victim).</summary>
        event Action<DamageEvent> OnDamageTaken;
        /// <summary>A unit was downed (payload Target = the downed unit).</summary>
        event Action<DownedEvent> OnDowned;
        /// <summary>A kill was credited (payload Source = killer, Target = victim).</summary>
        event Action<KillEvent> OnKill;
        /// <summary>Combat became active.</summary>
        event Action OnBattleStart;
    }
}
