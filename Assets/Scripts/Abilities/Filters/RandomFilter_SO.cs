using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Keep N random candidates (Docs/05).</summary>
    [CreateAssetMenu(fileName = "RandomFilter", menuName = "CapsuleWars/Abilities/Filters/Random", order = 52)]
    public class RandomFilter_SO : AbilityFilterStrategy
    {
        [SerializeField, Min(1)] private int n = 1;

        private System.Random rng;

        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            rng ??= new System.Random();
            AbilitySelect.KeepRandomN(candidates, n, rng);
        }
    }
}
