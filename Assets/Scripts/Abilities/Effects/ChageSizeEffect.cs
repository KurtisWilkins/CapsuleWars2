using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangeSizeEffect", menuName = "Abilities/Effects/ChangeSizeEffect", order = 0)]
public class ChageSizeEffect : AbilityEffectsStrategy
{
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] public float delay = .3f;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float deathTime = 5f;
    [Tooltip("Precentage of change for the effect.")]
    [SerializeField] public float changeSize = 1f;

    public override void StartEffect(AbilityData data)
    {
        foreach (UnitStatusController target in data.targets)
        {
            if (target == null)
                return;

            if (target.IsUnitDead() || target.isCursed) { return; }

            data.user.GetComponent<MonoBehaviour>().StartCoroutine(EffectDelayed(data, target));
        }
    }

    public IEnumerator EffectDelayed(AbilityData data, UnitStatusController target)
    {
        yield return new WaitForSeconds(delay);
        target.transform.localScale = new Vector3(changeSize * target.transform.localScale.x, changeSize * target.transform.localScale.y, 1f);
        //target.BuildMassMod target.GetMass() * changeSize;
        yield return new WaitForSeconds(deathTime);
        target.transform.localScale = new Vector3(target.transform.localScale.x / changeSize, target.transform.localScale.y / changeSize, 1f);
    }
}