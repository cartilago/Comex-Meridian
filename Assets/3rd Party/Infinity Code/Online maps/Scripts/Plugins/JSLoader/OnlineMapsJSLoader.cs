/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Processes all Unity requests through XMLHttpRequest. \n
/// This allows you to bypass restrictions of Webplayer without using a proxy server, and accelerate the processing of requests.\n
/// <strong>Important: </strong> In Unity Editor this script modifies WWW Security Emulation / Host URL.\n
/// Use OnlineMapsUtils.GetWWW, to make your own request.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/JS Loader")]
public class OnlineMapsJSLoader : MonoBehaviour
{
    private List<OnlineMapsWWW> requests;

    private void Start()
    {
        requests = new List<OnlineMapsWWW>();
        OnlineMapsUtils.OnGetWWW += OnGetWWW;
    }

    private OnlineMapsWWW OnGetWWW(string url)
    {
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        string reqID = Guid.NewGuid().ToString("N");
        Application.ExternalCall("OnlineMapsGetURL", url, reqID, gameObject.name, "OnGetURLSuccess", "OnGetURLError");
        OnlineMapsWWW www = new OnlineMapsWWW(url, OnlineMapsWWW.RequestType.direct, reqID);
        requests.Add(www);
        return www;
#elif UNITY_WEBPLAYER && UNITY_EDITOR
        EditorSettings.webSecurityEmulationHostUrl = url.Substring(0, url.IndexOf("/", 9));
        OnlineMapsWWW www = new OnlineMapsWWW(url);
        return www;
#else 
        return null;
#endif
    }

    private void OnGetURLSuccess(string response)
    {
        string[] resp = response.Split(new[] {"|||"}, StringSplitOptions.None);
        string id = resp[0];
        string headers = resp[1];
        string data = resp[2];
        OnlineMapsWWW www = requests.FirstOrDefault(r => r.id == id);
        if (www == null) return;
        requests.Remove(www);
        byte[] bytes = Convert.FromBase64String(data);
        www.SetBytes(headers, bytes);
    }

    private void OnGetURLError(string response)
    {
        string[] resp = response.Split(new[] { "|||" }, StringSplitOptions.None);
        string id = resp[0];
        OnlineMapsWWW www = requests.FirstOrDefault(r => r.id == id);
        if (www == null) return;
        requests.Remove(www);
        www.SetError(resp[1]);
    }
}
