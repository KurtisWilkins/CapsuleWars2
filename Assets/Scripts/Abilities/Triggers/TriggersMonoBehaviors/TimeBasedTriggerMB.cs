using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class TimeBasedTriggerMB : MonoBehaviour
{
    [Tooltip("Delay in seconds for start of the ability.")]
    [SerializeField] private float timeCost = 1.5f;
    [SerializeField] private bool isBasic;
    [SerializeField] private UnitStatusController user;
    [SerializeField] private UnitAttackController unitAttack;
    public float attackLastTime;
    public bool isloaded = false;


    // Start is called before the first frame update
    void Start()
    {
        attackLastTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isloaded) { Trigger(); }
    }

    public void LoadData(float _timecost, bool _isBasic, UnitStatusController _user, UnitAttackController _unitAttackController)
    {
        timeCost = _timecost;
        isBasic = _isBasic;
        user = _user;
        unitAttack = _unitAttackController;
        isloaded = true;
    }

    private void Trigger()
    {
        if (!unitAttack.CanAttack())
        {
            if (attackLastTime < (Time.time - timeCost))
            {
                if (isBasic)
                {
                    if (unitAttack.basicAttackInRange)
                    {
                        unitAttack.BasicAttack();
                        attackLastTime = Time.time;
                    }
                }
                else
                {
                    unitAttack.PassiveAttack();
                    attackLastTime = Time.time;
                }
            }
        }
    }
    

}
