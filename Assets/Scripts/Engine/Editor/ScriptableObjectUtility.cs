/// <summary>
/// ScriptableObjectUtility.
/// Created by Jorge L. Chavez Herrera
/// 
/// Provides functionality for easily crating unique new ScriptableObject asset files.
/// </summary>
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

namespace Meridian.Framework.Utils
{
	static public class ScriptableObjectUtility
    {
        #region Classimplementation

        /// <summary>
        //	Utility for easily crating unique new ScriptableObject asset files.
        /// </summary>
        public static void CreateAsset<T> () where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T> ();
			
			string path = AssetDatabase.GetAssetPath (Selection.activeObject);
			if (path == "") 
			{
				path = "Assets";
			} 
			else if (Path.GetExtension (path) != "") 
			{
				path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
			}
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");
			
			AssetDatabase.CreateAsset (asset, assetPathAndName);
			
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = asset;
		}
	}
    #endregion
}