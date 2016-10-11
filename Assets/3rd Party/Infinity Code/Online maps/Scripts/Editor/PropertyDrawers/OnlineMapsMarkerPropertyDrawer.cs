/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
#define UNITY_5_0P
#endif

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OnlineMapsMarker))]
public class OnlineMapsMarkerPropertyDrawer : PropertyDrawer
{
    public static bool isRemoved = false;

    private const int countFields = 9;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.LabelField(position, label);

        try
        {
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

            EditorGUI.BeginChangeCheck();
            SerializedProperty pRot = DrawProperty(property, "_rotation", ref rect, new GUIContent("Rotation (0-1)"));
            if (EditorGUI.EndChangeCheck()) if (pRot.floatValue < 0 || pRot.floatValue > 1) pRot.floatValue = Mathf.Repeat(pRot.floatValue, 1);

            DrawProperty(property, "_scale", ref rect);
            DrawProperty(property, "label", ref rect);
            DrawProperty(property, "align", ref rect);

            EditorGUI.BeginChangeCheck();
            SerializedProperty pTexture = DrawProperty(property, "texture", ref rect);
            if (EditorGUI.EndChangeCheck()) OnlineMapsEditor.CheckMarkerTextureImporter(pTexture);

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