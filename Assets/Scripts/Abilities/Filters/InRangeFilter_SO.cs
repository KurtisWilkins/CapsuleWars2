using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Keeps only targets within the ability's range. If the ability has
    /// range 0, this filter is a no-op (assumes unlimited range).
    /// </summary>
    [CreateAssetMenu(fileName = "InRangeFilter", menuName = "CapsuleWars/Abilities/Filters/In Range", order = 51)]
    public class InRangeFilter_SO : AbilityFilterStrategy
    {
        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            float range = ctx.Ability != null ? ctx.Ability.Range : 0f;
            if (range <= 0f || ctx.Source == null) return;

            float rangeSqr = range * range;
            Vector3 src = ctx.Source.Transform.position;
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if ((candidates[i].Transform.position - src).sqrMagnitude > rangeSqr)
                    candidates.RemoveAt(i);
            }
        }
    }
}
