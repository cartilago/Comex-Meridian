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

[CustomEditor(typeof(OnlineMapsIGUITextureControl))]
public class OnlineMapsIGUITextureControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool dirty = false;
        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture, ref dirty);

#if !IGUI
        if (GUILayout.Button("Enable iGUI"))
        {
            if (EditorUtility.DisplayDialog("Enable iGUI", "You have iGUI in your project?", "Yes, I have iGUI", "Cancel")) OnlineMapsEditor.AddCompilerDirective("IGUI");
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