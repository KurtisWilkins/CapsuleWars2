using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardGoldPanel : MonoBehaviour
{
    [SerializeField] public Image goldIcon;
    [SerializeField] public TextMeshProUGUI goldAmount;
    [SerializeField] public Image additionalIcon;
    [SerializeField] public TextMeshProUGUI additionalAmount;

    public void Start()
    {
        //goldIcon.gameObject.SetActive(false);
        //goldAmount.gameObject.SetActive(false);
        //additionalIcon.gameObject.SetActive(false);
        //additionalAmount.gameObject.SetActive(false);
    }

    public void LoadGold( int x)
    {
        goldIcon.gameObject.SetActive(true);
        goldAmount.gameObject.SetActive(true);

        //goldIcon.sprite = icon;
        goldAmount.text = "+" + string.Format("{0:#,0}", x);
    }

    public void LoadAdditional(Sprite icon, int x)
    {
        additionalIcon.gameObject.SetActive(true);
        additionalAmount.gameObject.SetActive(true);

        goldIcon.sprite = icon;
        goldAmount.text = "+" + string.Format("{0:#,0}", x);
    }
}
