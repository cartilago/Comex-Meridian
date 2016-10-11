/// <summary>
/// About panel.
/// Provides functionalty for showing stores in a Google map.
///
/// Created by Jorge L. Chavez Herrera.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using InfinityCode.OnlineMapsExamples;

public class StoreLocatorPanel : Panel 
{
    #region Class members
    public Camera mapCamera;
    public OnlineMaps map;
    private bool storesLoaded;
    #endregion

    #region MonoBehaviour overrides
    private void Start()
    {
        
    }

    private void OnEnable()
    {
        if (mapCamera != null) mapCamera.gameObject.SetActive(true);
        if (map != null) map.gameObject.SetActive(true);

        if (storesLoaded == false)
        {
            storesLoaded = true;
            MeridianApp.Instance.GetStoresCatalog(GetStoresCatalogDelegate);
        }
    }

    private void OnDisable()
    {
       if (mapCamera != null) mapCamera.gameObject.SetActive(false);
       if (map != null) map.gameObject.SetActive(false);
    }
    #endregion

    #region Panel overrides
    #endregion

    #region Class implementation
    private void GetStoresCatalogDelegate(MeridianData.Stores stores)
    {
        foreach (MeridianData.Store store in stores.storeList)
        {
            OnlineMapsMarker marker = OnlineMaps.instance.AddMarker(float.Parse(store.Longitud), float.Parse(store.Latitud), store.Tienda.TrimEnd());
            marker.scale = 0.5f;
        }

        Debug.Log(stores.storeList.Length + " Stores got");
    }
    #endregion
}
