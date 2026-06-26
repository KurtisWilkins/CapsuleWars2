using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// Keep only candidates matching the source's class and/or element (Docs/05 calls this
    /// "race/class/element"; CW2 has no Race concept, so class + element are the available axes).
    /// Toggle which axes must match.
    /// </summary>
    [CreateAssetMenu(fileName = "RaceClassElementFilter", menuName = "CapsuleWars/Abilities/Filters/Class+Element Match", order = 53)]
    public class RaceClassElementFilter_SO : AbilityFilterStrategy
    {
        [SerializeField] private bool matchClass = true;
        [SerializeField] private bool matchElement = false;

        public override void Filter(AbilityCastContext ctx, List<IUnitRef> candidates)
        {
            var srcRoot = ctx.Source != null && ctx.Source.GameObject != null
                ? ctx.Source.GameObject.GetComponentInParent<UnitRoot>() : null;
            if (srcRoot == null || srcRoot.Status == null) return;

            var srcClass = srcRoot.Status.UnitClass;
            var srcElement = srcRoot.Status.PrimaryElement;

            AbilitySelect.KeepWhere(candidates, c =>
            {
                var r = c != null && c.GameObject != null ? c.GameObject.GetComponentInParent<UnitRoot>() : null;
                if (r == null || r.Status == null) return false;
                if (matchClass && r.Status.UnitClass != srcClass) return false;
                if (matchElement && r.Status.PrimaryElement != srcElement) return false;
                return true;
            });
        }
    }
}
