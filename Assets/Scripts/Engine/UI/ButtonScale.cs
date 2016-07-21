/// <summary>
/// ButtonScale.
/// Created by: Jorge L. Chavez Herrera.
///
/// Scales a button when pressing it, restoring it to its original scale on release.
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Class members
    public RectTransform targetTransform;
	public float duration = 0.125f;
	public Vector3 from = Vector3.one;
	public Vector3 to = Vector3.one * 1.125f;
    #endregion

    #region MonoBehaviour overrides
    private void Awake()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        targetTransform.localScale = from;
    }
    #endregion

    #region IPointerDownHandler implementation
    // This fucntion is triggered by the Unity's event system
    public void OnPointerDown (PointerEventData eventData) 
	{
		Selectable selectable = GetComponent<Selectable>();
			
		if (selectable != null && selectable.interactable == true)
		{
			StopAllCoroutines ();
			StartCoroutine (ScaleTo (to, 0.125f));
		}	
	}
	#endregion
		
	#region IPointerUpHandler implementation
	// This fucntion is triggered by the Unity's event system
	public void OnPointerUp (PointerEventData eventData) 
	{
		Selectable selectable = GetComponent<Selectable>();
			
		if (selectable != null && selectable.interactable == true)
		{
			StopAllCoroutines ();
			StartCoroutine (ScaleTo (from, 0.125f));
		}
	}
	#endregion
		
	#region Class implementation
	private IEnumerator ScaleTo (Vector3 scale, float duration)
	{
		Vector3 startScale = targetTransform.localScale;
			
		for (float t = 0 ; t < duration; t+= Time.deltaTime)
		{
			targetTransform.localScale = Vector3.Lerp (startScale, scale, Mathf.SmoothStep (0, 1, t / duration));
			yield return null;
		}

        targetTransform.localScale = scale;
	}
	#endregion
}