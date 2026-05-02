using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Body", menuName = "UnitBuilder/Body")]
public class UnitBody_SO : ScriptableObject
{
    public int BodyID;
    [SerializeField] public int raceID;
    [SerializeField] public string raceNameID;

    [SerializeField] public List<Sprite> arm_L; 
    [SerializeField] public List<Sprite> arm_R; 
    [SerializeField] public List<Sprite> foot_L; 
    [SerializeField] public List<Sprite> foot_R;
    [SerializeField] public List<Sprite> body;
    [SerializeField] public List<Sprite> head;
    [SerializeField] public List<Sprite> tail;

    [SerializeField] public Color color;
}
