using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AsteroidField))]
public class AsteroidFieldEditor : Editor
{
    AsteroidField field;
    Editor outerRingEditor;
    Editor middleRingEditor;
    Editor innerRingEditor;

    public override void OnInspectorGUI()
	{
        // using (var check = new EditorGUI.ChangeCheckScope())
        // {
            base.OnInspectorGUI();
            // if (check.changed)
            // {
            //     field.GeneratePlanet();
            // }
        // }

        // if (GUILayout.Button("Generate Planet (Debug)"))
        // {
        //     field.GeneratePlanet();
        // }

        DrawSettingsEditor(field.outerRing, ref field.outerRingFoldout, ref outerRingEditor);
        DrawSettingsEditor(field.middleRing, ref field.middleRingFoldout, ref middleRingEditor);
        DrawSettingsEditor(field.innerRing, ref field.innerRingFoldout, ref innerRingEditor);
	}

    void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            // using (var check = new EditorGUI.ChangeCheckScope())
            // {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    // if (check.changed)
                    // {
                    //     if (onSettingsUpdated != null)
                    //     {
                    //         onSettingsUpdated();
                    //     }
                    // }
                }
            // }
        }
    }

	private void OnEnable()
	{
        field = (AsteroidField)target;
	}
}
