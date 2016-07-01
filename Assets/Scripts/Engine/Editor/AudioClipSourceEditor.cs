/// <summary>
/// AudioSourceEditor.
/// Created by Jorge L. Chavez Herrera.
/// 
/// Provides functionality for creating AudioSource scriptable objects within the Unity editor.

/// </summary>
using UnityEngine;
using UnityEditor;
using Meridian.Framework.Utils;


public class AudioClipSourceEditor
{
    #region Class implementation
    [MenuItem("Assets/Create/AudioClipSource")]
    static public void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<AudioClipSource>();
    }
    #endregion
}
