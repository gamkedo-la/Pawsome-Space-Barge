using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OrbitalElements))]
public class OrbitalElementsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property,
        GUIContent label)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);

        EditorGUILayout.BeginHorizontal();

        const int spaceWidth = 10;
        var labelWidth = GUILayout.Width(15);
        var fieldWidth = GUILayout.Width(80);
        
        EditorGUILayout.LabelField("a", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("semiMajorAxis"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth, false);
        EditorGUILayout.LabelField("e", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("eccentricity"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth, false);
        EditorGUILayout.LabelField("ν", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("nu"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth, false);
        EditorGUILayout.LabelField("ω", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("omega"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth, false);
        EditorGUILayout.LabelField("T", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("T"), GUIContent.none, fieldWidth);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("rp", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("rp"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth, false);
        EditorGUILayout.LabelField("ra", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("ra"), GUIContent.none, fieldWidth);
        
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0f;
    }
}