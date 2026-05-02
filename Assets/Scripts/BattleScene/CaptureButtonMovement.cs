using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CaptureButtonMovement : MonoBehaviour
{
    [SerializeField] public RectTransform descriptionPanel;

    [SerializeField] public bool isOnScreen = false;

    [SerializeField] private float onScreenValue = 1167f;
    [SerializeField] private float offScreenValue = 1600f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerTween()
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
}
