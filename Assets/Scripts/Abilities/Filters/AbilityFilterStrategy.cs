using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Narrows the candidate list in place. Applied in chain — each filter
    /// shrinks (or rearranges) the list, the next sees the result.
    /// </summary>
    public abstract class AbilityFilterStrategy : ScriptableObject
    {
        public abstract void Filter(AbilityCastContext ctx, List<IUnitRef> candidates);
    }
}
