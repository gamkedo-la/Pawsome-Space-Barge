using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer: PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var previousState = GUI.enabled;

        GUI.enabled = false;

        EditorGUI.PropertyField(position, property, label);

        GUI.enabled = previousState;
    }
}
