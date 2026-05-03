using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Combat.State
{
    /// <summary>
    /// One per battle scene. Holds the unit registry and exposes it to
    /// the Units assembly via <see cref="CombatServices"/>.
    /// In M3 this will be created and torn down by BattleStateManager;
    /// in M2 you place one in the scene manually.
    /// </summary>
    [DisallowMultipleComponent]
    public class BattleContext : MonoBehaviour, ICombatRegistry
    {
        private readonly List<IUnitRef> units = new();
        public IReadOnlyList<IUnitRef> Units => units;

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
        }

        public void Unregister(IUnitRef unit)
        {
            if (unit == null) return;
            units.Remove(unit);
        }
    }
}
