using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AbilityTargetingStrategy : ScriptableObject
{
    public abstract void StartTargeting(AbilityData data);
}