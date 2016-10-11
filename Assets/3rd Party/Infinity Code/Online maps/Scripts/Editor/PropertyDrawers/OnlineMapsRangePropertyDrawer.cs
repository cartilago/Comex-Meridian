/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OnlineMapsRange))]
public class OnlineMapsRangePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pMin = property.FindPropertyRelative("min");
        SerializedProperty pMax = property.FindPropertyRelative("max");
        SerializedProperty pMinLimit = property.FindPropertyRelative("minLimit");
        SerializedProperty pMaxLimit = property.FindPropertyRelative("maxLimit");

        label.text = string.Format("{0} ({1}-{2})", label.text, pMin.intValue, pMax.intValue);
        position = EditorGUI.PrefixLabel(position, label);

        float min = pMin.intValue;
        float max = pMax.intValue;

        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(position, ref min, ref max, pMinLimit.intValue, pMaxLimit.intValue);
        if (EditorGUI.EndChangeCheck())
        {
            if (min > max) min = max;
            pMin.intValue = Mathf.RoundToInt(min);
            pMax.intValue = Mathf.RoundToInt(max);
        }
    }
}