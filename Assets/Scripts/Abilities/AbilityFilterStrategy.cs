using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AbilityFilterStrategy : ScriptableObject
{
    public abstract void StartFilter(AbilityData data);
}

[System.Serializable]
public class AbilityFilterStrategies
{
    [SerializeField] public AbilityFilterStrategy[] abilityFilterStrategies;
}