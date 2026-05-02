using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Timeline;

public class UnitAttackController : MonoBehaviour
{
    [SerializeField] private UnitAnimationController unitAnimation;
    [SerializeField] private UnitStatusController unitStatusController;
    [SerializeField] private bool battlestarted = false;
    [SerializeField] public bool basicAttackInRange = false;
    [SerializeField] private bool unitDead = false;
    [SerializeField] public GameObject projectileSpawn;
    [SerializeField] public GameObject targetSpawn;

    [SerializeField] public Ability_SO baseAbility;
    [SerializeField] public Ability_SO passiveAbility;

    public float basicAttackLastTime;
    public float passiveAttackLastTime;
    private Transform target;

    private void Start()
    {
        BattleController.onBattleStarted += StartBattle;
        unitAnimation = GetComponent<UnitAnimationController>();
        unitStatusController = GetComponent<UnitStatusController>();
        GetComponent<UnitHealthController>().onDeath += UnitDied;
        GetComponent<UnitMovementController>().onTargetChange += SetCurrentTarget;
    }

    private void OnDestroy()
    {
        BattleController.onBattleStarted -= StartBattle;
        GetComponent<UnitHealthController>().onDeath -= UnitDied;
        GetComponent<UnitMovementController>().onTargetChange -= SetCurrentTarget;
    }

    public void LoadUnitAttackController(UnitDTO unitDTO, Database database)
    {
        baseAbility = database.baseAbilities[unitDTO.basicID];
        passiveAbility = database.passiveAbilities[unitDTO.passiveID];
        baseAbility.triggerStrategy.StartTrigger(unitStatusController, true);
        passiveAbility.triggerStrategy.StartTrigger(unitStatusController, false);
    }

    /// <summary>
    /// This Checks if the unit can attack.
    /// </summary>
    /// <returns></returns>
    public bool CanAttack()
    {
        return !battlestarted || unitDead;
    }

    /// <summary>
    /// Checks if basic move is Ready and can be used.
    /// </summary>
    public void BasicAttack()
    {
        unitAnimation.DoAttack(baseAbility.GetanimationData());
        baseAbility.DoAbility(unitStatusController, target.GetComponent<UnitStatusController>());
        //Debug.Log(gameObject.name + " did basic move.");
    }

    /// <summary>
    /// Checks if passive move is Ready and can be used.
    /// </summary>
    public void PassiveAttack()
    {
        unitAnimation.DoAttack(passiveAbility.GetanimationData());
        passiveAbility.DoAbility(unitStatusController, target.GetComponent<UnitStatusController>());
        //Debug.Log(gameObject.name + " did passive move.");
    }

    /// <summary>
    /// Sets in the unit is in range to use basic attack. True for in range.
    /// </summary>
    /// <param name="a"></param>
    public void CanBasicAttack(bool a)
    {
        basicAttackInRange = a;
    }

    /// <summary>
    /// This starts and ends the battle for the unit. true start false is end.
    /// </summary>
    /// <param name="x"></param>
    public void StartBattle(bool x)
    {
        if (x)
        {
            
        }
        battlestarted = x;
    }

    /// <summary>
    /// This is used to reset the timer on the basic move timer.
    /// </summary>
    public void DidBasicMove()
    {
        basicAttackLastTime = Time.time;
    }

    /// <summary>
    /// This is used to reset the timer on the passive and basic move timer.
    /// </summary>
    public void DidPassiveMove()
    {
        passiveAttackLastTime = Time.time;
        basicAttackLastTime = Time.time;
    }

    /// <summary>
    /// Listens for a event when unit dies and is revived.
    /// </summary>
    /// <param name="x"></param>
    public void UnitDied(bool x,UnitStatusController attacker, UnitStatusController defender)
    {
        unitDead = x;
    }

    public void SetCurrentTarget(Transform t)
    {
        target = t;
    }

    public float AttackStoppingDistance()
    {
        return baseAbility.range;
    }
}
