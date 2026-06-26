using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>Fires when an allied unit is downed — Docs/05.</summary>
    [CreateAssetMenu(fileName = "OnAllyDeathTrigger", menuName = "CapsuleWars/Abilities/Triggers/On Ally Death", order = 34)]
    public class OnAllyDeathTrigger_SO : EventTriggerBase_SO
    {
        protected override float EventTime(AbilityRuntime runtime) => runtime.LastAllyDeathTime;
    }
}
