using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitalBody))]
public class OrbitalBodyEditor : Editor
{
    private float deltaV = 0.1f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Label("Planet Centric Inertial Coordinates", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("centerOfMass"), new GUIContent("Planet"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionPci"), new GUIContent("Position"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("velocityPci"), new GUIContent("Velocity"));

        EditorGUILayout.Separator();

        GUILayout.Label("Orbital Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitalElements"));

        EditorGUILayout.Separator();

        
        GUILayout.Label("Spaceship control", EditorStyles.boldLabel);

        deltaV = EditorGUILayout.FloatField(new GUIContent("Δv [m/s]"), deltaV);
        
        EditorGUI.BeginChangeCheck();
        
        var body = (OrbitalBody) target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("- [Retrograde]"))
        {
            body.AddDeltaV(0, deltaV * body.Retrograde);
            body.Recalculate(0);
            ApplyChange(body);
        }

        if (GUILayout.Button("+ [Prograde]"))
        {
            body.AddDeltaV(0, deltaV * body.Prograde);
            body.Recalculate(0);
            ApplyChange(body);
        }

        if (GUILayout.Button("- [Nadir]"))
        {
            body.AddDeltaV(0, deltaV * body.Nadir);
            body.Recalculate(0);
            ApplyChange(body);
        }

        if (GUILayout.Button("+ [Zenith]"))
        {
            body.AddDeltaV(0, deltaV * body.Zenith);
            body.Recalculate(0);
            ApplyChange(body);
        }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        
        GUILayout.Label("Orbit control", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Circular Orbit"))
        {
            body.InitializeOrbit(0);
            ApplyChange(body);
        }
        
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    private void ApplyChange(OrbitalBody body)
    {
        var orbitalElements = serializedObject.FindProperty("orbitalElements");
        orbitalElements.FindPropertyRelative("semiMajorAxis").floatValue = body.OrbitalElements.semiMajorAxis;
        orbitalElements.FindPropertyRelative("eccentricity").floatValue = body.OrbitalElements.eccentricity;
        orbitalElements.FindPropertyRelative("nu").floatValue = body.OrbitalElements.nu;
        orbitalElements.FindPropertyRelative("T").floatValue = body.OrbitalElements.T;
        orbitalElements.FindPropertyRelative("omega").floatValue = body.OrbitalElements.omega;

        serializedObject.FindProperty("positionPci").vector3Value = body.PositionPci;
        serializedObject.FindProperty("velocityPci").vector3Value = body.Velocity;
    }

    private void OnSceneGUI()
    {
        var body = (OrbitalBody) target;
        var points = body.GetOrbitWorldPositions(0);
        if (points == null)
        {
            return;
        }

        Handles.color = Color.white;

        Handles.DrawPolyLine(points);
        Handles.DrawLine(points[^1], points[0]);
    }
}