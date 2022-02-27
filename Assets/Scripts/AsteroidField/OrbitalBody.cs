using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    public OrbitalElements orbitalElements = new();
    public float boosterPower = 0f;
    public float lateralPower = 0f;
    public float timeOffset;

    private Transform centerOfMass;

    [SerializeField] private Vector3 positionEci;
    [SerializeField] private Vector3 velocityEci;

    private readonly Vector3 up = Vector3.forward;

    private bool initializedOrbit;

    public Vector3 Position => positionEci + centerOfMass.position;
    public Vector3 Velocity => velocityEci;
    public Vector3 Prograde { get; private set; }

    public Vector3 Retrograde => -Prograde;
    public Vector3 Zenith { get; private set; }

    public Vector3 Nadir => -Zenith;

    private void Start()
    {
        centerOfMass = GameObject.FindGameObjectWithTag("Planet").transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Position, Prograde * 10);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Position, Zenith * 10);
    }

    public void Recalculate(float time)
    {
        var currentTimestamp = time + timeOffset;
        if (!initializedOrbit)
        {
            initializedOrbit = true;
            // Create the initial orbital elements based on current position being in a stable, circular orbit
            orbitalElements.SetCircularOrbit(currentTimestamp, transform.position - centerOfMass.position);
        }

        UpdatePositionAndVelocityAtTime(currentTimestamp);

        if (boosterPower == 0 && lateralPower == 0) return;

        AddDeltaV(currentTimestamp, boosterPower * Prograde + lateralPower * Zenith);
        boosterPower = 0;
        lateralPower = 0;
    }

    public void AddDeltaV(float time, Vector3 deltaV)
    {
        orbitalElements.SetOrbit(time, positionEci, velocityEci + deltaV);
    }

    private void UpdatePositionAndVelocityAtTime(float time)
    {
        (positionEci, velocityEci) = orbitalElements.ToCartesian(time);
        var forwardTrajectory = Vector3.Cross(up, positionEci);

        Prograde = forwardTrajectory.normalized;
        Zenith = positionEci.normalized;
    }

    public Vector3 GetAdjustedWorldPositionAtTime(float time)
    {
        var (pos, _) = orbitalElements.ToCartesian(time);
        return pos + centerOfMass.position;
    }
}