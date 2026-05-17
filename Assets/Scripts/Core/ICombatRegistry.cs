using System;
using System.Collections.Generic;

namespace CapsuleWars.Core
{
    /// <summary>
    /// Read+write registry of units in an active battle. Implemented by
    /// Combat.State.BattleContext; consumed by Units.* via CombatServices.
    /// Fires <see cref="OnUnitRegistered"/> and <see cref="OnUnitUnregistered"/>
    /// so aggregators can hook per-unit events without polling.
    /// </summary>
    public interface ICombatRegistry
    {
        IReadOnlyList<IUnitRef> Units { get; }
        void Register(IUnitRef unit);
        void Unregister(IUnitRef unit);

        event Action<IUnitRef> OnUnitRegistered;
        event Action<IUnitRef> OnUnitUnregistered;
    }
}

