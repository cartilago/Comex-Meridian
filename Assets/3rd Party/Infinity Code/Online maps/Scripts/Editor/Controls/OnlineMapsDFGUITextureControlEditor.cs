/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_5_3P
#endif

using UnityEditor;
using UnityEngine;

#if UNITY_5_3P
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

[CustomEditor(typeof(OnlineMapsDFGUITextureControl))]
public class OnlineMapsDFGUITextureControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool dirty = false;

        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture, ref dirty);

#if !DFGUI
        if (GUILayout.Button("Enable DFGUI"))
        {
            if (EditorUtility.DisplayDialog("Enable DFGUI", "You have DFGUI in your project?", "Yes, I have DFGUI", "Cancel")) OnlineMapsEditor.AddCompilerDirective("DFGUI");
        }
#else
        base.OnInspectorGUI();
#endif

        if (dirty)
        {
            EditorUtility.SetDirty(api);
            EditorUtility.SetDirty(control);
            if (!Application.isPlaying)
            {
#if UNITY_5_3P
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
            }
            else api.Redraw();
        }
    }
}