/// <summary>
/// Lista App.
/// By Jorge L. Chávez Herrera
///
/// Singleton class providing high level communication functions to Comex CMS.
/// </summary>

using UnityEngine;
using System.Collections;
using Meridian.Framework.Utils;

public class MeridianApp : MonoSingleton<MeridianApp>
{
    #region MonoBehaviour Overrides
    #endregion

    #region Stores catalog
    /// <summary>
    /// Gets the stores catalog.
    /// </summary>
    public void GetStoresCatalog(SimpleDelegate<MeridianData.Stores> getStoresCatalogDelegate)
    {
        StartCoroutine(GetStoresCatalogCoroutine(getStoresCatalogDelegate));
    }

    private IEnumerator GetStoresCatalogCoroutine(SimpleDelegate<MeridianData.Stores> getStoresCatalogDelegate)
    {
        WWW www = MeridianCommunications.GET("/Catalog/Get/Sucursal/0");
        yield return www;

        MeridianData.Stores stores = null;

        if (www.error == null)
        {
            // Since JSon comes in the fom of an array we must wrap data around a class.
            stores = JsonUtility.FromJson<MeridianData.Stores>("{\"storeList\":" + www.text + "}");
        }
        else
        {
            Debug.Log(www.error);
        }

        if (getStoresCatalogDelegate != null)
            getStoresCatalogDelegate(stores); 
    }
    #endregion

    #region Store
    /// <summary>
    /// Gets a single store specified by ID.
    /// </summary>
    public void GetStore(int id, SimpleDelegate<MeridianData.Store> getStoreDelegate)
    {
        StartCoroutine(GetStoreCoroutine(id, getStoreDelegate));
    }

    private IEnumerator GetStoreCoroutine(int id, SimpleDelegate<MeridianData.Store> getStoreDelegate)
    {
        WWW www = MeridianCommunications.GET("/Catalog/Get/Sucursal/0/ID/"+id);
        yield return www;

        MeridianData.Stores stores = null;

        if (www.error == null)
        {
            // Since JSon comes in the fom of an array we must wrap data around a class.
            stores = JsonUtility.FromJson<MeridianData.Stores>("{\"storeList\":" + www.text + "}");
        }
        else
        {
            Debug.Log(www.error);
        }

        if (getStoreDelegate != null)
            getStoreDelegate(stores.storeList[0]);
    }
    #endregion
}