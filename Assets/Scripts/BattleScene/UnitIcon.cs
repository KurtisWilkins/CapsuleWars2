using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitIcon : MonoBehaviour
{
    [SerializeField] public Camera cameraRend;
    [SerializeField] public RawImage rawImage;
    [SerializeField] public RenderTexture renderTexture;
    [SerializeField] public UnitInventory inventory;

    public void HydrateUnitIcon(UnitDTO unitDTO, Database database)
    {
        if(cameraRend == null)
        {
            cameraRend = gameObject.GetComponentInChildren<Camera>();
        }
        inventory.LoadUnitInventory(unitDTO, database);
        LoadIconImage();
    }

    /// <summary>
    /// Mark the character dead
    /// </summary>
    public void MarkDead()
    {
        
    }

    /// <summary>
    /// Mark the character Alive
    /// </summary>
    public void MarkAlive()
    {
        
    }

    public void LoadIconImage()
    {
        renderTexture = new RenderTexture(512, 512, 16);
        renderTexture.Create();

        cameraRend.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
    }

    void OnDestroy()
    {
        // Clean up Render Texture when the object is destroyed
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}
