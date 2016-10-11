/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnlineMapsLocationService))]
public class OnlineMapsLocationServiceEditor:Editor
{
    private static GUIStyle _toggleStyle;
    private bool showCreateMarker = true;
    private bool showGPSEmulator = true;
    private bool showUpdatePosition = true;
    private SerializedProperty pCreateMarkerInUserPosition;
    private SerializedProperty pMarkerType;
    private SerializedProperty pMarker3DPrefab;
    private SerializedProperty pMarker2DTexture;
    private SerializedProperty pMarker2DAlign;
    private SerializedProperty pMarkerTooltip;
    private SerializedProperty pUseCompassForMarker;
    private SerializedProperty pUseGPSEmulator;
    private SerializedProperty pEmulatorPosition;
    private SerializedProperty pEmulatorCompass;
    private SerializedProperty pDesiredAccuracy;
    private SerializedProperty pCompassThreshold;
    private SerializedProperty pFindLocationByIP;
    private SerializedProperty pUpdatePosition;
    private SerializedProperty pUpdateDistance;
    private SerializedProperty pAutoStopUpdateOnInput;
    private SerializedProperty pRestoreAfter;

    private static GUIStyle toggleStyle
    {
        get
        {
            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(GUI.skin.toggle);
                _toggleStyle.margin.top = 0;
            }
            return _toggleStyle;
        }
    }

    private void CacheSerializedProperties()
    {
        pCreateMarkerInUserPosition = serializedObject.FindProperty("createMarkerInUserPosition");
        pMarkerType = serializedObject.FindProperty("markerType");
        pMarker3DPrefab = serializedObject.FindProperty("marker3DPrefab");
        pMarker2DTexture = serializedObject.FindProperty("marker2DTexture");
        pMarker2DAlign = serializedObject.FindProperty("marker2DAlign");
        pMarkerTooltip = serializedObject.FindProperty("markerTooltip");
        pUseCompassForMarker = serializedObject.FindProperty("useCompassForMarker");
        pUseGPSEmulator = serializedObject.FindProperty("useGPSEmulator");
        pEmulatorPosition = serializedObject.FindProperty("emulatorPosition");
        pEmulatorCompass = serializedObject.FindProperty("emulatorCompass");
        pDesiredAccuracy = serializedObject.FindProperty("desiredAccuracy");
        pCompassThreshold = serializedObject.FindProperty("compassThreshold");
        pFindLocationByIP = serializedObject.FindProperty("findLocationByIP");
        pUpdatePosition = serializedObject.FindProperty("updatePosition");
        pUpdateDistance = serializedObject.FindProperty("updateDistance");
        pAutoStopUpdateOnInput = serializedObject.FindProperty("autoStopUpdateOnInput");
        pRestoreAfter = serializedObject.FindProperty("restoreAfter");
    }

    private void OnCreateMarkerGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        bool createMarker = pCreateMarkerInUserPosition.boolValue;
        if (createMarker)
        {
            EditorGUILayout.BeginHorizontal();
            showCreateMarker = GUILayout.Toggle(showCreateMarker, "", EditorStyles.foldout, GUILayout.ExpandWidth(false), GUILayout.Height(16));
        }

        pCreateMarkerInUserPosition.boolValue = GUILayout.Toggle(pCreateMarkerInUserPosition.boolValue, "Create Marker", toggleStyle);

        if (createMarker) EditorGUILayout.EndHorizontal();

        if (pCreateMarkerInUserPosition.boolValue && showCreateMarker)
        {
            pMarkerType.enumValueIndex = EditorGUILayout.Popup("Type", pMarkerType.enumValueIndex, new[] {"2D", "3D"});

            if (pMarkerType.enumValueIndex == (int)OnlineMapsLocationServiceMarkerType.threeD) EditorGUILayout.PropertyField(pMarker3DPrefab, new GUIContent("Prefab"));
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMarker2DTexture, new GUIContent("Texture"));
                if (EditorGUI.EndChangeCheck() && pMarker2DTexture.objectReferenceValue != null) OnlineMapsEditor.CheckMarkerTextureImporter(pMarker2DTexture);
                EditorGUILayout.PropertyField(pMarker2DAlign, new GUIContent("Align"));
            }

            EditorGUILayout.PropertyField(pMarkerTooltip, new GUIContent("Tooltip"));
            EditorGUILayout.PropertyField(pUseCompassForMarker, new GUIContent("Use Compass"));
        }

        EditorGUILayout.EndVertical();
    }

    private void OnEnable()
    {
        CacheSerializedProperties();
    }

    private void OnGPSEmulatorGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        bool useGPSEmulator = pUseGPSEmulator.boolValue;
        if (useGPSEmulator)
        {
            EditorGUILayout.BeginHorizontal();
            showGPSEmulator = GUILayout.Toggle(showGPSEmulator, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));
        }

        pUseGPSEmulator.boolValue = GUILayout.Toggle(pUseGPSEmulator.boolValue, "Use GPS Emulator", toggleStyle);

        if (useGPSEmulator) EditorGUILayout.EndHorizontal();

        if (pUseGPSEmulator.boolValue && showGPSEmulator)
        {
            EditorGUILayout.HelpBox("The emulator is automatically disabled on the devices and use the data from the sensors.", MessageType.Info);

            if (GUILayout.Button("Copy position from Online Maps"))
            {
                OnlineMaps api = (target as OnlineMapsLocationService).GetComponent<OnlineMaps>();
                if (api != null) pEmulatorPosition.vector2Value = api.position;
            }

            EditorGUILayout.PropertyField(pEmulatorPosition, new GUIContent("Position (Lng/Lat)"));
            EditorGUILayout.PropertyField(pEmulatorCompass, new GUIContent("Compass (0-360)"));
        }

        EditorGUILayout.EndVertical();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 160;

        EditorGUILayout.PropertyField(pDesiredAccuracy, new GUIContent("Desired Accuracy (meters)"));
        EditorGUIUtility.labelWidth = labelWidth;

        EditorGUILayout.PropertyField(pCompassThreshold);
        EditorGUILayout.PropertyField(pFindLocationByIP);

        OnUpdatePositionGUI();
        OnCreateMarkerGUI();
        OnGPSEmulatorGUI();

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);
    }

    private void OnUpdatePositionGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        
        bool updatePosition = pUpdatePosition.boolValue;
        if (updatePosition)
        {
            EditorGUILayout.BeginHorizontal();
            showUpdatePosition = GUILayout.Toggle(showUpdatePosition, "", EditorStyles.foldout, GUILayout.ExpandWidth(false), GUILayout.Height(16));
        }

        pUpdatePosition.boolValue = GUILayout.Toggle(pUpdatePosition.boolValue, "Update Map Position", toggleStyle);

        if (updatePosition) EditorGUILayout.EndHorizontal();

        if (pUpdatePosition.boolValue && showUpdatePosition)
        {
            EditorGUILayout.PropertyField(pUpdateDistance);
            EditorGUILayout.PropertyField(pAutoStopUpdateOnInput, new GUIContent("Auto Stop On Input"));

            bool restoreAfter = pRestoreAfter.intValue != 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            restoreAfter = GUILayout.Toggle(restoreAfter, "", GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck()) pRestoreAfter.intValue = restoreAfter ? 10 : 0;
            EditorGUI.BeginDisabledGroup(!restoreAfter);
            EditorGUILayout.PropertyField(pRestoreAfter, new GUIContent("Restore After (sec)"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}