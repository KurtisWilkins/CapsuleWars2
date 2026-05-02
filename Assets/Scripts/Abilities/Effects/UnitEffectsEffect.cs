using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffDebuffEffect", menuName = "Abilities/Effects/BuffDebuffEffect", order = 0)]
public class UnitEffectsEffect : AbilityEffectsStrategy
{
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] public float delay = .3f;
    [Tooltip("SHould the buff always hit")]
    [SerializeField] public bool autoHit = false;
    [Tooltip("The Prefab that you want to spawn")]
    [SerializeField] public bool isProtected = false;
    [SerializeField] public float isProtectedTime = 0f;
    [SerializeField] public bool isShield = false;
    [SerializeField] public float isShieldTime = 0f;
    [SerializeField] public bool isStunned = false;
    [SerializeField] public float isStunnedTime = 0f;
    [SerializeField] public bool isFrozen = false;
    [SerializeField] public float isFrozenTime = 0f;
    [SerializeField] public bool isTrapped = false;
    [SerializeField] public float isTrappedTime = 0f;
    [SerializeField] public bool isMarked = false;
    [SerializeField] public float isMarkedTime = 0f;
    [SerializeField] public bool isUnLucky = false;
    [SerializeField] public float isUnLuckyTime = 0f;
    [SerializeField] public bool isLastStand = false;
    [SerializeField] public float isLastStandTime = 0f;
    [SerializeField] public bool isMadness = false;
    [SerializeField] public float isMadnessTime = 0f;
    [SerializeField] public bool isCursed = false;
    [SerializeField] public float isCursedTime = 0f;

    [SerializeField] public bool isAttackBoosted = false;
    [SerializeField] public float isAttackBoostedTime = 0f;
    [SerializeField] public bool isAttackBroken = false;
    [SerializeField] public float isAttackBrokenTime = 0f;
    [SerializeField] public bool isDefenseBoosted = false;
    [SerializeField] public float isDefenseBoostedTime = 0f;
    [SerializeField] public bool isDefenseBroken = false;
    [SerializeField] public float isDefenseBrokenTime = 0f;
    [SerializeField] public bool isSpeedBoosted = false;
    [SerializeField] public float isSpeedBoostedTime = 0f;
    [SerializeField] public bool isSpeedBroken = false;
    [SerializeField] public float isSpeedBrokenTime = 0f;
    [SerializeField] public bool isAccuracyBoosted = false;
    [SerializeField] public float isAccuracyBoostedTime = 0f;
    [SerializeField] public bool isAccuracyBroken = false;
    [SerializeField] public float isAccuracyBrokenTime = 0f;
    [SerializeField] public bool isCritRateBoosted = false;
    [SerializeField] public float isCritRateBoostedTime = 0f;
    [SerializeField] public bool isCritRateBroken = false;
    [SerializeField] public float isCritRateBrokenTime = 0f;
    [SerializeField] public bool isCritDamageBoosted = false;
    [SerializeField] public float isCritDamageBoostedTime = 0f;
    [SerializeField] public bool isCritDamageBroken = false;
    [SerializeField] public float isCritDamageBrokenTime = 0f;
    [SerializeField] public bool isResistenceBoosted = false;
    [SerializeField] public float isResistenceBoostedTime = 0f;
    [SerializeField] public bool isResistenceBroken = false;
    [SerializeField] public float isResistenceBrokenTime = 0f;


    public override void StartEffect(AbilityData data)
    {
        foreach (UnitStatusController target in data.targets)
        {
            if (target == null)
                return;

            if (target.IsUnitDead()) { return; }

            data.user.GetComponent<MonoBehaviour>().StartCoroutine(EffectDelayed(data, target));

        }
    }

    public IEnumerator EffectDelayed(AbilityData data, UnitStatusController target)
    {
        yield return new WaitForSeconds(delay);
        DoEffect(data, target);
    }

    public bool BuffHits(AbilityData data, UnitStatusController target)
    {
        if(target.isProtected && target.GetTeamID() != data.user.GetTeamID())
            return false;
        if (autoHit)
            return true;
        int maxRoll = data.user.GetAccuracy() + target.GetResistance();
        int roll = Random.Range(1, maxRoll + 1);

        if (roll >= target.GetResistance())
            return true;
        else
            return false;
    }

    public void DoEffect(AbilityData data, UnitStatusController target)
    {
        //Debug.Log("Attempting to apply Buff/Debuff");
        if (BuffHits(data, target))
        {
            Debug.Log("Buff/Debuff Hits");
            if (isProtected && !target.isProtected)
                target.SetIsProtected(true, data.user);
            target.isProtectedCoroutine = target.StartCoroutine(RemoveEffectIsProtected(target, data.user));
            if (isShield && !target.isShield)
                target.SetIsShield(true, data.user);
            target.isShieldCoroutine = target.StartCoroutine(RemoveEffectIsSheild(target, data.user));
            if (isStunned && !target.isStunned)
                target.SetIsStunned(true, data.user);
            target.isStunnedCoroutine = target.StartCoroutine(RemoveEffectIsStunned(target, data.user));
            if (isFrozen && !target.isFrozen)
                target.SetIsFrozen(true, data.user);
            target.isFrozenCoroutine = target.StartCoroutine(RemoveEffectIsFrozen(target, data.user));
            if (isTrapped && !target.isTrapped)
                target.SetIsTrapped(true, data.user);
            target.isTrappedCoroutine = target.StartCoroutine(RemoveEffectIsTrapped(target, data.user));
            if (isMarked && !target.isMarked)
                target.SetIsMarked(true, data.user);
            target.isMarkedCoroutine = target.StartCoroutine(RemoveEffectIsMarked(target, data.user));
            if (isUnLucky && !target.isUnLucky)
                target.SetIsUnLucky(true, data.user);
            target.isUnLuckyCoroutine = target.StartCoroutine(RemoveEffectIsUnLucky(target, data.user));
            if (isLastStand && !target.isLastStand)
                target.SetIsLastStand(true, data.user);
            target.isLastStandCoroutine = target.StartCoroutine(RemoveEffectIsLastStand(target, data.user));
            if (isMadness && !target.isMadness)
                target.SetIsMadness(true, data.user);
            target.isMadnessCoroutine = target.StartCoroutine(RemoveEffectIsMadness(target, data.user));
            if (isCursed && !target.isCursed)
                target.SetIsCursed(true, data.user);
            target.isCursedCoroutine = target.StartCoroutine(RemoveEffectIsCursed(target, data.user));

            if (isAttackBoosted && !target.isAttackBoosted)
                target.SetIsAttackBoosted(true, data.user);
            target.isAttackBoostedCoroutine = target.StartCoroutine(RemoveEffectIsAttackBoosted(target, data.user));
            if (isAttackBroken && !target.isAttackBroken)
                target.SetIsAttackBroken(true, data.user);
            target.isAttackBrokenCoroutine = target.StartCoroutine(RemoveEffectIsAttackBroken(target, data.user));
            if (isDefenseBoosted && !target.isDefenseBoosted)
                target.SetIsDefenseBoosted(true, data.user);
            target.isDefenseBoostedCoroutine = target.StartCoroutine(RemoveEffectIsDefenseBoosted(target, data.user));
            if (isDefenseBroken && !target.isDefenseBroken)
                target.SetIsDefenseBroken(true, data.user);
            target.isDefenseBrokenCoroutine = target.StartCoroutine(RemoveEffectIsDefenseBroken(target, data.user));
            if (isSpeedBoosted && !target.isSpeedBoosted)
                target.SetIsSpeedBoosted(true, data.user);
            target.isSpeedBoostedCoroutine = target.StartCoroutine(RemoveEffectIsSpeedBoosted(target, data.user));
            if (isSpeedBroken && !target.isSpeedBroken)
                target.SetIsSpeedBroken(true, data.user);
            target.isSpeedBrokenCoroutine = target.StartCoroutine(RemoveEffectIsSpeedBroken(target, data.user));
            if (isAccuracyBoosted && !target.isAccuracyBoosted)
                target.SetIsAccuracyBoosted(true, data.user);
            target.isAccuracyBoostedCoroutine = target.StartCoroutine(RemoveEffectIsAccuracyBoosted(target, data.user));
            if (isAccuracyBroken && !target.isAccuracyBroken)
                target.SetIsAccuracyBroken(true, data.user);
            target.isAccuracyBrokenCoroutine = target.StartCoroutine(RemoveEffectIsAccuracyBroken(target, data.user));
            if (isCritRateBoosted && !target.isCritRateBoosted)
                target.SetIsCritRateBoosted(true, data.user);
            target.isCritRateBoostedCoroutine = target.StartCoroutine(RemoveEffectIsCritRateBoosted(target, data.user));
            if (isCritRateBroken && !target.isCritRateBroken)
                target.SetIsCritRateBroken(true, data.user);
            target.isCritRateBrokenCoroutine = target.StartCoroutine(RemoveEffectIsCritRateBroken(target, data.user));
            if (isCritDamageBoosted && !target.isCritDamageBoosted)
                target.SetIsCritDamageBoosted(true, data.user);
            target.isCritDamageBoostedCoroutine = target.StartCoroutine(RemoveEffectIsCritDamageBoosted(target, data.user));
            if (isCritDamageBroken && !target.isCritDamageBroken)
                target.SetIsCritDamageBroken(true, data.user);
            target.isCritDamageBrokenCoroutine = target.StartCoroutine(RemoveEffectIsCritDamageBroken(target, data.user));
            if (isResistenceBoosted && !target.isResistenceBoosted)
                target.SetIsResistenceBoosted(true, data.user);
            target.isResistenceBoostedCoroutine = target.StartCoroutine(RemoveEffectIsResistenceBoosted(target, data.user));
            if (isResistenceBroken && !target.isResistenceBroken)
                target.SetIsResistenceBroken(true, data.user);
            target.isResistenceBrokenCoroutine = target.StartCoroutine(RemoveEffectIsResistenceBroken(target, data.user));
        }
    }

    public IEnumerator RemoveEffectIsProtected(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isProtectedTime);
        target.SetIsProtected(false, attacker);
    }

    public IEnumerator RemoveEffectIsSheild(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isProtectedTime);
        target.SetIsProtected(false, attacker);
    }

    public IEnumerator RemoveEffectIsStunned(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isStunnedTime);
        target.SetIsStunned(false, attacker);
    }

    public IEnumerator RemoveEffectIsFrozen(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isFrozenTime);
        target.SetIsFrozen(false, attacker);
    }

    public IEnumerator RemoveEffectIsTrapped(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isTrappedTime);
        target.SetIsTrapped(false, attacker);
    }

    public IEnumerator RemoveEffectIsMarked(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isMarkedTime);
        target.SetIsMarked(false, attacker);
    }

    public IEnumerator RemoveEffectIsUnLucky(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isUnLuckyTime);
        target.SetIsUnLucky(false, attacker);
    }

    public IEnumerator RemoveEffectIsLastStand(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isLastStandTime);
        target.SetIsLastStand(false, attacker);
    }

    public IEnumerator RemoveEffectIsMadness(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isMadnessTime);
        target.SetIsMadness(false, attacker);
    }

    public IEnumerator RemoveEffectIsCursed(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isCursedTime);
        target.SetIsCursed(false, attacker);
    }

    public IEnumerator RemoveEffectIsAttackBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isAttackBoostedTime);
        target.SetIsAttackBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsAttackBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isAttackBrokenTime);
        target.SetIsAttackBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsDefenseBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isDefenseBoostedTime);
        target.SetIsDefenseBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsDefenseBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isDefenseBrokenTime);
        target.SetIsDefenseBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsSpeedBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isSpeedBoostedTime);
        target.SetIsSpeedBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsSpeedBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isSpeedBrokenTime);
        target.SetIsSpeedBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsAccuracyBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isAccuracyBoostedTime);
        target.SetIsAccuracyBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsAccuracyBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isAccuracyBrokenTime);
        target.SetIsAccuracyBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsCritRateBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isCritRateBoostedTime);
        target.SetIsCritRateBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsCritRateBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isCritRateBrokenTime);
        target.SetIsCritRateBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsCritDamageBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isCritDamageBoostedTime);
        target.SetIsCritDamageBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsCritDamageBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isCritDamageBrokenTime);
        target.SetIsCritDamageBroken(false, attacker);
    }

    public IEnumerator RemoveEffectIsResistenceBoosted(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isResistenceBoostedTime);
        target.SetIsResistenceBoosted(false, attacker);
    }

    public IEnumerator RemoveEffectIsResistenceBroken(UnitStatusController target, UnitStatusController attacker)
    {
        yield return new WaitForSeconds(isResistenceBrokenTime);
        target.SetIsResistenceBroken(false, attacker);
    }
}

