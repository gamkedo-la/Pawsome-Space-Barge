using UnityEditor;
using UnityEngine;

namespace AsteroidField.Editor
{
    [CustomPropertyDrawer(typeof(RandomPrefabEntry))]
    public class RandomPrefabEntryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, null, property);

            var prefabRect = new Rect(position.x, position.y, position.width / 2 - 2, position.height);
            var weightRect = new Rect(position.x + position.width / 2 + 2, position.y, position.width / 2 - 2,
                position.height);
            EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("prefab"), GUIContent.none);
            EditorGUI.IntSlider(weightRect, property.FindPropertyRelative("weight"), 1, 100, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}