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

[CustomEditor(typeof(OnlineMapsTextureControl))]
public class OnlineMapsTextureControlEditor : Editor
{
    protected OnlineMapsControlBase3D control;
    private bool showMarkers;

    protected SerializedProperty pAllowUserControl;
    protected SerializedProperty pAllowAddMarkerByM;
    protected SerializedProperty pAllowAddMarker3DByN;
    protected SerializedProperty pAllowZoom;
    protected SerializedProperty pZoomInOnDoubleClick;
    protected SerializedProperty pInvertTouchZoom;
    protected SerializedProperty pAllowCameraControl;
    protected SerializedProperty pCameraDistance;
    protected SerializedProperty pCameraRotation;
    protected SerializedProperty pCameraSpeed;
    protected SerializedProperty pCameraAdjustTo;
    protected SerializedProperty pMarker2DMode;
    protected SerializedProperty pMarker2DSize;
    protected SerializedProperty pMarker3DScale;
    protected SerializedProperty pAllowDefaultMarkerEvents;
    protected SerializedProperty pMarkers3D;
    protected SerializedProperty pActiveCamera;
    protected SerializedProperty pZoomMode;
    protected SerializedProperty pDefault3DMarker;

    protected virtual void CacheSerializedProperties()
    {
        pAllowUserControl = serializedObject.FindProperty("allowUserControl");
        pAllowAddMarkerByM = serializedObject.FindProperty("allowAddMarkerByM");
        pAllowAddMarker3DByN = serializedObject.FindProperty("allowAddMarker3DByN");
        pAllowZoom = serializedObject.FindProperty("allowZoom");
        pZoomInOnDoubleClick = serializedObject.FindProperty("zoomInOnDoubleClick");
        pInvertTouchZoom = serializedObject.FindProperty("invertTouchZoom");
        pAllowCameraControl = serializedObject.FindProperty("allowCameraControl");
        pCameraDistance = serializedObject.FindProperty("cameraDistance");
        pCameraRotation = serializedObject.FindProperty("cameraRotation");
        pCameraSpeed = serializedObject.FindProperty("cameraSpeed");
        pCameraAdjustTo = serializedObject.FindProperty("cameraAdjustTo");
        pMarker2DMode = serializedObject.FindProperty("marker2DMode");
        pMarker2DSize = serializedObject.FindProperty("marker2DSize");
        pMarker3DScale = serializedObject.FindProperty("marker3DScale");
        pAllowDefaultMarkerEvents = serializedObject.FindProperty("allowDefaultMarkerEvents");
        pMarkers3D = serializedObject.FindProperty("markers3D");
        pActiveCamera = serializedObject.FindProperty("activeCamera");
        pZoomMode = serializedObject.FindProperty("zoomMode");
        pDefault3DMarker = serializedObject.FindProperty("default3DMarker");
    }

    private void DrawCameraControlGUI()
    {
        
        bool allowCameraControl = pAllowCameraControl.boolValue;
        if (allowCameraControl) EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.PropertyField(pAllowCameraControl);

        if (pAllowCameraControl.boolValue)
        {
            EditorGUILayout.PropertyField(pCameraDistance);
            EditorGUILayout.PropertyField(pCameraRotation);
            EditorGUILayout.PropertyField(pCameraSpeed, new GUIContent("Camera Rotation Speed"));
            EditorGUILayout.PropertyField(pCameraAdjustTo);
        }

        if (allowCameraControl) EditorGUILayout.EndVertical();
    }

    public virtual void DrawMarker2DPropsGUI()
    {
        EditorGUI.BeginChangeCheck();

        int oldMode = pMarker2DMode.enumValueIndex;
        EditorGUILayout.PropertyField(pMarker2DMode);
        if (pMarker2DMode.enumValueIndex == (int)OnlineMapsMarker2DMode.billboard)
        {
            EditorGUILayout.PropertyField(pMarker2DSize);
            if (pMarker2DSize.floatValue < 1) pMarker2DSize.floatValue = 1;
        }
        if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
        {
            control.Clear2DMarkerInstances((OnlineMapsMarker2DMode)oldMode);
            OnlineMaps.instance.Redraw();
        }
    }

    protected void DrawMarkersGUI(ref bool dirty)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        showMarkers = OnlineMapsEditor.Foldout(showMarkers, string.Format("3D markers (Count: {0})", pMarkers3D.arraySize));

        if (showMarkers)
        {
            EditorGUILayout.PropertyField(pDefault3DMarker);
            EditorGUILayout.PropertyField(pMarker3DScale);
            EditorGUILayout.PropertyField(pAllowDefaultMarkerEvents);

            int removedIndex = -1;

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < pMarkers3D.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                OnlineMapsMarker3DPropertyDrawer.isRemoved = false;
                OnlineMapsMarker3DPropertyDrawer.isEnabledChanged = null;

                EditorGUILayout.PropertyField(pMarkers3D.GetArrayElementAtIndex(i), new GUIContent("Marker " + (i + 1)));

                if (OnlineMapsMarker3DPropertyDrawer.isRemoved) removedIndex = i;
                if (OnlineMapsMarker3DPropertyDrawer.isEnabledChanged.HasValue) control.markers3D[i].enabled = OnlineMapsMarker3DPropertyDrawer.isEnabledChanged.Value;

                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) dirty = true;

            if (removedIndex != -1)
            {
                ArrayUtility.RemoveAt(ref control.markers3D, removedIndex);
                dirty = true;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Marker"))
            {
                if (!Application.isPlaying)
                {
                    OnlineMapsMarker3D marker = new OnlineMapsMarker3D
                    {
                        position = control.GetComponent<OnlineMaps>().position,
                        scale = pMarker3DScale.floatValue
                    };
                    ArrayUtility.Add(ref control.markers3D, marker);
                }
                else
                {
                    GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    control.AddMarker3D(OnlineMaps.instance.position, prefab);
                    OnlineMapsUtils.DestroyImmediate(prefab);
                }
                dirty = true;
            }
        }

        EditorGUILayout.EndVertical();
    }

    protected void DrawPropsGUI(ref bool dirty)
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(pAllowUserControl);
        EditorGUILayout.PropertyField(pAllowAddMarkerByM);
        EditorGUILayout.PropertyField(pAllowAddMarker3DByN);

        DrawZoomGUI();
        DrawCameraControlGUI();

        EditorGUILayout.PropertyField(pActiveCamera, new GUIContent("Camera"));
        DrawMarker2DPropsGUI();

        if (EditorGUI.EndChangeCheck()) dirty = true;
    }

    private void DrawZoomGUI()
    {
        bool allowZoom = pAllowZoom.boolValue;
        if (allowZoom) EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.PropertyField(pAllowZoom);
        
        if (pAllowZoom.boolValue)
        {
            EditorGUILayout.PropertyField(pZoomInOnDoubleClick);
            EditorGUILayout.PropertyField(pInvertTouchZoom);
            EditorGUILayout.PropertyField(pZoomMode);

            DrawZoomLate();
        }
        if (allowZoom) EditorGUILayout.EndVertical();
    }

    protected virtual void DrawZoomLate()
    {
        
    }

    protected virtual void OnEnable()
    {
        control = target as OnlineMapsControlBase3D;
        CacheSerializedProperties();
    }

    public override void OnInspectorGUI()
    {
        bool dirty = false;

        serializedObject.Update();

        float oldWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 170;

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture, ref dirty);

        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        DrawPropsGUI(ref dirty);
        DrawMarkersGUI(ref dirty);

        EditorGUIUtility.labelWidth = oldWidth;

        serializedObject.ApplyModifiedProperties();

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
