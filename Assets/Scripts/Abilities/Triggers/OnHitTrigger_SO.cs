using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Fires when the unit lands a hit (deals damage) — Docs/05.</summary>
    [CreateAssetMenu(fileName = "OnHitTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Hit", order = 31)]
    public class OnHitTrigger_SO : EventTriggerBase_SO
    {
        protected override float EventTime(AbilityRuntime runtime) => runtime.LastHitDealtTime;
    }
}
