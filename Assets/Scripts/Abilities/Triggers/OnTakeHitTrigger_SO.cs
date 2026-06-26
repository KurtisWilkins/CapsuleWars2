using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Fires when the unit is hit (takes damage) — Docs/05.</summary>
    [CreateAssetMenu(fileName = "OnTakeHitTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Take Hit", order = 32)]
    public class OnTakeHitTrigger_SO : EventTriggerBase_SO
    {
        protected override float EventTime(AbilityRuntime runtime) => runtime.LastHitTakenTime;
    }
}
