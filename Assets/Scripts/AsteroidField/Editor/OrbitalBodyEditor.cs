using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitalBody))]
public class OrbitalBodyEditor : Editor
{
    private const float LineWidth = 2;

    private float deltaV = 1;
    
    public override void OnInspectorGUI()
    {
        var body = (OrbitalBody) target;
        
        base.OnInspectorGUI();

        EditorGUILayout.Separator();
    
        GUILayout.Label("Spaceship control");
        
        deltaV = EditorGUILayout.FloatField(new GUIContent("Δv [m/s]"), deltaV);
        
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("- [Retrograde]"))
        {
            body.AddDeltaV(Time.fixedTime, deltaV * body.Retrograde);
        }
        
        if (GUILayout.Button("+ [Prograde]"))
        {
            body.AddDeltaV(Time.fixedTime, deltaV * body.Prograde);
        }
        
        if (GUILayout.Button("- [Nadir]"))
        {
            body.AddDeltaV(Time.fixedTime, deltaV * body.Nadir);
        }
        
        if (GUILayout.Button("+ [Zenith]"))
        {
            body.AddDeltaV(Time.fixedTime, deltaV * body.Zenith);
        }
        
        EditorGUILayout.EndHorizontal();
        
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