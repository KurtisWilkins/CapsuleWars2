using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] public Transform characterSelectionPaneltoShutoff;
    [SerializeField] public Transform characterSelectionPanel;
    [SerializeField] public GameObject CharacterSelectionbuttonPrefab;

    [SerializeField] public Transform deploymentGridpanel;
    [SerializeField] public GameObject deploymentGridButtonPrefab;

    [SerializeField] public GameObject hidePanelButton;

    //[SerializeField] Camera iconCamera;
    //[SerializeField] Transform spawnpoint;

    // Start is called before the first frame update
    void Start()
    {
        LoadDeploymentGrid();
        LoadSelectorButtons();
        BattleController.onBattleStarted += BattleStarted;
    }

    public void OnDestroy()
    {
        BattleController.onBattleStarted -= BattleStarted;
    }

    public void LoadDeploymentGrid()
    {
        for(int i = 4; i > -5; i--)
        {
            for(int j= -10; j < -2; j++)
            {
                GameObject go = Instantiate(deploymentGridButtonPrefab, deploymentGridpanel);
                go.GetComponent<DeploymentGridButton>().HydrateGridButton(new Vector3(j*.75f,i*.75f,0));
            }
        }
    }

    public void LoadSelectorButtons()
    {
        //for(int i = 0; i < PlayerData.unitsInParty.Count; i++)
        //{
        //    GameObject b = Instantiate(CharacterSelectionbuttonPrefab, characterSelectionPanel);
        //    b.GetComponent<UnitSelectorButton>().HydrateSelectorButton(PlayerData.unitsInParty[i].UnitDTO, i);
        //}

        StartCoroutine(BuildButtons());
    }

    public void BattleStarted(bool x)
    {
        deploymentGridpanel.gameObject.SetActive(false);
        characterSelectionPaneltoShutoff.gameObject.SetActive(false);
        hidePanelButton.SetActive(false);
        GetComponent<Image>().color = new Color(255,255, 255, 1);
        GetComponent<Image>().enabled = false;
    }

    public IEnumerator BuildButtons()
    {
        //yield return new WaitForSeconds(.5f);
        //iconCamera.gameObject.SetActive(true);


        for (int i = 0; i < PlayerData.unitsInParty.Count; i++)
        {
            //GameObject tempUnit = UnitSpawner.Instance.SpawnUnit(PlayerData.unitsInParty[i].UnitDTO, spawnpoint.position, Vector3.zero);

            yield return new WaitForSeconds(.01f);
            //var tex = new RenderTexture(512, 512, 16);
            //iconCamera.targetTexture = tex;
            //Texture2D screenShot = new Texture2D(512, 512, TextureFormat.RGB24, false);
            //iconCamera.Render();
            //RenderTexture.active = tex;
            //screenShot.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            //screenShot.Apply();

            //PlayerData.unitsInParty[i].unitIcon = screenShot;
            //rawImage.texture = screenShot;

            //PlayerData.unitsInParty[i].unitIcon = screenShot;
            GameObject b = Instantiate(CharacterSelectionbuttonPrefab, characterSelectionPanel);
            b.GetComponent<UnitSelectorButton>().HydrateSelectorButton(PlayerData.unitsInParty[i].UnitDTO, i);//, screenShot);

            //Destroy(tempUnit);
        }
        //iconCamera.gameObject.SetActive(false);
    }


    //public void GetIcon(UnitDTO unitDTO, RawImage rawImage)
    //{
        
    //    StartCoroutine(GetAnImage(rawImage, unitDTO));
        
    //}


    //IEnumerator GetAnImage(RawImage rawImage, UnitDTO unitDTO)
    //{
    //    iconCamera.gameObject.SetActive(true);
    //    GameObject tempUnit = UnitSpawner.Instance.SpawnUnit(unitDTO, spawnpoint.position, Vector3.zero);

    //    yield return new WaitForSeconds(.01f);
    //    var tex = new RenderTexture(512, 512, 16);
    //    iconCamera.targetTexture = tex;
    //    Texture2D screenShot = new Texture2D(512, 512, TextureFormat.RGB24, false);
    //    iconCamera.Render();
    //    RenderTexture.active = tex;
    //    screenShot.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
    //    screenShot.Apply();

    //    rawImage.texture = screenShot;

    //    Destroy(tempUnit);
    //    iconCamera.gameObject.SetActive(false);
    //}
}
