/// <summary>
/// TakApp Debug.
/// By Jorge L. Chávez Herrera
///
/// Singleton class providing a debug console for mobile devices.
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Meridian.Framework.Utils;

namespace Meridian.Framework.Managers
{
	public class DebugManager : MonoSingleton<DebugManager>
	{
		#region Class members
	    public int maxLines = 25;
	    public GameObject deugTextPrefab;
	    public GameObject scrollView;
	    public Transform content;

	    private bool visible = false;
	    private float touchTimer;
		#endregion

	    #region MonoBehaviour overrides
	    private void Start()
	    {
	        scrollView.gameObject.SetActive(false);
	    }

	    private void Update()
	    {
	        // Touch 3 second with 3 fingers to toggle visibility

	        int touches = 0;

	        for (int i = 0; i < Input.touchCount; i++)
	        {
	            if (Input.touches[i].phase == TouchPhase.Moved || Input.touches[i].phase == TouchPhase.Stationary)
	            {
	                touches++;
	            }
	        } 

	        if (touches == 3)
	        {
	            touchTimer += Time.deltaTime;
	        }
	        else
	        {
	            touchTimer = 0;
	        }

	        if (touchTimer > 2)
	        {
	            touchTimer = 0;

	            visible = !visible;

	            scrollView.gameObject.SetActive(visible);
	        }

            if (Input.GetKeyDown(KeyCode.D))
            {
                visible = !visible;
                scrollView.gameObject.SetActive(visible);
            }
	    }
	    #endregion

	    #region Class implementation
	    static public Transform GetNewText(string text)
	    {
	        GameObject textGO = GameObject.Instantiate(Instance.deugTextPrefab);
	        textGO.GetComponent<Text>().text = text;
	        return textGO.transform;
	    }

	    static public void Log(string logString)
	    {
	        Transform t = GetNewText(logString);
	        t.SetParent(Instance.content);
	        t.localScale = Vector3.one;
	        t.SetAsLastSibling();

	        if (Instance.content.childCount > Instance.maxLines)
	            Destroy(Instance.content.GetChild(Instance.content.childCount-1));
	    }
	    #endregion
	}
}
