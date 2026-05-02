using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CaptureUnitEffect", menuName = "Abilities/Effects/CaptureUnitEffect", order = 0)]
public class CaptureUnitEffect : AbilityEffectsStrategy
{
    public GameObject SuccessEffect;
    public GameObject FailEffect;
    public int prob = 50;
    public float destroyDelay = 2;

    GameObject spawnedObject;
    
    public override void StartEffect(AbilityData data)
    {
        if(prob > Random.Range(0, 100))
        {
            data.battleController.TransferUnitToParty();
            spawnedObject = Instantiate(SuccessEffect,data.primaryTarget.transform);
            data.user.GetComponent<MonoBehaviour>().StartCoroutine(DestroyDelay(data));
        }
        else
        {
            spawnedObject = Instantiate(FailEffect, data.primaryTarget.transform);
            data.battleController.captureButtonMovement.TriggerTween();
        }
    }


    public IEnumerator DestroyDelay(AbilityData data)
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(spawnedObject);
        data.battleController.EndBattleOnCapture();
    }
}
