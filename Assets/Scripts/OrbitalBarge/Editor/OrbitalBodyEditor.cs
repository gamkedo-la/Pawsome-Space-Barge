using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitalBody))]
public class OrbitalBodyEditor : Editor
{
    private float deltaV = 0.1f;
    private OrbitalBody body;

    private void OnEnable()
    {
        body = (OrbitalBody) target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        GUILayout.Label("Orbital Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitalElements"));

        EditorGUILayout.Separator();

        
        GUILayout.Label("Spaceship control", EditorStyles.boldLabel);

        deltaV = EditorGUILayout.FloatField(new GUIContent("Δv [m/s]"), deltaV);
        
        EditorGUI.BeginChangeCheck();
        
        // var body = (OrbitalBody) target;

        EditorGUILayout.BeginHorizontal();

        var time = Application.isPlaying ? Time.fixedTime : 0;
        if (GUILayout.Button("- [Retrograde]"))
        {
            body.AddDeltaV(time, deltaV * body.Retrograde);
            body.Recalculate(time);
            ApplyChange(body);
        }

        if (GUILayout.Button("+ [Prograde]"))
        {
            body.AddDeltaV(time, deltaV * body.Prograde);
            body.Recalculate(time);
            ApplyChange(body);
        }

        if (GUILayout.Button("- [Nadir]"))
        {
            body.AddDeltaV(time, deltaV * body.Nadir);
            body.Recalculate(time);
            ApplyChange(body);
        }

        if (GUILayout.Button("+ [Zenith]"))
        {
            body.AddDeltaV(time, deltaV * body.Zenith);
            body.Recalculate(time);
            ApplyChange(body);
        }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        
        GUILayout.Label("Orbit control", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Circular Orbit"))
        {
            body.InitializeOrbit(time);
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
        // orbitalElements.FindPropertyRelative("rp").floatValue = body.OrbitalElements.rp;
        // orbitalElements.FindPropertyRelative("ra").floatValue = body.OrbitalElements.ra;

        serializedObject.FindProperty("positionPci").vector3Value = body.PositionPci;
        serializedObject.FindProperty("velocityPci").vector3Value = body.Velocity;
    }

    private void OnSceneGUI()
    {
        // var body = (OrbitalBody) target;
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