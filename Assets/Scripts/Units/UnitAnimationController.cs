using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        SetWalking(0f);
        GetComponent<UnitHealthController>().onDeath += DoDeath;
    }

    private void OnDestroy()
    {
        GetComponent<UnitHealthController>().onDeath -= DoDeath;
    }

    /// <summary>
    /// 0 is idle .5 is run and 1 is stunned
    /// </summary>
    /// <param name="x"></param>
    public void SetWalking(float x)
    {
        animator.SetFloat("RunState", x);
    }

    /// <summary>
    /// Attack State 0 is Normal and 1 is skill. Normal State 0 is Mele, .5 is Bow, and 1 is Magic. Skill State 0 is Mele, .5 is Bow, and 1 is Magic.
    /// </summary>
    /// <param name="attackState"></param>
    /// <param name="normalState"></param>
    /// <param name="skillState"></param>
    public void DoAttack(AnimationData data)
    {
        animator.SetFloat("AttackState",data.attackState);
        animator.SetFloat("NormalState", data.normalState);
        animator.SetFloat("SkillState", data.skillState);
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Death Animation Trigger.
    /// </summary>
    public void DoDeath(bool dead, UnitStatusController attacker, UnitStatusController defender)
    {
        if (dead)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            animator.SetTrigger("Revive");
        }
    }
}

[System.Serializable]
public class AnimationData
{
    [Tooltip("Attack State 0 is Normal and 1 is skill.")]
    [SerializeField] public float attackState;
    [Tooltip("Normal State 0 is Mele, .5 is Bow, and 1 is Magic.")]
    [SerializeField] public float normalState;
    [Tooltip("Skill State 0 is Mele, .5 is Bow, and 1 is Magic.")]
    [SerializeField] public float skillState;
}
