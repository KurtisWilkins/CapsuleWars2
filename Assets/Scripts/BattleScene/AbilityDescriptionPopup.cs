using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityDescriptionPopup : MonoBehaviour
{
    [SerializeField] public Image icon;
    [SerializeField] public Text nameTitle;
    [SerializeField] public Text description;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ChangePopUP(string _name, string _descrition, Sprite _icon)
    {
        icon.sprite = _icon;
        nameTitle.text = LocalizationManager.GetTranslation(_name);
        description.text = LocalizationManager.GetTranslation(_descrition); 
    }
}
