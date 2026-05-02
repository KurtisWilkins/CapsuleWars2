using DG.Tweening;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleFieldWeatherUIController : MonoBehaviour
{
    [SerializeField] public RectTransform descriptionPanel;

    [SerializeField] public bool isOnScreen = false;

    [SerializeField] private float onScreenValue = -320f;
    [SerializeField] private float offScreenValue = 340f;

    [SerializeField] private Text battleFieldName;
    [SerializeField] private Text weatherName;

    // Start is called before the first frame update
    void Start()
    {
        battleFieldName.text = LocalizationManager.GetTranslation(PlayerData.battleData.battleMap.nameID);
        weatherName.text = LocalizationManager.GetTranslation(PlayerData.battleData.battleWeather.nameID);
        StartCoroutine(TriggerComeOnScreen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerPanelMovement()
    {
        if (isOnScreen)
        {
            descriptionPanel.DOLocalMoveY(offScreenValue, .5f, true);
            isOnScreen = false;
        }
        else
        {
            descriptionPanel.DOLocalMoveY(onScreenValue, .5f, true);
            isOnScreen = true;
        }
    }

    public IEnumerator TriggerComeOnScreen()
    {
        yield return new WaitForSeconds(.5f);
        TriggerPanelMovement();
        yield return new WaitForSeconds(1.5f);
        TriggerPanelMovement();
    }
}
