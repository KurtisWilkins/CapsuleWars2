using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementData
{
    public UnitStatusController attacker;
    public List<UnitStatusController> possibleTargets = new List<UnitStatusController>();
    public UnitStatusController target;
}
