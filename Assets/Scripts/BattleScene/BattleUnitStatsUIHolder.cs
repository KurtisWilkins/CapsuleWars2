using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitStatsUIHolder : MonoBehaviour
{
    [SerializeField] public Text titleText;
    [SerializeField] public GameObject barCardPrefab;
    [SerializeField] public BattleUnitStatsUIBarHandler attackFriendly;
    [SerializeField] public BattleUnitStatsUIBarHandler attackEnemy;
    [SerializeField] public BattleUnitStatsUIBarHandler damagedFriendly;
    [SerializeField] public BattleUnitStatsUIBarHandler damagedEnemy;
    [SerializeField] public BattleUnitStatsUIBarHandler healingFriendly;
    [SerializeField] public BattleUnitStatsUIBarHandler healingEnemy;

    [SerializeField] public Button friendlyButton;
    [SerializeField] public Button enemyButton;
    [SerializeField] public Button attackButton;
    [SerializeField] public Button tankButton;
    [SerializeField] public Button healingButton;

    public bool isFriendlySelected = false;

    [SerializeField] public RectTransform descriptionPanel;

    [SerializeField] public bool isOnScreen = false;

    [SerializeField] private float onScreenValue = -320f;
    [SerializeField] private float offScreenValue = 340f;

    [SerializeField] private float LastTime;
    [SerializeField] private float waitTime;

    [SerializeField] private Color colorAttack = Color.red;
    [SerializeField] private Color colorTank = Color.blue;
    [SerializeField] private Color colorHeal = Color.green;

    private bool isbattleEnded = false;

    // Start is called before the first frame update
    void Start()
    {
        ButtonClickFriendlyEnemy();
        BattleController.onBattleEnded += BattleEnded;
    }

    void OnDestroy()
    {
        BattleController.onBattleEnded -= BattleEnded;
    }

    public void BattleEnded(bool x)
    {
        isbattleEnded = x;
    }

    public void ButtonClickFriendlyEnemy()
    {
        isFriendlySelected = !isFriendlySelected;
        friendlyButton.interactable = !isFriendlySelected;
        enemyButton.interactable = isFriendlySelected;

        if (attackFriendly.gameObject.activeSelf || attackEnemy.gameObject.activeSelf)
        {
            ButtonClickAttack();
        }
        else if (damagedFriendly.gameObject.activeSelf || damagedEnemy.gameObject.activeSelf)
        {
            ButtonClickTank();
        }
        else if (healingFriendly.gameObject.activeSelf || healingEnemy.gameObject.activeSelf)
        {
            ButtonClickHealing();
        }
    }


    public void ButtonClickAttack()
    {
        titleText.text = LocalizationManager.GetTranslation("Attack");
        if (isFriendlySelected)
        {
            attackFriendly.gameObject.SetActive(true);
            attackEnemy.gameObject.SetActive(false);
            damagedFriendly.gameObject.SetActive(false);
            damagedEnemy.gameObject.SetActive(false);
            healingFriendly.gameObject.SetActive(false);
            healingEnemy.gameObject.SetActive(false);
        }
        else
        {
            attackFriendly.gameObject.SetActive(false);
            attackEnemy.gameObject.SetActive(true);
            damagedFriendly.gameObject.SetActive(false);
            damagedEnemy.gameObject.SetActive(false);
            healingFriendly.gameObject.SetActive(false);
            healingEnemy.gameObject.SetActive(false);
        }
        attackButton.interactable = false;
        tankButton.interactable = true;
        healingButton.interactable = true;
    }

    public void ButtonClickTank()
    {
        titleText.text = LocalizationManager.GetTranslation("Tank");
        if (isFriendlySelected)
        {
            attackFriendly.gameObject.SetActive(false);
            attackEnemy.gameObject.SetActive(false);
            damagedFriendly.gameObject.SetActive(true);
            damagedEnemy.gameObject.SetActive(false);
            healingFriendly.gameObject.SetActive(false);
            healingEnemy.gameObject.SetActive(false);
        }
        else
        {
            attackFriendly.gameObject.SetActive(false);
            attackEnemy.gameObject.SetActive(false);
            damagedFriendly.gameObject.SetActive(false);
            damagedEnemy.gameObject.SetActive(true);
            healingFriendly.gameObject.SetActive(false);
            healingEnemy.gameObject.SetActive(false);
        }
        attackButton.interactable = true;
        tankButton.interactable = false;
        healingButton.interactable = true;
    }

    public void ButtonClickHealing()
    {
        titleText.text = LocalizationManager.GetTranslation("Healer");
        if (isFriendlySelected)
        {
            attackFriendly.gameObject.SetActive(false);
            attackEnemy.gameObject.SetActive(false);
            damagedFriendly.gameObject.SetActive(false);
            damagedEnemy.gameObject.SetActive(false);
            healingFriendly.gameObject.SetActive(true);
            healingEnemy.gameObject.SetActive(false);
        }
        else
        {
            attackFriendly.gameObject.SetActive(false);
            attackEnemy.gameObject.SetActive(false);
            damagedFriendly.gameObject.SetActive(false);
            damagedEnemy.gameObject.SetActive(true);
            healingFriendly.gameObject.SetActive(false);
            healingEnemy.gameObject.SetActive(true);
        }
        attackButton.interactable = true;
        tankButton.interactable = true;
        healingButton.interactable = false;
    }

    public void SpawnBar(bool isPlayerTeam,UnitStatusController unit, BattleUnitStats battleUnitStats,BattleController battleController)
    {
        if (isPlayerTeam)
        {
            GameObject ab = Instantiate(barCardPrefab, attackFriendly.parentT);
            GameObject tb = Instantiate(barCardPrefab, damagedFriendly.parentT);
            GameObject hb = Instantiate(barCardPrefab, healingFriendly.parentT);
            attackFriendly.AddUIBar(ab.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorAttack, 1));
            damagedFriendly.AddUIBar(tb.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorTank, 2));
            healingFriendly.AddUIBar(hb.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorHeal, 3));
        }
        else
        {
            GameObject ab = Instantiate(barCardPrefab, attackEnemy.parentT);
            GameObject tb = Instantiate(barCardPrefab, damagedEnemy.parentT);
            GameObject hb = Instantiate(barCardPrefab, healingEnemy.parentT);
            attackEnemy.AddUIBar(ab.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorAttack, 1));
            damagedEnemy.AddUIBar(tb.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorTank, 2));
            healingEnemy.AddUIBar(hb.GetComponent<BattleUnitStatsUIBar>().LoadData(unit, battleUnitStats, battleController, colorHeal, 3));
        }
    }

    public void Despawn(UnitStatusController unit)
    {
        if (0 == unit.GetTeamID())
        {
            foreach (var b in attackFriendly.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    attackFriendly.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
            foreach (var b in damagedFriendly.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    damagedFriendly.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
            foreach (var b in healingFriendly.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    healingFriendly.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
        }
        else
        {
            foreach (var b in attackEnemy.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    attackEnemy.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
            foreach (var b in damagedEnemy.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    damagedEnemy.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
            foreach (var b in healingEnemy.statsUIBars)
            {
                if (unit.GetUnitID() == b.unitStatusController.GetUnitID())
                {
                    BattleController.onStatsChange -= b.UpdateUI;
                    if(isbattleEnded){ break; }
                    GameObject g = b.gameObject;
                    healingEnemy.RemoveUIBAr(b);
                    if (g != null)
                    {
                        DOTween.Kill(g.GetComponent<RectTransform>());
                        Destroy(g);
                    }
                    break;
                }
            }
        }
    }

    public void TriggerStatsPanel()
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
