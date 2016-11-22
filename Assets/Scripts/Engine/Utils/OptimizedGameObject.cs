/// <summary>
/// Optimized GameObject.
/// Created by: Jorge L. Chavez Herrera
/// 
/// Defines accessors to cached versions of the most frequently used components.
/// </summary>

using UnityEngine;
using System.Collections;

public class OptimizedGameObject : MonoBehaviour
{	
	#region Class accessors
	/// <summary>
	/// Gets the cached Transform component.
	/// </summary>
	/// <value>The cached transform.</value>
	private Transform _cachedTransform;
	public Transform cachedTransform 
	{
		get 
		{
			// Store this component's reference for the first time
			if (_cachedTransform == null)
				_cachedTransform = GetComponent<Transform>();
			
			return _cachedTransform;
		}
	}
	
	/// <summary>
	/// Gets the cached rect transform.
	/// </summary>
	/// <value>The cached rect transform.</value>
	private RectTransform _cachedRectTransform;
	public RectTransform cachedRectTransform 
	{
		get 
		{
			// Store this component's reference for the first time
			if (_cachedRectTransform == null)
				_cachedRectTransform = GetComponent<RectTransform>();
			
			return _cachedRectTransform;
		}
	}
	
	/// <summary>
	/// Gets the cached audio component.
	/// </summary>
	/// <value>The cached audio.</value>
	private AudioSource _cachedAudio;
	public AudioSource cachedAudio 
	{
		get 
		{
            // Store this component's reference for the first time
            if (_cachedAudio == null)
            {
                _cachedAudio = GetComponent<AudioSource>();

                if (_cachedAudio == null)
                    _cachedAudio = gameObject.AddComponent<AudioSource>();
            }
			
			return _cachedAudio;
		}
	}
    #endregion
}