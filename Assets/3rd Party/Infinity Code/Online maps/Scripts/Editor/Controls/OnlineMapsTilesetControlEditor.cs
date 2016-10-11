/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_5_3P
#endif

using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_5_3P
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

[CustomEditor(typeof (OnlineMapsTileSetControl))]
public class OnlineMapsTilesetControlEditor : OnlineMapsTextureControlEditor
{
    private bool showShaders;
    private Shader defaultTilesetShader;
    private SerializedProperty pCheckMarker2DVisibility;
    private SerializedProperty pTileMaterial;
    private SerializedProperty pMarkerMaterial;
    private SerializedProperty pTilesetShader;
    private SerializedProperty pMarkerShader;
    private SerializedProperty pDrawingShader;
    private SerializedProperty pSmoothZoom;
    private SerializedProperty pColliderType;
    private SerializedProperty pUseElevation;
    private SerializedProperty pElevationScale;
    private SerializedProperty pElevationZoomRange;
    private SerializedProperty pBingAPI;

    protected override void CacheSerializedProperties()
    {
        base.CacheSerializedProperties();

        pCheckMarker2DVisibility = serializedObject.FindProperty("checkMarker2DVisibility");
        pTileMaterial = serializedObject.FindProperty("tileMaterial");
        pMarkerMaterial = serializedObject.FindProperty("markerMaterial");
        pTilesetShader = serializedObject.FindProperty("tilesetShader");
        pMarkerShader = serializedObject.FindProperty("markerShader");
        pDrawingShader = serializedObject.FindProperty("drawingShader");
        pSmoothZoom = serializedObject.FindProperty("smoothZoom");
        pColliderType = serializedObject.FindProperty("colliderType");
        pUseElevation = serializedObject.FindProperty("useElevation");
        pElevationScale = serializedObject.FindProperty("elevationScale");
        pElevationZoomRange = serializedObject.FindProperty("elevationZoomRange");
        pBingAPI = serializedObject.FindProperty("bingAPI");
    }

    private void CheckCameraDistance(OnlineMaps api)
    {
        if (api == null) return;

        Camera tsCamera = pActiveCamera.objectReferenceValue != null ? pActiveCamera.objectReferenceValue as Camera : Camera.main;

        if (tsCamera == null) return;

        Vector3 mapCenter = api.transform.position + new Vector3(api.tilesetSize.x / -2, 0, api.tilesetSize.y / 2);
        float distance = (tsCamera.transform.position - mapCenter).magnitude * 1.5f;
        if (distance <= tsCamera.farClipPlane) return;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.HelpBox("Potential problem detected:\n\"Camera - Clipping Planes - Far\" is too small.", MessageType.Warning);

        if (GUILayout.Button("Fix Clipping Planes - Far")) tsCamera.farClipPlane = distance;

        EditorGUILayout.EndVertical();
    }

    private void DrawElevationGUI(ref bool dirty)
    {
        bool useElevation = pUseElevation.boolValue;

        if (useElevation) EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(pUseElevation);
        if (EditorGUI.EndChangeCheck())
        {
            dirty = true;
            if (EditorApplication.isPlaying) control.UpdateControl();
        }

        if (pUseElevation.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pElevationScale);
            if (EditorGUI.EndChangeCheck())
            {
                dirty = true;
                if (EditorApplication.isPlaying) OnlineMaps.instance.Redraw();
            }

            EditorGUILayout.PropertyField(pElevationZoomRange, new GUIContent("Zoom"));
            EditorGUILayout.PropertyField(pBingAPI, new GUIContent("Bing Maps API key"));
            EditorGUILayout.HelpBox("Public Windows App or Public Windows Phone App have the 50.000 transaction within 24 hours. With the other chooses there's only 125.000 transactions within a year and the key will expire if exceeding it.", MessageType.Info);

            if (GUILayout.Button("Create Bing Maps API Key")) Process.Start("https://msdn.microsoft.com/en-us/library/ff428642.aspx");
        }

        if (useElevation) EditorGUILayout.EndVertical();
    }

    public override void DrawMarker2DPropsGUI()
    {
        base.DrawMarker2DPropsGUI();

        if (pMarker2DMode.enumValueIndex == (int)OnlineMapsMarker2DMode.flat) EditorGUILayout.PropertyField(pCheckMarker2DVisibility);
    }

    private void DrawMaterialsAndShaders()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        showShaders = GUILayout.Toggle(showShaders, "Materials & Shaders", EditorStyles.foldout);

        if (showShaders)
        {
            EditorGUILayout.PropertyField(pTileMaterial);
            EditorGUILayout.PropertyField(pMarkerMaterial);
            EditorGUILayout.PropertyField(pTilesetShader);
            EditorGUILayout.PropertyField(pMarkerShader);
            EditorGUILayout.PropertyField(pDrawingShader);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMoveCameraGUI(OnlineMaps api)
    {
        if (!GUILayout.Button("Move camera to center of Tileset")) return;
        if (api == null) return;

        Camera tsCamera = pActiveCamera.objectReferenceValue != null ? pActiveCamera.objectReferenceValue as Camera : Camera.main;

        if (tsCamera == null)
        {
            Debug.Log("Camera is null");
            return;
        }

        GameObject go = tsCamera.gameObject;
        float minSide = Mathf.Min(api.tilesetSize.x * api.transform.lossyScale.x, api.tilesetSize.y * api.transform.lossyScale.z);
        Vector3 position = api.transform.position + api.transform.rotation * new Vector3(api.tilesetSize.x / -2 * api.transform.lossyScale.x, minSide, api.tilesetSize.y / 2 * api.transform.lossyScale.z);
        go.transform.position = position;
        go.transform.rotation = api.transform.rotation * Quaternion.Euler(90, 180, 0);
    }

    protected override void DrawZoomLate()
    {
        base.DrawZoomLate();

        EditorGUILayout.PropertyField(pSmoothZoom);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        control = target as OnlineMapsControlBase3D;
        defaultTilesetShader = Shader.Find("Infinity Code/Online Maps/Tileset");
    }

    public override void OnInspectorGUI()
    {
        bool dirty = false;

        serializedObject.Update();

        float oldWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 170;

        OnlineMapsControlBaseEditor.CheckMultipleInstances(control, ref dirty);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.tileset, ref dirty);

        if (pTilesetShader.objectReferenceValue == null) pTilesetShader.objectReferenceValue = defaultTilesetShader;
        if (pMarkerShader.objectReferenceValue == null) pMarkerShader.objectReferenceValue = Shader.Find("Transparent/Diffuse");
        if (pDrawingShader.objectReferenceValue == null) pDrawingShader.objectReferenceValue = Shader.Find("Infinity Code/Online Maps/Tileset DrawingElement");

        if (!EditorApplication.isPlaying) CheckCameraDistance(api);

        DrawPropsGUI(ref dirty);

        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

        EditorGUILayout.PropertyField(pColliderType);

        if (!EditorApplication.isPlaying && pColliderType.enumValueIndex == (int)OnlineMapsTileSetControl.OnlineMapsColliderType.box && pUseElevation.boolValue)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.HelpBox("Potential problem detected:\nWhen using BoxCollider, can be a problem in interaction with a map with elevation.", MessageType.Warning);
            if (GUILayout.Button("Set Collider Type - Mesh")) pColliderType.enumValueIndex = (int)OnlineMapsTileSetControl.OnlineMapsColliderType.mesh;

            EditorGUILayout.EndVertical();
        }
        
        EditorGUI.EndDisabledGroup();

        DrawMarkersGUI(ref dirty);
        DrawMaterialsAndShaders();
        DrawElevationGUI(ref dirty);
        DrawMoveCameraGUI(api);

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

    private void OnSceneGUI()
    {
        if (Application.isPlaying) return;

        OnlineMaps api = control.GetComponent<OnlineMaps>();
        if (api == null) return;
        Quaternion rotation = api.transform.rotation;
        Vector3[] points = new Vector3[5];
        points[0] = points[4] = api.transform.position;
        points[1] = points[0] + rotation * new Vector3(-api.tilesetSize.x, 0, 0);
        points[2] = points[1] + rotation * new Vector3(0, 0, api.tilesetSize.y);
        points[3] = points[0] + rotation * new Vector3(0, 0, api.tilesetSize.y);
        Handles.DrawSolidRectangleWithOutline(points, new Color(1, 1, 1, 0.3f), Color.black);

        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = {textColor = Color.black}
        };

        Handles.Label(points[0] + rotation * new Vector3(api.tilesetSize.x / -2, 0, api.tilesetSize.y / 2), "Tileset Map", style);
    }
}