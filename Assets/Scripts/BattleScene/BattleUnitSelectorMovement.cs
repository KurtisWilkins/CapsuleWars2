using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitSelectorMovement : MonoBehaviour
{
    [SerializeField] public RectTransform descriptionPanel;

    [SerializeField] public bool isOnScreen = false;

    [SerializeField] private float onScreenValue = -320f;
    [SerializeField] private float offScreenValue = 340f;

    [SerializeField] private float LastTime;
    [SerializeField] private float waitTime;

    // Start is called before the first frame update
    void Start()
    {
        LastTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerDescriptionPanel()
    {
        if (isOnScreen)
        {
            descriptionPanel.DOLocalMoveX(offScreenValue, .5f, true);
            isOnScreen = false;
        }
        else
        {
            descriptionPanel.DOLocalMoveX(onScreenValue, .5f, true);
            isOnScreen = true;
        }


    }

    public bool TimeToWait()
    {
        if (LastTime > (Time.time - waitTime))
        {
            return false;
        }
        else
        {
            LastTime = Time.time;
            return true;
        }
    }
}
