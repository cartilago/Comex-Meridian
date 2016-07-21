/// <summary>
/// Button Sound.
/// Created by: Jorge L. Chavez Herrera.
///
/// Plays back an AudioSource when a button is pressed.
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonSound : MonoBehaviour, IPointerClickHandler
{
    #region Class members
    public AudioClipSource audioClipSource;
    public float volume = 1;
	public float delay = 0;
	#endregion
		
	#region Class accessors
	private AudioSource _cachedAudioSource;
	private AudioSource cachedAudioSource
	{
		get
		{
			if (_cachedAudioSource == null)
			{
				// Try finding an already exixting AudioSource
				_cachedAudioSource = GetComponent<AudioSource>();

                // If no AudioSource was found, add a new one
                if (_cachedAudioSource == null)
                {
                    _cachedAudioSource = gameObject.AddComponent<AudioSource>();
                    _cachedAudioSource.playOnAwake = false;
                }
			}
				
			return _cachedAudioSource;
		}
	}
	#endregion
		
	#region IPointerDownHandler implementation
	// This fucntion is triggered by the Unity's event system
	public void OnPointerClick (PointerEventData eventData) 
	{
		Selectable selectable = GetComponent<Selectable>();
			
		if (selectable != null && selectable.interactable == true && audioClipSource != null)
		{
            audioClipSource.Play(cachedAudioSource);
		}
	}
	#endregion
}
