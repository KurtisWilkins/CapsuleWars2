using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementFilterStrategy : ScriptableObject
{
    public abstract void DoMovementFilter(MovementData data);
}

