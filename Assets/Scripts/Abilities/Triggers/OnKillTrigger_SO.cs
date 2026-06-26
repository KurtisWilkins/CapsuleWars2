using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Fires when the unit kills an enemy — Docs/05.</summary>
    [CreateAssetMenu(fileName = "OnKillTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Kill", order = 33)]
    public class OnKillTrigger_SO : EventTriggerBase_SO
    {
        protected override float EventTime(AbilityRuntime runtime) => runtime.LastKillTime;
    }
}
