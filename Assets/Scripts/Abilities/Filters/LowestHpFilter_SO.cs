using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Keep the N candidates with the lowest current HP% (Docs/05).</summary>
    [CreateAssetMenu(fileName = "LowestHpFilter", menuName = "CapsuleWars/Abilities/Filters/Lowest HP", order = 51)]
    public class LowestHpFilter_SO : AbilityFilterStrategy
    {
        [SerializeField, Min(1)] private int n = 1;

        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            AbilitySelect.KeepLowestN(candidates, HpPercent, n);
        }

        private static float HpPercent(IUnitRef u)
        {
            var root = u != null && u.GameObject != null ? u.GameObject.GetComponentInParent<UnitRoot>() : null;
            if (root == null || root.Health == null || root.Status == null) return float.MaxValue;
            return (float)root.Health.CurrentHp / Mathf.Max(1, root.Status.MaxHp);
        }
    }
}
