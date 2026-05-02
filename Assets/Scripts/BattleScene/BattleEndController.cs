using DG.Tweening;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleEndController : MonoBehaviour
{
    [SerializeField] public GameObject expPanelPrefab;
    [SerializeField] public GameObject unitStatCardPrefab;
    [SerializeField] public Transform gridForExpPanels;
    [SerializeField] public Transform gridForYourStatsPanels;

    [SerializeField] public GameObject battlemanager;

    [SerializeField] public GameObject expPanel;
    [SerializeField] public GameObject xpScrollPanel;
    [SerializeField] public GameObject statsScrollPanel;
    [SerializeField] public GameObject statsXPToggleButton;
    [SerializeField] public Text statsXpToggleButtonText;
    [SerializeField] public GameObject rewardPanel;
    [SerializeField] public GameObject recruitPanel;

    [SerializeField] public CharacterSelectionUI characterSelectionUI;
    [SerializeField] public BattleController battleController;
    [SerializeField] public UnitStaticsDetailedBattlePopUp unitStaticsDetailedBattlePopUp;

    [SerializeField] public Text titleCondtion;
    

    [SerializeField] private Button button;

    List<Tweener> t = new List<Tweener>();
    List<GameObject> expButtons = new List<GameObject>();

    bool loadedGold = false;
    //bool loadedequipment = false;
    bool loadedrecruit = false;

    bool won = false;
    bool nextButtonClicked = false;

    public static event Action<int> onExpReward;
    public static event Action<int> onGoldReward;
    public static event Action<EquipmentDTO> onEquiptmentReward;



    public void LoadEndBattlePanel(bool w)
    {
        if (w)
        {
            titleCondtion.text = LocalizationManager.GetTranslation("Victory");
            statsXpToggleButtonText.text = LocalizationManager.GetTranslation("Show Stats");
            titleCondtion.color = Color.yellow;
            t.Add(titleCondtion.transform.DOPunchScale(Vector3.one, .5f, 5, 1));
            AudioManager.instance.PlayBGM(PlayerData.victoryMusicIntro);
            StartCoroutine(waitForSound());
            if (PlayerData.battleData.hasQuest) { QuestManager.instance.MarkQuestComplete(PlayerData.battleData.QuestID); }
            if (PlayerData.battleData.hasEvent) { EventManager.instance.MarkEventComplete(PlayerData.battleData.eventID); }
        }
        else
        {
            titleCondtion.text = LocalizationManager.GetTranslation("Defeat");
            statsXpToggleButtonText.text = LocalizationManager.GetTranslation("Show Stats");
            titleCondtion.color = Color.red;
            t.Add(titleCondtion.transform.DOPunchScale(Vector3.one, .5f, 5, 1));

        }
        won = w;
        StartCoroutine(SpawnEXPPanel());
    }

    public void OnDestroy()
    {

        foreach (var x in t)
        {
            x.Kill();
        }
    }


    public IEnumerator SpawnEXPPanel()
    {
        for (int i = 0; i < BattleController.deployedUnits.Count; i++)
        {
            if (BattleController.deployedUnits[i].GetTeamID() == 0)
            {
                GameObject e = Instantiate(unitStatCardPrefab, gridForYourStatsPanels);
                //Debug.Log("Unit Stats Class count" + battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats.Count);
                //expButtons.Add(e);
                for (int j = 0; j < battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats.Count; j++)
                {

                    if (BattleController.deployedUnits[i].GetUnitID() == battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats[j].unitID)
                    {
                        e.GetComponent<UnitStatsCard>().HydrateSelectorButton(BattleController.deployedUnits[i].dTO, BattleController.deployedUnits[i].GetTeamID(), battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats[j], battleController, unitStaticsDetailedBattlePopUp);
                    }
                }
            }
        }


        expPanel.SetActive(true);
        for (int i = 0; i < BattleController.deployedUnits.Count; i++)
        {
            if (BattleController.deployedUnits[i].GetTeamID() == 0)
            {
                GameObject e = Instantiate(expPanelPrefab, gridForExpPanels);
                expButtons.Add(e);
                for (int j = 0; j < PlayerData.unitsInParty.Count; j++)
                {
                    BattleUnitStats battleUnitStats = new BattleUnitStats();
                    for (int g = 0; g < battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats.Count; g++)
                    {
                        if (BattleController.deployedUnits[i].GetUnitID() == battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats[g].unitID)
                        {
                            battleUnitStats = battleController.unitBattleStats[BattleController.deployedUnits[i].GetTeamID()].battleUnitStats[g];
                        }
                    }

                    if (BattleController.deployedUnits[i].GetUnitID() == PlayerData.unitsInParty[j].UnitDTO.iD)
                    {
                        e.GetComponent<UnitSelectorButton>().LoadUnitExpPanel(BattleController.deployedUnits[i].dTO, PlayerData.unitsInParty[j].UnitDTO,battleUnitStats,won);
                    }
                }
                yield return new WaitForSeconds(.1f);
            }
        }

        StartCoroutine(DoExp());
    }

    IEnumerator DoExp()
    {
        yield return new WaitForSeconds(.5f);
        if (!nextButtonClicked)
        {
            onExpReward?.Invoke(PlayerData.battleData.expReward);
        }

    }


    public void NextButtonClick()
    {
        nextButtonClicked = true;
        StopCoroutinesonButtons();
        expPanel.SetActive(false);
        statsXPToggleButton.SetActive(false);
        if (PlayerData.battleData.hasGoldReward)
        {
            if (!loadedGold)
            {
                LoadReward();
                loadedGold = true;
                //StartCoroutine(DelayButtonClick());
                return;
            }
            rewardPanel.SetActive(false);
        }
        //if (PlayerData.battleData.hasEquipmentReward && won)
        //{
        //    if (!loadedequipment)
        //    {
        //        LoadEquipmentReward();
        //        loadedequipment = true;
        //        return;
        //    }
        //    equipmentPanel.SetActive(false);
        //}
        if (PlayerData.battleData.hasUnitReward && won)
        {
            if (!loadedrecruit)
            {
                StartCoroutine(LoadUnitReward());
                loadedrecruit = true;
                //StartCoroutine(DelayButtonClick());
                return;
            }
            recruitPanel.SetActive(false);
        }
        //battleController.EndBattle();
        //TransitionController.instance.ChangeSceneWithDelay("ID", 0,1f);
        StartCoroutine(DisableRewardPanels());
    }

    //IEnumerator DelayButtonClick()
    //{
    //    button.interactable = false;
    //    yield return new WaitForSeconds(.1f);
    //    button.interactable = true;
    //}
    //public void LoadGoldReward()
    //{
    //    goldPanel.gameObject.SetActive(true);

    //    RewardGoldPanel rewardGold = goldPanel.GetComponent<RewardGoldPanel>();

    //    rewardGold.LoadGold(PlayerData.battleData.goldReward);

    //    PlayerData.ChangeGold(PlayerData.battleData.goldReward);

    //}


    public void LoadReward()
    {
        rewardPanel.gameObject.SetActive(true);

        RewardPanel rewardEquipment = rewardPanel.GetComponent<RewardPanel>();
        if (won)
        {
            rewardEquipment.LoadReward(PlayerData.battleData.equipmentReward, PlayerData.battleData.goldReward);
            PlayerData.AddEquipment(PlayerData.battleData.equipmentReward);
            PlayerData.ChangeGold(PlayerData.battleData.goldReward);
        }
        else
        {
            rewardEquipment.LoadReward(PlayerData.battleData.equipmentReward, PlayerData.battleData.goldReward);
            PlayerData.AddEquipment(PlayerData.battleData.equipmentReward);
            PlayerData.ChangeGold(PlayerData.battleData.goldReward);
        }

    }


    public IEnumerator LoadUnitReward()
    {
        recruitPanel.gameObject.SetActive(true);

        RewardRecruitmentPanel rewardRecruitment = recruitPanel.GetComponent<RewardRecruitmentPanel>();

        GameObject g = UnitSpawner.Instance.SpawnUnit(PlayerData.battleData.unitReward, new Vector3(0, 0, 100), Vector3.zero);

        yield return new WaitForSeconds(.2f);

        rewardRecruitment.LoadRecruit(g.GetComponent<UnitStatusController>(), PlayerData.battleData.unitReward);

        UnitBattlePlacement unitBattlePlacement = new UnitBattlePlacement();

        unitBattlePlacement.UnitDTO = PlayerData.battleData.unitReward;

        PlayerData.unitsInParty.Add(unitBattlePlacement);
        PlayerController.instance.GetComponent<PlayerUnitParty>().LoadParty();
        Destroy(g);
    }

    public IEnumerator DisableRewardPanels()
    {
        battleController.TransistionBackDelay();
        yield return new WaitForSeconds(.1f);
        gameObject.SetActive(false);
    }

    public IEnumerator waitForSound()
    {
        //Wait Until Sound has finished playing
        while (AudioManager.instance.bgm[PlayerData.victoryMusicIntro].isPlaying)
        {
            yield return null;
        }

        //Auidio has finished playing, disable GameObject
        AudioManager.instance.PlayBGM(PlayerData.victoryMusic);
    }

    public void StopCoroutinesonButtons()
    {
        foreach (GameObject g in expButtons)
        {
            g.GetComponent<MonoBehaviour>().StopAllCoroutines();
        }
    }

    public void ButtonClickToggleStatsXP()
    {
        if (xpScrollPanel.activeSelf)
        {
            xpScrollPanel.SetActive(false);
            statsScrollPanel.SetActive(true);
            statsXpToggleButtonText.text = LocalizationManager.GetTranslation("Show XP");
        }
        else
        {
            xpScrollPanel.SetActive(true);
            statsScrollPanel.SetActive(false);
            statsXpToggleButtonText.text = LocalizationManager.GetTranslation("Show Stats");
        }
    }
}
