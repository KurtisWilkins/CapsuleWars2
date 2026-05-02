using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "New ClosestUnitFiltering", menuName = "Movement/Filtering/ClosestUnit", order = 0)]
public class MovementFilterClosest : MovementFilterStrategy
{
    [SerializeField] private bool includesSelf = false;
    [SerializeField] private bool getClosest = true;
    public override void DoMovementFilter(MovementData data)
    {
        if (getClosest)
        {
            UnitStatusController closestUnit = null;
            float distance = Mathf.Infinity;

            foreach (var unit in data.possibleTargets)
            {
                if (!includesSelf && unit == data.attacker) { return; }
                float x = Vector2.Distance(data.attacker.transform.position, unit.transform.position);
                //float i = Vector2.Distance(data.attacker.transform.position, unit.transform.position);
                if (distance > x)
                {
                    closestUnit = unit;
                    distance = x;
                }
            }

            data.target = closestUnit;

            if (data.target == null)
            {
                //Might need a check here
            }
        }
        else
        {
            UnitStatusController closestUnit = null;
            float distance = 0f;

            foreach (var unit in data.possibleTargets)
            {
                if (!includesSelf && unit == data.attacker) { return; }
                float x = Vector2.Distance(data.attacker.transform.position, unit.transform.position);
                //float i = Vector2.Distance(data.attacker.transform.position, unit.transform.position);
                if (distance < x)
                {
                    closestUnit = unit;
                    distance = x;
                }
            }

            data.target = closestUnit;

            if (data.target == null)
            {
                //Might need a check here
            }
        }
        
    }
}
