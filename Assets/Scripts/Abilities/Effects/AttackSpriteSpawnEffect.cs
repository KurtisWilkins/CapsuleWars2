using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackSpriteSpawnEffect", menuName = "Abilities/Effects/AttackSpriteSpawnEffect", order = 0)]
public class AttackSpriteSpawnEffect : AbilityEffectsStrategy
{
    [Tooltip("Delay in seconds for start of effect.")]
    [SerializeField] public float delay = .3f;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float deathTime = 1f;
    [Tooltip("The Sound to play")]
    [SerializeField] public int soundEffect = 7;
    [Tooltip("The Prefab that you want to spawn")]
    [SerializeField] public GameObject objectToSpawn;
    [Tooltip("How long the effect visuals last.")]
    [SerializeField] public float frameRate = 1f;
    [Tooltip("Different Lists for Elements.")]
    [SerializeField] public List<SprtieList> frames;
    
    
    GameObject go;

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
        SpawnEffect(data, target);
    }

    public void SpawnEffect(AbilityData data, UnitStatusController target)
    {
        go = Instantiate(objectToSpawn, target.gameObject.GetComponent<UnitAttackController>().targetSpawn.transform);
        go.GetComponent<EffectSpriteController>().LoadEffect(frames[data.user.GetComponent<UnitHealthController>().element1.elementTypeID].frames, frameRate);
        Destroy(go,deathTime); AudioManager.instance.PlaySFX(soundEffect);
    }
}

[System.Serializable]
public class SprtieList
{
    [SerializeField] public string name;
    [SerializeField] public List<Sprite> frames;
}
