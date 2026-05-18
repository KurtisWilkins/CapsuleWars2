using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Keeps the N targets closest to the source unit.</summary>
    [CreateAssetMenu(fileName = "ClosestNFilter", menuName = "CapsuleWars/Abilities/Filters/Closest N", order = 50)]
    public class ClosestNFilter_SO : AbilityFilterStrategy
    {
        [SerializeField, Min(1)] private int n = 1;

        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            if (candidates.Count <= n) return;
            if (ctx.Source == null) return;

            Vector3 src = ctx.Source.Transform.position;
            candidates.Sort((a, b) =>
            {
                float da = (a.Transform.position - src).sqrMagnitude;
                float db = (b.Transform.position - src).sqrMagnitude;
                return da.CompareTo(db);
            });
            candidates.RemoveRange(n, candidates.Count - n);
        }
    }
}
