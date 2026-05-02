using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInspection : MonoBehaviour
{
    [SerializeField] BattleController battleController;
    UnitStatusController currentHit;
    [SerializeField] UnitDescriptionBattlePanel unitDescriptionBattlePanel;

    // Update is called once per frame
    void Update()
    {
        if(battleController.battleEnded == true) { return; }
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Vector2 point = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                Collider2D hit = Physics2D.OverlapPoint(point);
                if(hit != null)
                {
                    if(hit.gameObject.tag == "Unit")
                    {
                        UnitStatusController unitStatus = hit.gameObject.GetComponent<UnitStatusController>();
                        currentHit = unitStatus;
                        unitDescriptionBattlePanel.FillDescription(unitStatus.gameObject);
                            
                    }
                }
            }
        }
    }

}
