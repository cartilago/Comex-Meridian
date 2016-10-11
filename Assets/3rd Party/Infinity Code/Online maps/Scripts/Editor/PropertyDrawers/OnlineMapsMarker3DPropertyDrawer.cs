/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
#define UNITY_5_0P
#endif

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OnlineMapsMarker3D))]
public class OnlineMapsMarker3DPropertyDrawer : PropertyDrawer
{
    public static bool isRemoved = false;
    public static bool? isEnabledChanged;

    private const int countFields = 7;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        try
        {
            SerializedProperty pEnabled = property.FindPropertyRelative("_enabled");
            EditorGUI.BeginChangeCheck();
            bool newEnabled = EditorGUI.ToggleLeft(new Rect(position.x, position.y, position.width, 16), label, pEnabled.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (Application.isPlaying) isEnabledChanged = newEnabled;
                else pEnabled.boolValue = newEnabled;
            }

            Rect rect = new Rect(position.x, position.y, position.width, 16);

            EditorGUI.BeginChangeCheck();
            SerializedProperty pLat = DrawProperty(property, "latitude", ref rect);
            if (EditorGUI.EndChangeCheck())
            {
#if UNITY_5_0P
                if (pLat.doubleValue < -90) pLat.doubleValue = -90;
                else if (pLat.doubleValue > 90) pLat.doubleValue = 90;
#else
                if (pLat.floatValue < -90) pLat.floatValue = -90;
                else if (pLat.floatValue > 90) pLat.floatValue = 90;
#endif
            }

            EditorGUI.BeginChangeCheck();
            SerializedProperty pLng = DrawProperty(property, "longitude", ref rect);
            if (EditorGUI.EndChangeCheck())
            {
#if UNITY_5_0P
                if (pLng.doubleValue < -180) pLng.doubleValue += 360;
                else if (pLng.doubleValue > 180) pLng.doubleValue -= 360;
#else
                if (pLng.floatValue < -180) pLng.floatValue += 360;
                else if (pLng.floatValue > 180) pLng.floatValue -= 360;
#endif
            }

            DrawProperty(property, "range", ref rect, new GUIContent("Zooms"));

            DrawProperty(property, "_scale", ref rect);
            DrawProperty(property, "label", ref rect);
            DrawProperty(property, "prefab", ref rect);

            rect.y += 18;
            if (GUI.Button(rect, "Remove")) isRemoved = true;
        }
        catch
        {
        }


        EditorGUI.EndProperty();
    }

    private SerializedProperty DrawProperty(SerializedProperty property, string name, ref Rect rect, GUIContent label = null)
    {
        rect.y += 18;
        SerializedProperty prop = property.FindPropertyRelative(name);
        EditorGUI.PropertyField(rect, prop, label);
        return prop;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (countFields + 1) * 18;
    }
}