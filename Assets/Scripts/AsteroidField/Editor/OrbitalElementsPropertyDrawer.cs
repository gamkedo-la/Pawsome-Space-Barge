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
        var fieldWidth = GUILayout.MinWidth(45);
        
        EditorGUILayout.LabelField("a", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("semiMajorAxis"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth);
        EditorGUILayout.LabelField("e", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("eccentricity"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth);
        EditorGUILayout.LabelField("ν", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("nu"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth);
        EditorGUILayout.LabelField("ω", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("omega"), GUIContent.none, fieldWidth);
        EditorGUILayout.Space(spaceWidth);
        EditorGUILayout.LabelField("T", labelWidth);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("T"), GUIContent.none, fieldWidth);
        
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndProperty();
    }
}