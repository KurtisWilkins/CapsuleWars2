using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


[CreateAssetMenu(fileName = "HealthBasedDamageEffect", menuName = "Abilities/Effects/HealthBasedDamageEffect", order = 0)]
public class DamageEffect : AbilityEffectsStrategy
{
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] public float delay = .3f;
    [Tooltip("Regualr Damage 1 is normal .5 is half 2 is double. negitive will heal")]
    [SerializeField] public float damageModifier = 1f;
    [Tooltip("Mod is added to the CurrentHP/MaxHP.")]
    [SerializeField] public float mod = 1f;
    [Tooltip("true gets bigger as health grows false gets bigger as helath shrinks.")]
    [SerializeField] bool grows = false;
    [Tooltip("Does the attack ingnore Defnece?")]
    [SerializeField] bool ignoreDefence = false;

    public override void StartEffect(AbilityData data)
    {
        foreach (UnitStatusController target in data.targets)
        {
            if (target == null)
                return;
            //Unit targetUnit = targetObject.GetComponent<Unit>();
            if (target.IsUnitDead()) { return; }
            //targetUnit.ApplyDamage(data.user.GetComponent<Unit>().damage);
            data.user.GetComponent<MonoBehaviour>().StartCoroutine(EffectDelayed(data,target));
            //DoDamage(targetUnit, data.user.GetComponent<Unit>());
        }
    }

    public IEnumerator EffectDelayed(AbilityData data, UnitStatusController target)
    {
        yield return new WaitForSeconds(delay);
        DoDamage(data, target);
    }


    /// <summary>
    /// This is used to calculate the normal damage to another unit
    /// </summary>
    /// <param name="EnemyUnit"></param>
    /// <param name="attackingUnit"></param>
    private void DoDamage(AbilityData data, UnitStatusController target)
    {
        
        float critDamage = Critdamageamount(data.user);
        float damage = 0f;

        if (data.isElement)
        {
            damage = data.user.GetAttackElement() + critDamage;
        }
        else
        {
            damage = data.user.GetAttack() + critDamage;
        }

        float defenceMod = 1f;
        if (!ignoreDefence)
        {
            defenceMod = 1000f / (1000f + target.GetDefense() * 2f);
        }

        float damageModified = defenceMod * damage * GetElementModification(data.attackType1, target);

        target.GetComponent<UnitHealthController>().TakeDamage(damageModified * damageModifier,data.user);
        //target.GetComponent<UnitMovementController>().AttackedByUnit(data.user);
    }

    /// <summary>
    /// Checks if unit crits
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private float Critdamageamount(UnitStatusController unit)
    {
        float hit = Random.Range(0, 1000);
        if (hit < unit.GetCritRate() && !unit.isUnLucky)
        {
            return unit.GetCritDamage() * 3;
        }
        else
        {
            return 0f;
        }
    }


    private float GetElementModification(ElementType_SO move, UnitStatusController defender)
    {
        float x = 1f;
        



        return x;
    }
}
