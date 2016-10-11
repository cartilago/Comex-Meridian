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


[CustomEditor(typeof(OnlineMapsNGUITextureControl))]
public class OnlineMapsNGUITextureControlEditor : Editor
{
#if NGUI
    private bool noColliderWarning;
#endif

#if NGUI
    private void OnEnable()
    {
        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(target as OnlineMapsControlBase);

        if (api.GetComponent<BoxCollider>() == null) noColliderWarning = true;
    }
#endif

    public override void OnInspectorGUI()
    {
        bool dirty = false;

        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture, ref dirty);

#if !NGUI
        if (GUILayout.Button("Enable NGUI"))
        {
            if (EditorUtility.DisplayDialog("Enable NGUI", "You have NGUI in your project?", "Yes, I have NGUI", "Cancel")) OnlineMapsEditor.AddCompilerDirective("NGUI");
        }
#else
        if (noColliderWarning)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.HelpBox("Potential problem detected:\nGameObject has no BoxCollider, so you can not control the map.", MessageType.Warning);
            if (GUILayout.Button("Add BoxCollider"))
            {
                BoxCollider bc = api.gameObject.AddComponent<BoxCollider>();
                UITexture uiTexture = api.GetComponent<UITexture>();
                if (uiTexture != null) bc.size = uiTexture.localSize;
            }

            EditorGUILayout.EndVertical();
        }

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