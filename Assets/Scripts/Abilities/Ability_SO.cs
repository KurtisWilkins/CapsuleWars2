using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability", order = 0)]
public class Ability_SO : ScriptableObject
{
    [SerializeField] public string abilityNameID;
    [SerializeField] public int abilityID;
    [SerializeField] public Sprite icon;
    [SerializeField] public string descriptionID;
    [SerializeField] public float timeCost = 1.5f;
    [SerializeField] public float range = 4f;
    [SerializeField] public bool isElement = false;
    [SerializeField] public ElementType_SO moveType1;

    [SerializeField] AnimationData animation;

    [SerializeField] public AbilityTriggerStrategy triggerStrategy;
    [SerializeField] AbilityTargetingStrategy[] targetingStrategy;
    [SerializeField] AbilityFilterStrategies[] filterStrategies;
    [SerializeField] AbilityEffectsStrategies[] effectStrategies;

    public void DoAbility(UnitStatusController user, UnitStatusController primaryTarget)
    {
        AbilityData data = new AbilityData(user, primaryTarget,isElement,moveType1);

        //Starts Targeting process
        targetingStrategy[user.dTO.evolution].StartTargeting(data);

        //Filters for correct units
        foreach (var filterStrategy in filterStrategies[user.dTO.evolution].abilityFilterStrategies)
        {
            filterStrategy.StartFilter(data);
        }

        //Applies Desired Effects
        foreach (var effectStrategy in effectStrategies[user.dTO.evolution].abilityEffectsStrategies)
        {
            effectStrategy.StartEffect(data);
        }
    }

    /// <summary>
    /// Returns the animation for ability
    /// </summary>
    /// <returns></returns>
    public AnimationData GetanimationData()
    {
        return animation;
    }
}
