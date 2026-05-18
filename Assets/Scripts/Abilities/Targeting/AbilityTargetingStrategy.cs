using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Produces the initial candidate set for a cast. Filters then narrow
    /// this list; Effects apply to whatever survives.
    /// </summary>
    public abstract class AbilityTargetingStrategy : ScriptableObject
    {
        public abstract void Collect(AbilityCastContext ctx, List<IUnitRef> output);
    }
}
