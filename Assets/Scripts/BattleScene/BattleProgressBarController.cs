using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleProgressBarController : MonoBehaviour
{
    [SerializeField] private Image progressSprite;
    [SerializeField] private GameObject panel;
    
    // Start is called before the first frame update
    void Start()
    {
        BattleController.onBattleStatus += UpdateBattleProgressBar;
        BattleController.onBattleStarted += SpawnSelf;
        panel.SetActive(false);
    }


    private void OnDestroy()
    {
        BattleController.onBattleStatus -= UpdateBattleProgressBar;
        BattleController.onBattleStarted -= SpawnSelf;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBattleProgressBar(float x)
    {
        progressSprite.fillAmount = x;
    }

    public void SpawnSelf(bool x)
    {
        panel.SetActive(true);
    }
}
