using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityTriggerStrategy : ScriptableObject
{
    public abstract void StartTrigger(UnitStatusController _user, bool isBasic );
}
