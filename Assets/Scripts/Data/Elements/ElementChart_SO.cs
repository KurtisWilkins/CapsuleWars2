using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.Elements
{
    /// <summary>
    /// 5×5 element family matchup table. Single asset shared by the whole
    /// project; assign to BattleStateManager.elementChart.
    /// Default values follow Docs/08_ElementSystem.md: strong ×1.5,
    /// neutral ×1.0, weak ×0.67. Tunable per design pass.
    /// </summary>
    [CreateAssetMenu(fileName = "ElementChart", menuName = "CapsuleWars/Elements/Element Chart", order = 71)]
    public class ElementChart_SO : ScriptableObject, IElementChart
    {
        [Tooltip("Multiplier when an attacker beats a defender on the family wheel.")]
        [SerializeField, Min(1f)] private float strongMultiplier = 1.5f;

        [Tooltip("Multiplier when neither side has the family advantage.")]
        [SerializeField, Min(0.01f)] private float neutralMultiplier = 1.0f;

        [Tooltip("Multiplier when an attacker is on the losing side of the wheel.")]
        [SerializeField, Min(0.01f), Range(0.01f, 1f)] private float weakMultiplier = 0.67f;

        // 5×5 matrix, row = attacker family, column = defender family.
        // 1 = strong, 0 = neutral, -1 = weak.
        // Per Docs/08: Fire→Air,Spirit | Water→Fire,Air | Earth→Water,Fire |
        //              Spirit→Earth,Water | Air→Spirit,Earth
        private static readonly int[,] Matchup =
        {
            //         Fire Water Earth Spirit Air
            /*Fire  */ {  0,  -1,  -1,    1,    1 },
            /*Water */ {  1,   0,  -1,   -1,    1 },
            /*Earth */ {  1,   1,   0,   -1,   -1 },
            /*Spirit*/ { -1,   1,   1,    0,   -1 },
            /*Air   */ { -1,  -1,   1,    1,    0 },
        };

        public float GetMultiplier(ElementFamily attacker, ElementFamily defender)
        {
            int a = (int)attacker;
            int d = (int)defender;
            if (a < 0 || a > 4 || d < 0 || d > 4) return neutralMultiplier;
            int code = Matchup[a, d];
            return code switch
            {
                1 => strongMultiplier,
                -1 => weakMultiplier,
                _ => neutralMultiplier
            };
        }
    }
}
