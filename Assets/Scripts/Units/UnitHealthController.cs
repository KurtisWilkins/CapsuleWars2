using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class UnitHealthController : MonoBehaviour
{
    [SerializeField] private float currentPercentage = 50f;
    [SerializeField] private float currentHealth = 50f;
    [SerializeField] private float maxHealth = 50f;

    [SerializeField] private bool dead = false;
    [SerializeField] private bool isHitEffectON = false;

    private UnitStatusController unitStatusController;
    private UnitMovementController unitMovementController;

    [SerializeField] private SpriteRenderer[] bodyParts;
    [SerializeField] private Material[] bodyPartMaterials;

    [SerializeField] float flashTime = .5f;

    public ElementType_SO element1;
    public ElementType_SO element2;

    public event Action<bool,UnitStatusController, UnitStatusController> onDeath;
    public event Action<bool,UnitStatusController, UnitStatusController> onRevive;
    public event Action<float, UnitStatusController, UnitStatusController> onHealthChange;

    private void Awake()
    {
        GetMaterials();
    }

    // Start is called before the first frame update
    void Start()
    {
        unitStatusController = GetComponent<UnitStatusController>();
        unitMovementController = GetComponent<UnitMovementController>();
        BattleController.onBattleStarted += SetHealthOnBattleStart;
    }

    private void OnDestroy()
    {
        BattleController.onBattleStarted -= SetHealthOnBattleStart;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadUnitHealthController(UnitDTO unitDTO)
    {
        currentPercentage = unitDTO.currentHealthPrecent;
        element1 = UnitSpawner.Instance.database.elementTypes[unitDTO.type1ID];
        element2 = UnitSpawner.Instance.database.elementTypes[unitDTO.type2ID];
    }

    public void UnitDeath(bool x,UnitStatusController attacker)
    {
        onDeath?.Invoke(x, attacker,unitStatusController);
        dead = x;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void TakeDamage(float x,UnitStatusController attacker)
    {
        if(x >= 0f) 
        { 
            CheckAttackerIsCloser(attacker); 
        }
        modifyHealthStat(-x);
        if (currentHealth == 0 && !dead)
        {
            UnitDeath(true, attacker);
        }
        onHealthChange?.Invoke(x, attacker,unitStatusController);
        StartCoroutine(HitEffect());
    }

    public void ReviveUnit(float x,UnitStatusController reviver)
    {
        onHealthChange?.Invoke(x, reviver,unitStatusController);
        onRevive?.Invoke(true, reviver,unitStatusController);
        modifyHealthStat(x);
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

    }

    public void modifyHealthStat(float x)
    {
        currentHealth = Mathf.Clamp(currentHealth += x,0,maxHealth);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetNewCurrentHealth()
    {
        return (float)currentHealth/(float)maxHealth;
    }

    public void SetHealthOnBattleStart(bool x)
    {
        maxHealth = unitStatusController.GetHealth();
        currentHealth = currentPercentage * maxHealth;
    }
    

    public IEnumerator HitEffect()
    {
        if (isHitEffectON)
        {
            //yield return new WaitForSeconds(.01f); 
            yield return null;
        }
        else
        {

            isHitEffectON = true;
            SetFlashAMount(.6f);

            float currentFlashAmount = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < flashTime)
            {
                //iterate ElaspedTime
                elapsedTime += Time.deltaTime;

                //lerp the flash amount
                currentFlashAmount = UnityEngine.Mathf.Lerp(.6f, 0f, (elapsedTime / flashTime));
                SetFlashAMount(currentFlashAmount);
                yield return null;
            }
            //yield return new WaitForSeconds(.25f);
            //bodyMaterial.SetFloat("_HitEffectBlend", 0f);
            isHitEffectON = false;
        }

    }

    private void SetFlashAMount(float flashAmount)
    {
        for (int i = 0; i < bodyPartMaterials.Length; i++)
        {
            bodyPartMaterials[i].SetFloat("_HitEffectBlend", flashAmount);
        }
    }

    private void GetMaterials()
    {
        bodyPartMaterials = new Material[bodyParts.Length];
        for(int i = 0;  i < bodyParts.Length; i++)
        {
            bodyParts[i].material = new Material(bodyParts[i].GetComponent<SpriteRenderer>().material);
            bodyPartMaterials[i] = bodyParts[i].material;
        }
    }

    private void CheckAttackerIsCloser(UnitStatusController unitStatus)
    {
        unitMovementController.UnitWasAttackedCheck(unitStatus);
    }
}
