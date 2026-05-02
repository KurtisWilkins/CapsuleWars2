using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawedOnProjectileEffect", menuName = "Abilities/Effects/SpawnedOnProjectile", order = 0)]
public class SpawnTargetPrefabOnProjectileEffect : AbilityEffectsStrategy
{
    [Tooltip("Prefab you want to spawn.")]
    [SerializeField] GameObject targetPrefab;
    [Tooltip("Delay in seconds for destroying the effect.")]
    [SerializeField] float desroyDelay = -1;
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] float spawnDelay = .3f;
    [Tooltip("Type of Effect.")]
    [SerializeField] float deathTime;
    [Tooltip("Sets Scale of effect.")]
    [SerializeField] Vector3 scaleToSpawn = new Vector3(0, 0, 0);
    [Tooltip("Sets Scale of effect.")]
    [SerializeField] Vector3 rotationToSpawn = new Vector3(0, 0, 0);


    public override void StartEffect(AbilityData data)//, float deathTime = 0f)
    {
        //EffectData effectData = new EffectData();//data.user.GetComponent<Unit>());
        data.user.GetComponent<MonoBehaviour>().StartCoroutine(Effect(data.targets, deathTime));
    }

    public IEnumerator Effect(List<UnitStatusController> target, float time)
    {
        yield return new WaitForSeconds(spawnDelay);

        foreach(UnitStatusController controller in target)
        {
            GameObject paretneObject = controller.GetComponent<UnitAttackController>().projectileSpawn;
            GameObject effect = Instantiate(targetPrefab, paretneObject.transform);
            effect.transform.localScale = new Vector3(effect.transform.localScale.x + scaleToSpawn.x, effect.transform.localScale.y + scaleToSpawn.y, effect.transform.localScale.z + scaleToSpawn.z);
            //effect.transform.rotation = Quaternion.Euler(rotationToSpawn);
            //effect.transform.LookAt(target.transform);
            effect.transform.position = paretneObject.transform.position;
            //effectData.SetEffectStats(isBuff, effectType, effect);
            if (time > desroyDelay)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return new WaitForSeconds(desroyDelay);
            }

            //if (!effectData.CheckIsCanceled())
            //{
            //    Destroy(effect);
            //}
            //effectData.GetUnit().RemoveEffectFromUnit(effectData);
        }

        
    }
}
