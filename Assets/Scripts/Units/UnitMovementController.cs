using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitMovementController : MonoBehaviour
{
    public Rigidbody2D theRB;
    public float moveSpeed = 1f;
    [SerializeField] private UnitStatusController target;

    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private UnitAnimationController unitAnimation;
    [SerializeField] private UnitAttackController unitAttack;
    [SerializeField] private bool battlestarted = false;
    [SerializeField] private bool unitDead = false;
    [SerializeField] public bool knockBack = false;

    [SerializeField] public MovementTargeting movementTargeting;
    [SerializeField] public GameObject highLightSelected;
    [SerializeField] public GameObject highLightTargeted;
    [SerializeField] public GameObject lightRef;

    private UnitStatusController unitStatusController;

    public event Action<Transform> onTargetChange;

    // Start is called before the first frame update
    private void Start()
    {
        BattleController.onBattleStarted += StartBattle;
        BattleController.onBattleEnded += EndBattle;
        unitStatusController = GetComponent<UnitStatusController>();
        unitAttack = GetComponent<UnitAttackController>();
        unitAnimation = GetComponent<UnitAnimationController>();
        GetComponent<UnitHealthController>().onDeath += UnitDied;
        stoppingDistance = unitAttack.AttackStoppingDistance();
        lightRef.GetComponent<Light>().color = unitStatusController.unitType1.elementTextColor;
        if(transform.rotation.y != 0f)
        {
            lightRef.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            lightRef.transform.localPosition = new Vector3(0f, .2f, 8.5f);
        }
        else
        {
            lightRef.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            lightRef.transform.localPosition = new Vector3(0f, .2f, -8.5f);
        }
    }

    private void OnDestroy()
    {
        BattleController.onBattleStarted -= StartBattle;
        BattleController.onBattleEnded -= EndBattle;
        GetComponent<UnitHealthController>().onDeath -= UnitDied;
    }

    public void LoadUnitMovementController(UnitDTO unitDTO, Database database)
    {
        movementTargeting = database.movementTargetings[unitDTO.targetingID];
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(knockBack)
        {
            return;
        }
        if (CanMove())
        {
            theRB.linearVelocity = Vector3.zero;
            unitAnimation.SetWalking(0f);
            return;
        }

        if(target == null || target.IsUnitDead()) 
        { 
            GetTarget();
            if(target == null)
            {
                return;
            }
        }
        else
        {
            FaceEnemy();

            theRB.linearVelocity = GetVelocity();
        }
    }

    private void FaceEnemy()
    {
        if (target.transform.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            lightRef.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            lightRef.transform.localPosition = new Vector3(0f, .2f, 8.5f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            lightRef.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            lightRef.transform.localPosition = new Vector3(0f, .2f, -8.5f);
        }
    }

    private bool CanMove()
    {
        return !battlestarted || unitDead;
    }

    /// <summary>
    /// Gets the velocity of the unit if they shoudl be walking of idle.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetVelocity()
    {
        Vector3 velocity = Vector3.zero;

        if (Vector3.Distance(target.transform.position, transform.position) > stoppingDistance)
        {
            //Debug.Log("Character Distance is " + Vector3.Distance(target.position, transform.position) + " Stopping Range is " + stoppingDistance);
            velocity = (target.transform.position - transform.position).normalized * moveSpeed;
            unitAnimation.SetWalking(.5f);
            unitAttack.CanBasicAttack(false);
        }
        else
        {
            //Debug.Log("Character Distance is " + Vector3.Distance(target.position, transform.position) + " Stopping Range is " + stoppingDistance + " Fired Attack");
            unitAnimation.SetWalking(0f);
            unitAttack.CanBasicAttack(true);
        }

        return velocity;
    }

    /// <summary>
    /// this starts the movement of the unit.
    /// </summary>
    /// <param name="x"></param>
    public void StartBattle(bool x)
    {
        battlestarted = x;
        theRB.mass = unitStatusController.GetMass();
        moveSpeed = ((1000f + unitStatusController.BuildSpeedMod()) / 1000f);
        if (x) { GetTarget(); }
    }

    public void EndBattle(bool x)
    {
        battlestarted = x;
        if (!unitStatusController.IsUnitDead()) { unitAnimation.SetWalking(0f); unitAttack.CanBasicAttack(false); }
    }

    public void UnitDied(bool x,UnitStatusController attacker, UnitStatusController defender)
    {
        unitDead = x;
    }

    public void VailidateTarget(bool x, UnitStatusController attacker, UnitStatusController defender)
    {
        if (x)
        {
            if(target != null)
            {
                target.GetComponent<UnitHealthController>().onDeath -= VailidateTarget;
            }
            GetTarget();
        }
    }

    public void GetTarget()
    {
        //GetTargeting pattern Here
        target = movementTargeting.GetTarget(unitStatusController); //GetTargeting pattern Here
        if (target == null) { theRB.linearVelocity = Vector3.zero; return; }
        onTargetChange?.Invoke(target.transform);
        target.GetComponent<UnitHealthController>().onDeath += VailidateTarget;
        unitAttack.CanBasicAttack(false);
    }

    public void AttackedByUnit(UnitStatusController unitStatusController)
    {
        if(target == null)
        {
            target = unitStatusController;
        }
        else
        {
            if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, unitStatusController.transform.position))
            {
                target = unitStatusController;
            }
        }
    }

    public UnitMovementController GetTargetForHighlight()
    {
        UnitStatusController uSC = movementTargeting.GetTarget(unitStatusController);
        if(uSC == null)
        {
            return null;
        }
        else
        {
            return uSC.gameObject.GetComponent<UnitMovementController>();
        }
    }

    public void UnitWasAttackedCheck(UnitStatusController attacker)
    {
        if(target != null)
        {
            float nT = Vector2.Distance(gameObject.transform.position, attacker.transform.position);
            float cT = Vector2.Distance(gameObject.transform.position, target.transform.position);
            if (nT < cT && !movementTargeting.isHealerTargeting)
            {
                target = attacker;
            }
        }
        else
        {
            if (!movementTargeting.isHealerTargeting)
            {
                target = attacker;
            }
        }
        
    } 

    public void ToggleOnHighlightTargeted()
    {
        highLightTargeted.SetActive(true);
    }
    public void ToggleOffHighlightTargeted()
    {
        highLightTargeted.SetActive(false);
    }

    public void ToggleOnHighlightSelected()
    {
        highLightSelected.SetActive(true);
    }

    public void ToggleOffHighlightSelected()
    {
        highLightSelected.SetActive(false);
    }
}
