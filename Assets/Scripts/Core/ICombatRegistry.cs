using System.Collections.Generic;

namespace CapsuleWars.Core
{
    /// <summary>
    /// Read+write registry of units in an active battle. Implemented by
    /// Combat.State.BattleContext; consumed by Units.* via CombatServices.
    /// </summary>
    public interface ICombatRegistry
    {
        IReadOnlyList<IUnitRef> Units { get; }
        void Register(IUnitRef unit);
        void Unregister(IUnitRef unit);
    }
}
