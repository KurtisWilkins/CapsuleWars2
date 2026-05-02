using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ClassRaceElementFilter", menuName = "Movement/Filtering/ClassRaceElement", order = 0)]
public class MovementFilterRaceClassElement : MovementFilterStrategy
{
    [SerializeField] public List<UnitClass_SO> FilterClasses;
    [SerializeField] public List<UnitBody_SO> FilterRaces;
    [SerializeField] public List<ElementType_SO> FilterElements;
    
    public override void DoMovementFilter(MovementData data)
    {
        List<UnitStatusController> newTargets = new List<UnitStatusController>();
        foreach (var unit in data.possibleTargets)
        {
            if(FilterClasses.Count > 0)
            {
                foreach(var c in FilterClasses)
                {
                    if(unit.unitClass.classTypeID == c.classTypeID)
                    {
                        newTargets.Add(unit);
                    }
                }
            }
            if(FilterRaces.Count > 0)
            {
                foreach( var race in FilterRaces)
                {
                    if(unit.race == race.BodyID)
                    {
                        newTargets.Add(unit);
                    }
                }
            }
            if(FilterElements.Count > 0)
            {
                foreach(var element in FilterElements)
                {
                    if(unit.unitType1.elementTypeID == element.elementTypeID || unit.unitType2.elementTypeID == element.elementTypeID)
                    {
                        newTargets.Add(unit);
                    }
                }
            }
        }

        if(newTargets.Count > 0)
        {
            data.possibleTargets = newTargets;
        }
    }
}
