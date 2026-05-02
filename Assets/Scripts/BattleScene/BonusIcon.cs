using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusIcon : MonoBehaviour
{
    [SerializeField] public Image icon;
    [SerializeField] public Text count;
    [SerializeField] public bool isElement;
    [SerializeField] public int id;
    [SerializeField] public AbilityDescriptionPopup popup;

    public void SetData(Sprite sprite, int _Id, bool _isElement, AbilityDescriptionPopup abilityDescriptionPopup)
    {
        icon.sprite = sprite;
        id = _Id;
        isElement = _isElement;
        popup = abilityDescriptionPopup;
        count.text = 0.ToString();
    }

    public void ClickedOnDisplay()
    {
        popup.gameObject.SetActive(true);
        if (isElement)
        {
            popup.ChangePopUP(UnitSpawner.Instance.database.elementTypes[id].nameID, 
                UnitSpawner.Instance.database.elementTypes[id].descritpionID, 
                icon.sprite);
        }
        else
        {
            popup.ChangePopUP(UnitSpawner.Instance.database.classTypes[id].nameID, 
                UnitSpawner.Instance.database.classTypes[id].descritpionID, 
                icon.sprite);
        }
    }

    public void ChangeCount(int x)
    {
        count.text = x.ToString();
    }
}
