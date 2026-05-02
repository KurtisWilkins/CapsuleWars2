using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KnockBackEffect", menuName = "Abilities/Effects/AttackKnockBackEffect", order = 0)]
public class KnockBackEffect : AbilityEffectsStrategy
{
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] public float delay = .3f;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float deathTime = 1f;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float moveSpeed = 1f;

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
        Vector3 dir = (target.transform.position - data.user.transform.position).normalized * moveSpeed;
        target.GetComponent<UnitMovementController>().knockBack = true;
        target.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        target.GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
        yield return new WaitForSeconds(deathTime);
        target.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        target.GetComponent<UnitMovementController>().knockBack = false;
    }
}