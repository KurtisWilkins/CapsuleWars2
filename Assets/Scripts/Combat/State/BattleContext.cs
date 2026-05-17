using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Combat.State
{
    /// <summary>
    /// One per battle scene. Holds the unit registry and exposes it to
    /// the Units assembly via <see cref="CombatServices"/>.
    /// In M3 BattleStateManager owns the phase; BattleContext is just the
    /// registry. Post-M4 this collapses into a single BattleSpawner-managed
    /// object.
    /// </summary>
    [DisallowMultipleComponent]
    public class BattleContext : MonoBehaviour, ICombatRegistry
    {
        private readonly List<IUnitRef> units = new();
        public IReadOnlyList<IUnitRef> Units => units;

        public event Action<IUnitRef> OnUnitRegistered;
        public event Action<IUnitRef> OnUnitUnregistered;

        private void Awake()
        {
            CombatServices.Registry = this;
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(CombatServices.Registry, this))
                CombatServices.Registry = null;
        }

        public void Register(IUnitRef unit)
        {
            if (unit == null) return;
            if (units.Contains(unit)) return;
            units.Add(unit);
            OnUnitRegistered?.Invoke(unit);
        }

        public void Unregister(IUnitRef unit)
        {
            if (unit == null) return;
            if (units.Remove(unit))
                OnUnitUnregistered?.Invoke(unit);
        }
    }
}

