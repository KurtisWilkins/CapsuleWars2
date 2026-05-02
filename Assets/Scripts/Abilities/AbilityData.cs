using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityData
{
    public UnitStatusController user;
    public UnitStatusController primaryTarget;
    public List<UnitStatusController> targets = new List<UnitStatusController>();
    public bool isElement = false;
    public ElementType_SO attackType1;
    public BattleController battleController;

    public AbilityData(UnitStatusController _user, UnitStatusController _primaryTarget, bool isElement, ElementType_SO attackType1)
    {
        user = _user;
        primaryTarget = _primaryTarget;
        this.isElement = isElement;
        this.attackType1 = attackType1;
    }
}
