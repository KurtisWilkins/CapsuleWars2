using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GetUnitsTargeting", menuName = "Movement/Targeting/GetUnitsTargeting", order = 0)]
public class MovementGetUnits : MovementTargetingStrategy
{
    [SerializeField] public bool getAllUnits = false;
    [SerializeField] public bool getAllyUnits = false;
    
    
    public override void GetMovementTarget(MovementData data)
    {   
        if (getAllUnits)
        {
            foreach (UnitStatusController unit in BattleController.deployedUnits)
            {
                data.possibleTargets.Add(unit);
            }
        }
        else
        {
            if (getAllyUnits)
            {
                foreach(UnitStatusController unit in BattleController.deployedUnits)
                {
                    if(unit.GetTeamID() == data.attacker.GetTeamID() && !unit.IsUnitDead() && unit.dTO.iD != data.attacker.dTO.iD)
                    {
                        data.possibleTargets.Add(unit);
                    }
                }

                if (data.possibleTargets.Count < 1)
                {
                    foreach (UnitStatusController u in BattleController.deployedUnits)
                    {
                        if (u.dTO.iD == data.attacker.dTO.iD)
                        {
                            data.possibleTargets.Add(u);
                        }
                    }
                }
            }
            else
            {
                foreach (UnitStatusController unit in BattleController.deployedUnits)
                {
                    if (unit.GetTeamID() != data.attacker.GetTeamID() && !unit.IsUnitDead())
                    {
                        data.possibleTargets.Add(unit);
                    }
                }
            }
        }
    }
}
