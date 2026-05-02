using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeBasedTrigger", menuName = "Abilities/Triggers/TimeBasedTrigger", order = 0)]
public class TimeBasedTrigger : AbilityTriggerStrategy
{
    [Tooltip("Delay in seconds for start of the ability.")]
    [SerializeField] private float timeCost = 1.5f;

    public override void StartTrigger(UnitStatusController _user, bool isBasic)
    {
        _user.gameObject.AddComponent<TimeBasedTriggerMB>().LoadData(timeCost, isBasic, _user, _user.GetComponent<UnitAttackController>());
        //_user.GetComponent<TimeBasedTriggerMB>().LoadData(timeCost, isBasic, _user, _user.GetComponent<UnitAttackController>());
    }
}
