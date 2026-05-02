using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementTargetingStrategy :ScriptableObject
{
    public abstract void GetMovementTarget(MovementData data);
}
