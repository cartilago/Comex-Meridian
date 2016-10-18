/// <summary>
/// Meridian App.
/// By Jorge L. Chávez Herrera
///
/// Singleton class providing high level communication functions to Comex CMS.
/// </summary>

using UnityEngine;
using System.Collections;
using Meridian.Framework.Utils;

public class MeridianApp : MonoSingleton<MeridianApp>
{
    #region Class members
    private bool _internetAvailable;
    static public SimpleDelegate adminUserReadyDelegate;
    #endregion

    #region Class accessors
    static private MeridianData.UserLogin _adminUser;
    static public MeridianData.UserLogin adminUser
    {
        get { return _adminUser; }
    }

    static private MeridianData.UserLogin _currentUser;
    static public MeridianData.UserLogin currentUser
    {
        get { return _currentUser; }
    }
    #endregion

    #region MonoBehaviour Overrides
    private void OnEnable()
    {
        StartCoroutine(MeridianCommunications.CheckInternetConnection(InternetOk, NoInternet));
    }
    #endregion

    #region Internet connection
    private void InternetOk()
    {
        _internetAvailable = true;

        if (_adminUser == null)
            GetAdminUser();
    }

    private void NoInternet()
    {
        _internetAvailable = false;
    }

    private void GetAdminUser()
    {
        UserLogin("admin", "123", GetAdminUserDelegate);
    }

    private void GetAdminUserDelegate(MeridianData.UserLoginResult loginResult)
    {
        if (loginResult != null)
        {
            _adminUser = loginResult.userList[0];

            if (adminUserReadyDelegate != null)
                adminUserReadyDelegate();
        }
    }
    #endregion

    #region Users
    /// <summary>
    /// Authenticates an existing user.
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="password"></param>
    /// <param name="userLoginDelegate"></param>
    public void UserLogin(string user, string password, SimpleDelegate<MeridianData.UserLoginResult> userLoginDelegate)
    {
        StartCoroutine(UserLoginCoroutine(user, password, userLoginDelegate));
    }

    private IEnumerator UserLoginCoroutine(string user, string password, SimpleDelegate<MeridianData.UserLoginResult> userLoginDelegate)
    {
        // Build json by hand
        string jsonString = "{ mail:'" + user + "', password:" + password + " }";

        WWW www = MeridianCommunications.POST("/Catalog/Login", jsonString);

        yield return www;

        MeridianData.UserLoginResult result = null;

        if (www.error == null)
        {
            // Since JSon comes in the form of an array we must wrap data around a class.
            result = JsonUtility.FromJson<MeridianData.UserLoginResult>("{\"userList\":" + www.text + "}");
        }
        else
        {
        }

        if (userLoginDelegate != null)
            userLoginDelegate(result);
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="registerUserDelegate"></param>
    public void RegisterUser(string token, string admin, string user, string email, string password, SimpleDelegate<MeridianData.RegisterUserResult> registerUserDelegate)
    {
        StartCoroutine(RegisterUserCoroutine(token, admin, user, email, password, registerUserDelegate));
    }

    private IEnumerator RegisterUserCoroutine(string token, string admin, string user, string email, string password, SimpleDelegate<MeridianData.RegisterUserResult> registerUserDelegate)
    {
        // Build json by hand
        string jsonString = "{" + string.Format("lEmail:\"{0}\", lToken:\"{1}\", Nombre:\"{2}\", Email:\"{3}\", Password:\"{4}\"", admin, token, user, email, password) + "}";

        WWW www = MeridianCommunications.POST("/Catalog/RegistrarUsuario", jsonString);

        yield return www;

        MeridianData.RegisterUserResult result = null;

        if (www.error == null)
        {
            // Since JSon comes in the fom of an array we must wrap data around a class.
            result = JsonUtility.FromJson<MeridianData.RegisterUserResult>("{\"registerUserList\":" + www.text + "}");
        }
        else
        {
            Debug.Log(www.error);
        }

        if (registerUserDelegate != null)
            registerUserDelegate(result);
    }
    #endregion

    #region Stores
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
            // Since JSon comes in the form of an array we must wrap data around a class.
            stores = JsonUtility.FromJson<MeridianData.Stores>("{\"storeList\":" + www.text + "}");
        }
        else
        {
            Debug.Log(www.error);
        }

        if (getStoresCatalogDelegate != null)
            getStoresCatalogDelegate(stores); 
    }

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