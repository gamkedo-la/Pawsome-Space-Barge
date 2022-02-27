using UnityEngine;

public class OrbitalBody : MonoBehaviour
{

    [Header("Planet Centric Inertial coordinates")]
    [SerializeField] private Vector3 positionPci;
    [SerializeField] private Vector3 velocityPci;
    
    [Header("Orbital Elements")]
    [SerializeField] private OrbitalElements orbitalElements = new();
    
    [Header("Orbit fast track")]
    public float timeOffset;

    private Transform centerOfMass;

    private readonly Vector3 up = Vector3.forward;

    private bool initializedOrbit;
    private bool orbitCached;
    private Vector3[] orbitCache;

    public Vector3 Position => positionPci + centerOfMass.position;
    public Vector3 Velocity => velocityPci;
    public Vector3 Prograde { get; private set; }

    public Vector3 Retrograde => -Prograde;
    public Vector3 Zenith { get; private set; }

    public Vector3 Nadir => -Zenith;

    public float ProgradeRotation => Vector3.Angle(Vector3.right, Prograde);

    private void Start()
    {
        centerOfMass = GameObject.FindGameObjectWithTag("Planet").transform;
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
    }

    public void AddDeltaV(float time, Vector3 deltaV)
    {
        orbitalElements.SetOrbit(time + timeOffset, positionPci, velocityPci + deltaV);
        orbitCached = false;
    }

    private void UpdatePositionAndVelocityAtTime(float time)
    {
        (positionPci, velocityPci) = orbitalElements.ToCartesian(time);

        Prograde = velocityPci.normalized;
        Zenith = positionPci.normalized;
    }

    public Vector3[] GetOrbitWorldPositions(int numberOfPoints)
    {
        if (!Application.isPlaying)
        {
            return null;
        }
        
        if (numberOfPoints == 0)
        {
            numberOfPoints = orbitCache?.Length ?? 100;
        }

        if (orbitCache == null || orbitCache.Length != numberOfPoints)
        {
            orbitCache = new Vector3[numberOfPoints];
            orbitCached = false;
        }

        if (!orbitCached)
        {
            orbitalElements.GetOrbitCoordinates(orbitCache);
            var centerOfMassPosition = centerOfMass.position;
            for (var i = 0; i < numberOfPoints; i++)
            {
                orbitCache[i] += centerOfMassPosition;
            }
        }

        return orbitCache;
    }
}