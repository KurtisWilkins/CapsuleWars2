using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeploymentGridButton : MonoBehaviour
{
    [SerializeField] public Vector3 deploymentTransform;
    [SerializeField] public UnitIcon unitIcon;

    UnitSelectorButton unitSelectorButton = null;

    private int unitSelected;
    public int unitDeployedID;
    private GameObject deployedUnit;
    [SerializeField] private bool isDeployed = false;
    [SerializeField] private bool isBattleScene = true;

    public int buttonSoundDeploy = 1;
    public int buttonSoundRemove = 5;
    public int buttonSoundNotAllowed = 3;
    [SerializeField] private Button button;

    public static event Action<DeploymentGridButton> OnDeployUnit;

    // Start is called before the first frame update
    void Start()
    {
        UnitSelectorButton.OnUnitSelected += SetUnitSelected;
        unitIcon.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnitSelectorButton.OnUnitSelected -= SetUnitSelected;
    }

    public void HydrateGridButton(Vector3 spawnpoint)
    {
        deploymentTransform = spawnpoint;
        unitIcon.gameObject.SetActive(false);
    }


    public void buttonClicked()
    {
        
        
        if (isDeployed)
        {
            Destroy(deployedUnit);
            unitIcon.gameObject.SetActive(false);
            isDeployed = false;
            OnDeployUnit?.Invoke(this);
            AudioManager.instance.PlaySFX(buttonSoundRemove);
        }
        else
        {
            if (unitSelectorButton == null)
            {
                AudioManager.instance.PlaySFX(buttonSoundNotAllowed);
                return;
            }
            if (unitSelectorButton.isDeployed)
            {
                AudioManager.instance.PlaySFX(buttonSoundNotAllowed);
                return;
            }

            if (isBattleScene)
            {
                deployedUnit = UnitSpawner.Instance.SpawnUnit(PlayerData.unitsInParty[unitSelected].UnitDTO, deploymentTransform, new Vector3(0f, 180f, 0f), 0);
            }
            
            unitIcon.gameObject.SetActive(true);
            unitIcon.HydrateUnitIcon(PlayerData.unitsInParty[unitSelected].UnitDTO, UnitSpawner.Instance.database);
            unitDeployedID = unitSelected;
            isDeployed = true;
            OnDeployUnit?.Invoke(this);
            AudioManager.instance.PlaySFX(buttonSoundDeploy);
        }
        StartCoroutine(DelayButtonClick());
    } 

    public void SetUnitSelected(UnitSelectorButton x)
    {
        unitSelectorButton = x;
        unitSelected = unitSelectorButton.unitID;
    }

    IEnumerator DelayButtonClick()
    {
        button.interactable = false;
        yield return new WaitForSeconds(.1f);
        button.interactable = true;
    }
}
