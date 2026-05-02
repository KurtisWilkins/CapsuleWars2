using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityEffectsStrategy : ScriptableObject
{
    public abstract void StartEffect(AbilityData data);
}

[System.Serializable]
public class AbilityEffectsStrategies
{
    [SerializeField] public AbilityEffectsStrategy[] abilityEffectsStrategies;
}