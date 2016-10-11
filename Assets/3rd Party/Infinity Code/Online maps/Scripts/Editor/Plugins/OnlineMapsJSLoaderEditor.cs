/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OnlineMapsJSLoader))]
public class OnlineMapsJSLoaderEditor:Editor
{
    public override void OnInspectorGUI()
    {
#if UNITY_WEBPLAYER
        EditorGUILayout.HelpBox("Important: In Unity Editor this script modifies WWW Security Emulation / Host URL.\nUse OnlineMapsUtils.GetWWW, to make your own request.", MessageType.Info);
        EditorGUILayout.HelpBox("Every time after you build the project, patch an HTML file.", MessageType.Info);
        if (GUILayout.Button("Patch File"))
        {
            string filename = EditorUtility.OpenFilePanel("Select HTML file", Application.dataPath, "html");
            if (!string.IsNullOrEmpty(filename)) ModifyFile(filename);
        }
#else
        EditorGUILayout.HelpBox("This component only works for Webplayer.", MessageType.Info);
#endif
    }

    public string FindAsset(string filename)
    {
#if UNITY_4_3
        string[] files = Directory.GetFiles(Application.dataPath, filename);
        return (files.Length > 0) ? files[0] : null;
#else
        string[] guids = AssetDatabase.FindAssets(filename);
        return (guids != null && guids.Length > 0)? AssetDatabase.GUIDToAssetPath(guids[0]): null;
#endif
    }

    private void ModifyFile(string filename)
    {
        if (!File.Exists(filename)) return;

        string text = File.ReadAllText(filename, Encoding.UTF8);

        if (!text.Contains("OnlineMaps.js"))
        {
            text = text.Replace("</body>", "<script src=\"OnlineMaps.js\"></script></body>");
            File.WriteAllText(filename, text, Encoding.UTF8);
        }

        string original = null;
#if UNITY_WEBPLAYER
        original = FindAsset("_OnlineMapsWebplayer");
#endif

        if (original != null)
        {
            string dest = new FileInfo(filename).DirectoryName + "/OnlineMaps.js";
            File.Copy(original, dest, true);
        }

        EditorUtility.DisplayDialog("Success", "Patching of HTML is finished.", "OK");
    }
}
