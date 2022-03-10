using UnityEngine;

public class TrajectoryIndicator : MonoBehaviour
{
    public OrbitalBody body;
    public LineRenderer lineRenderer;

    [Range(2, 200)]
    public int steps = 10;

    private void FixedUpdate()
    {
        if (lineRenderer.positionCount != steps)
        {
            lineRenderer.positionCount = steps;
        }

        var points = body.GetOrbitWorldPositions(steps);

        for (var i = 0; i < steps; i++)
        {
            var point = points[i];
            lineRenderer.SetPosition(i, transform.InverseTransformPoint(point));
        }
    }
}