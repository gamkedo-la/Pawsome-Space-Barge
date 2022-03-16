using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitalBody))]
public class OrbitalBodyEditor : Editor
{
    private float deltaV = 0.1f;
    private float deltaT = 60;

    public override void OnInspectorGUI()
    {
        var body = (OrbitalBody) target;
        var time = Application.isPlaying ? Time.fixedTime : 0;
        
        serializedObject.Update();

        if (serializedObject.FindProperty("positionPci").vector3Value != body.transform.position)
        {
            serializedObject.FindProperty("positionPci").vector3Value = body.transform.position;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            body.SetOrbit(time);
        }
        
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();
        
        if (EditorGUI.EndChangeCheck())
        {
            body.SetOrbit(time);
        }

        EditorGUILayout.Separator();

        GUILayout.Label("Orbital Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitalElements"));

        EditorGUILayout.Separator();
        
        GUILayout.Label("Spaceship control", EditorStyles.boldLabel);

        deltaV = EditorGUILayout.FloatField(new GUIContent("Δv [m/s]"), deltaV);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("- [Retrograde]"))
        {
            AddDeltaV(deltaV * body.Retrograde);
        }

        if (GUILayout.Button("+ [Prograde]"))
        {
            AddDeltaV(deltaV * body.Prograde);
        }

        if (GUILayout.Button("- [Nadir]"))
        {
            AddDeltaV(deltaV * body.Nadir);
        }

        if (GUILayout.Button("+ [Zenith]"))
        {
            AddDeltaV(deltaV * body.Zenith);
        }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();

        deltaT = EditorGUILayout.FloatField(new GUIContent("Δt [s]"), deltaT);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button($"Rewind {deltaT} seconds"))
        {
            SetPositionAtTime(time-deltaT);
        }

        if (GUILayout.Button($"Forward {deltaT} seconds"))
        {
            SetPositionAtTime(time+deltaT);
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Label("Orbit control", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Circular Orbit"))
        {
            var position = serializedObject.FindProperty("positionPci").vector3Value;
            var mu = body.OrbitalElements.Mu;
            var velocity = Vector3.Cross(position, Vector3.forward).normalized * Mathf.Sqrt(mu / position.magnitude);
            serializedObject.FindProperty("velocityPci").vector3Value = velocity;
        }
        
        EditorGUILayout.EndHorizontal();
        if (serializedObject.ApplyModifiedProperties())
        {
            body.SetOrbit(time);
        }
    }

    private void AddDeltaV(Vector3 dV)
    {
        serializedObject.FindProperty("velocityPci").vector3Value += dV;
    }

    private void SetPositionAtTime(float t)
    {
        var body = (OrbitalBody) target;
        var (pos, vel) = body.OrbitalElements.ToCartesian(t);
        serializedObject.FindProperty("positionPci").vector3Value = pos;
        serializedObject.FindProperty("velocityPci").vector3Value = vel;
        body.transform.position = pos;
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