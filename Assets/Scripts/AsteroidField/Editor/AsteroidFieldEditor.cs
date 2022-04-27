using UnityEngine;
using UnityEditor;


/// <summary>
/// Enables display and editing of Asteroid Prefab Factories from AsteroidField panel.
/// </summary>
[CustomEditor(typeof(AsteroidField))]
public class AsteroidFieldEditor : Editor
{
    AsteroidField field;
    Editor outerRingEditor;
    Editor middleRingEditor;
    Editor innerRingEditor;

    public override void OnInspectorGUI()
	{
        base.OnInspectorGUI();

        DrawSettingsEditor(field.outerRing, ref field.outerRingFoldout, ref outerRingEditor);
        DrawSettingsEditor(field.middleRing, ref field.middleRingFoldout, ref middleRingEditor);
        DrawSettingsEditor(field.innerRing, ref field.innerRingFoldout, ref innerRingEditor);
	}

    void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            if (foldout)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }

	private void OnEnable()
	{
        field = (AsteroidField)target;
	}
}
