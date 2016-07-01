/// <summary>
/// AudioClipSource.
/// Created by: Jorge Luis Chavez Herrera.
/// 
///  Defines a wrapper clas for audioclips inlcuding relevant information for mixing.
/// </summary>
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioClipSource : ScriptableObject
{
    #region Class members
    public AudioClip audioClip;
    public float volume = 1;
    public float pitch = 1;
    public AudioMixerGroup mixerGroup;

    [System.NonSerialized]
    public float lastPlaybackTime;
    #endregion

    #region Class implementation
    public void Play(AudioSource audioSource)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.Play();
    }
    #endregion
}
