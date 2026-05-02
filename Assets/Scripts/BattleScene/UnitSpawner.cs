using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance;
    public Database database;
    
    //[SerializeField] Camera iconCamera;
    //[SerializeField] Transform spawnpoint;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    public GameObject SpawnUnit(UnitDTO unitDTO, Vector3 position, Vector3 rotation, int teamID = 0)
    {
        GameObject unit = Instantiate(database.unitObjects[unitDTO.armatureID].unitObjectBase,position,Quaternion.Euler(rotation));

        unit.GetComponent<UnitInventory>().LoadUnitInventory(unitDTO, database);
        unit.GetComponent<UnitMovementController>().LoadUnitMovementController(unitDTO,database);
        unit.GetComponent<UnitAttackController>().LoadUnitAttackController(unitDTO,database);
        unit.GetComponent<UnitHealthController>().LoadUnitHealthController(unitDTO);
        

        unit.GetComponent<UnitStatusController>().LoadUnitStatusController(unitDTO, database, teamID);

        if (unitDTO.evolution == 1)
        {
            unit.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }
        if(unitDTO.evolution == 2)
        {
            unit.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        return unit;
    }

    //public void BuildIcon(UnitDTO unitDTO, RawImage rawImage)
    //{
    //    StartCoroutine(CreateUnitIcons(unitDTO, rawImage));
    //}

    //public IEnumerator CreateUnitIcons(UnitDTO unitDTO, RawImage rawImage)
    //{
    //    //yield return new WaitForSeconds(.5f);
    //    iconCamera.gameObject.SetActive(true);

    //    GameObject tempUnit = UnitSpawner.Instance.SpawnUnit(unitDTO, spawnpoint.position , Vector3.zero);

    //    yield return new WaitForSeconds(.01f);
    //    var tex = new RenderTexture(512, 512, 16);
    //    iconCamera.targetTexture = tex;
    //    Texture2D screenShot = new Texture2D(512, 512, TextureFormat.RGB24, false);
    //    iconCamera.Render();
    //    RenderTexture.active = tex;
    //    screenShot.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
    //    screenShot.Apply();

    //    //PlayerData.unitsInParty[i].unitIcon = screenShot;
    //    rawImage.texture = screenShot;

    //    Destroy(tempUnit);
        
    //    iconCamera.gameObject.SetActive(false);
    //}
}
