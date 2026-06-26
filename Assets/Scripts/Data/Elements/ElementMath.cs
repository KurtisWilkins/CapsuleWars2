using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Elements
{
    /// <summary>
    /// Central element-damage multiplier math (Docs/08). Family-level matchup via the chart, plus the
    /// dual-element rule: a defender with primary + secondary elements gives the attacker the LEAST
    /// favorable (lowest) of the two multipliers — i.e. best defense. Pure + null-safe → EditMode-testable.
    /// Both damage paths (UnitAttackController basic hits, DamageEffect_SO ability hits) route through here.
    /// </summary>
    public static class ElementMath
    {
        /// <summary>
        /// Multiplier an <paramref name="attacker"/> element deals into a defender's primary (+ optional
        /// secondary) element. Returns 1.0 when the chart/attacker is null or the defender has no element.
        /// </summary>
        public static float Multiplier(IElementChart chart, ElementType_SO attacker,
                                       ElementType_SO defenderPrimary, ElementType_SO defenderSecondary)
        {
            if (chart == null || attacker == null) return 1f;

            bool any = false;
            float mult = 1f;

            if (defenderPrimary != null)
            {
                mult = chart.GetMultiplier(attacker.Family, defenderPrimary.Family);
                any = true;
            }
            if (defenderSecondary != null)
            {
                float second = chart.GetMultiplier(attacker.Family, defenderSecondary.Family);
                mult = any ? Mathf.Min(mult, second) : second;   // least favorable for attacker = best defense
                any = true;
            }

            return any ? mult : 1f;
        }
    }
}
