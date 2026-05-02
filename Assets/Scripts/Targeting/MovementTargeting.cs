using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMovementTargeting", menuName = "Movement/Movement", order = 0)]
public class MovementTargeting : ScriptableObject
{
    [SerializeField] public string movementNameID;
    [SerializeField] public int targetingID;
    [SerializeField] public Sprite icon;
    [SerializeField] public string descriptionID;
    [SerializeField] public bool isHealerTargeting = false;

    [SerializeField] public MovementTargetingStrategy movementTargeting;
    [SerializeField] public MovementFilterStrategy[] movementFilters;

    public UnitStatusController GetTarget(UnitStatusController user)
    {
        MovementData data = new MovementData { attacker = user, possibleTargets = new List<UnitStatusController>(), target = null };

        movementTargeting.GetMovementTarget(data);

        foreach (MovementFilterStrategy filter in movementFilters)
        {
            filter.DoMovementFilter(data);
        }

        if (data.target != null)
        {
            return data.target;
        }

        return null;
    }
}
