using UnityEngine;

namespace CapsuleWars.Core
{
    /// <summary>
    /// Minimal contract every battle participant exposes. Implemented by
    /// UnitRoot in the Units assembly; defined here so payload structs in
    /// CombatEvents and the ICombatRegistry interface can refer to units
    /// without Core taking a dependency on Units.
    /// </summary>
    public interface IUnitRef
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
        Team Team { get; }
        bool IsDowned { get; }
    }
}
