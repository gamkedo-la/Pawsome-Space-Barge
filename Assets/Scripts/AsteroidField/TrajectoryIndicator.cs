using UnityEngine;

public class TrajectoryIndicator : MonoBehaviour
{
    public OrbitalBody body;
    public LineRenderer lineRenderer;

    public float stepDeltaT = 1f;
    [Range(2, 200)]
    public int steps = 10;

    private void FixedUpdate()
    {
        if (lineRenderer.positionCount != steps)
        {
            lineRenderer.positionCount = steps;
        }

        for (var i = 0; i < steps; i++)
        {
            var point = body.GetAdjustedWorldPositionAtTime(Time.time + i * stepDeltaT);
            lineRenderer.SetPosition(i, transform.InverseTransformPoint(point));
        }
    }
}