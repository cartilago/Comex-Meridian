/// <summary>
/// About panel.
/// Provides functionalty for showing stores in a Google map.
///
/// Created by Jorge L. Chavez Herrera.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using InfinityCode.OnlineMapsExamples;

public class StoreLocatorPanel : Panel 
{
    #region Class members
    public Camera mapCamera;
    public OnlineMaps map;
    private bool storesLoaded;
    public Text[] closestStoreNames;
    public Text[] closestStoreDistances;
    private MeridianData.Stores stores;
    private Vector2 userCoordinates;
    #endregion

    #region MonoBehaviour overrides
    private void OnEnable()
    {
        if (mapCamera != null) mapCamera.gameObject.SetActive(true);
        if (map != null) map.gameObject.SetActive(true);

        if (storesLoaded == false)
        {
            storesLoaded = true;
            MeridianApp.Instance.GetStoresCatalog(GetStoresCatalogDelegate);

            OnlineMapsLocationService locationService = OnlineMapsLocationService.instance;

            if (locationService != null)
            {
                locationService.OnLocationChanged += OnLocationChanged;
            }
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
    private void GetStoresCatalogDelegate(MeridianData.Stores inStores)
    {
        if (inStores != null)
        {
            stores = inStores;

            foreach (MeridianData.Store store in inStores.storeList)
            {
                OnlineMapsMarker marker = OnlineMaps.instance.AddMarker(float.Parse(store.Longitud), float.Parse(store.Latitud), store.Tienda.TrimEnd());
                marker.scale = 0.5f;
            }

            Debug.Log(inStores.storeList.Length + " Stores got");

            UpdateClosestStores(userCoordinates);
        }
    }

    private void OnLocationChanged(Vector2 coordinates)
    {
        userCoordinates = coordinates;

        if (stores != null)
            UpdateClosestStores(userCoordinates);
    }

    // When the location has changed
    private void UpdateClosestStores(Vector2 gpsCoordinates)
    {
        // Sort stores by distance
        List<MeridianData.Store> distanceSortedStores = new List<MeridianData.Store>(stores.storeList);

        Debug.Log(distanceSortedStores.Count);

        distanceSortedStores.Sort(delegate (MeridianData.Store s1, MeridianData.Store s2) {
                return Vector2.Distance(gpsCoordinates, s1.coordinates).CompareTo
                  ((Vector2.Distance(gpsCoordinates, s2.coordinates)));
            });

        for (int i = 0; i < 3; i++)
        {
            Vector2 markerCoordinates = distanceSortedStores[i].coordinates;
            float distance = OnlineMapsUtils.DistanceBetweenPoints(gpsCoordinates, markerCoordinates).magnitude;
            closestStoreNames[i].text = distanceSortedStores[i].Tienda;
            closestStoreDistances[i].text = string.Format("{0:0.00} km", distance);
        }

        // Redraw map.
        OnlineMaps.instance.Redraw();
    }
    #endregion
}
