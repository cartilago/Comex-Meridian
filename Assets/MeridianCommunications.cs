/// <summary>
/// Meridian Comunications.
/// By Jorge L. Chavez Herrera.
///
/// Singleton class providigng simple access to comuication functions to Lista RESTWS WEB services through Unity's WWW class.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Meridian.Framework.Utils;

	
public class MeridianCommunications : MonoSingleton<MeridianCommunications> 
{	
	#region Class members
    private const string REST_WS_HEADER = "Content-Type";
    private const string REST_WS_HEADER_TYPE = "application/json";
    #endregion

    #region Class accessors
    /// <summary>
    /// Returns the URL address to comunicate with a database based on URLType setting.
    /// </summary>
    /// <value>The current UR.</value>
    static private string currentURL
	{
		get 
		{
            return "http://app-decorador-meridian.azurewebsites.net/api/";
		} 
	}
	#endregion
		
	#region Class implementation
	/// <summary>
	/// Sends a POST message to the specified URL
	/// </summary>
	/// <param name="url">URL.</param>
	/// <param name="post">Post.</param>
	static public WWW POST (string url, Dictionary<string,string> post)
	{
		WWWForm form = new WWWForm ();
		Dictionary<string,string> headers = new Dictionary<string, string>();
		headers [REST_WS_HEADER] = REST_WS_HEADER_TYPE;

		if (post != null)
		{
			foreach (KeyValuePair<string,string> post_arg in post)
			{
				form.AddField (post_arg.Key, post_arg.Value);
			}
		}
			
		byte[] rawData = form.data;	
		WWW www = new WWW (currentURL + url, rawData, headers);
		Instance.StartCoroutine (WaitForRequest(www));
			
		return www; 
	}
		
	/// <summary>
	/// Sends a GET message to the specified URL (The WWW class will use GET by default and POST if you supply a postData parameter).
	/// </summary>
	/// <param name="url">URL.</param>
	static public WWW GET (string url)
	{	
		Dictionary<string,string> headers = new Dictionary<string, string>();
		headers [REST_WS_HEADER] = REST_WS_HEADER_TYPE;
			
		WWW www = new WWW (currentURL + url, null, headers);
		Instance.StartCoroutine (WaitForRequest (www));
			
		return www;
	}
		
	/// <summary>
	/// Waits for a WWW request.
	/// </summary>
	/// <returns>The for request.</returns>
	/// <param name="www">Www.</param>
	static private IEnumerator WaitForRequest (WWW www)
	{
		yield return www;
			
		// check for errors
		if (www.error == null)
		{
            // Debug.Log("WWW Ok!: " + www.text);
		} 
		else 
		{
            // Debug.Log("WWW Error: " + www.error);
		}    
	}
	
	/// <summary>
	/// Checks if there's an active internet connection by reching TakTakTak server.
	/// </summary>
	/// <returns>The internet connection.</returns>
	/// <param name="connectionOKDelegate">Connection OK delegate.</param>
	/// <param name="noConnectionDelegate">No connection delegate.</param>
	static public IEnumerator CheckInternetConnection (SimpleDelegate connectionOKDelegate, SimpleDelegate noConnectionDelegate)
	{
        Debug.Log("Checking Internet connection...");

        WWW www = new WWW ("https://www.google.com");
			
		yield return www;
			
		if (www.error != null)
		{
            // Internet connection error
            Debug.LogError("Internet connection error: " + www.error);
				
			if (noConnectionDelegate != null)
				noConnectionDelegate ();
		} 
		else 
		{
            // Internet connection OK
            Debug.Log("Internet connection found!");
				
			if (connectionOKDelegate != null)
				connectionOKDelegate ();
		}
	} 
	#endregion
}
