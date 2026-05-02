using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] public UnitHealthController healthController;

    [SerializeField] GameObject healthBarBackground;
    [SerializeField] Image healthBar;
    [SerializeField] Color good;
    [SerializeField] Color medium;
    [SerializeField] Color low;

    [SerializeField] bool isDead = false;


    // Start is called before the first frame update
    void Start()
    {
        healthController.onHealthChange += UpdateHealthBar;
        healthController.onDeath += UnitDied;
    }

    private void OnDestroy()
    {
        healthController.onHealthChange -= UpdateHealthBar;
        healthController.onDeath -= UnitDied;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthBar(float x,UnitStatusController attacker, UnitStatusController defender)
    {
        healthBarBackground.SetActive(!isDead);
        healthBar.fillAmount = healthController.GetNewCurrentHealth();
        //Debug.Log("Event value: " + x + " Function amount: " + healthBar.fillAmount);

        if(x > .6f)
        {
            healthBar.color = good;
        }
        if(x > .3f)
        {
            healthBar.color = medium;
        }
        else
        {
            healthBar.color = low;
        }
    }

    public void UnitDied(bool b,UnitStatusController attacker, UnitStatusController defender) 
    {
        isDead = b;
        healthBar.color = low;
        healthBarBackground.SetActive(false);
    }
}
