using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "Spawed Projectile Effect", menuName = "Abilities/Effects/SpawnedProjectile", order = 0)]
public class SpawnTargetProjectilePrefabEffect : AbilityEffectsStrategy
{
    [SerializeField] GameObject targetPrefab;
    [SerializeField] List<AbilityEffectsStrategy> effectStrategies = new List<AbilityEffectsStrategy>();
    [SerializeField] float desroyDelay = -1;
    [SerializeField] float spawnDelay = .3f;
    [SerializeField] Vector3 spawnPosition = Vector3.zero;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float frameRate = 1f;
    [Tooltip("Different Lists for Elements.")]
    [SerializeField] public List<SprtieList> frames;
    public override void StartEffect(AbilityData data)
    {

        foreach (UnitStatusController targetObject in data.targets)
        {
            if (targetObject == null)
                return;
            data.user.GetComponent<MonoBehaviour>().StartCoroutine(Effect(targetObject,data));
        }
    }

    public IEnumerator Effect(UnitStatusController target,AbilityData data)
    {
        yield return new WaitForSeconds(spawnDelay);

        GameObject effect = Instantiate(targetPrefab,data.user.GetComponent<UnitAttackController>().projectileSpawn.transform);
        effect.transform.localPosition = effect.transform.localPosition + spawnPosition;
        Projectile projectile = effect.GetComponent<Projectile>();
        projectile.LoadProjectileData(effectStrategies, data,target.gameObject);
        if(frames.Count > 0)
        {
            effect.GetComponent<EffectSpriteController>().LoadEffect(frames[data.user.GetComponent<UnitHealthController>().element1.elementTypeID].frames, frameRate);
        }
        //effect.transform.position = target.GetComponent<Unit>().centerTarget.transform.position;

        yield return new WaitForSeconds(desroyDelay);
        //Destroy(effect);
    }
}
