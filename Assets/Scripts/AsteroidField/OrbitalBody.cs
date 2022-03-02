using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    [Header("Limits")] 
    [SerializeField] private float minimumOrbitalRadius = 2000f;
    [SerializeField] private float maximumOrbitalRadius = 7000f;
    
    [Header("Planet Centric Inertial Coordinates")]
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private Vector3 positionPci;
    [SerializeField] private Vector3 velocityPci;
    
    [Header("Orbit fast track")] 
    [SerializeField] private float timeOffset;

    [SerializeField] [HideInInspector] private OrbitalElements orbitalElements = new();

    private bool orbitCached;
    private Vector3[] orbitCache;

    public Vector3 PositionPci => positionPci;
    public Vector3 Position => positionPci + centerOfMass.position;
    public Vector3 Velocity => velocityPci;
    public Vector3 Prograde { get; private set; }

    public Vector3 Retrograde => -Prograde;
    public Vector3 Zenith { get; private set; }

    public Vector3 Nadir => -Zenith;

    public float ProgradeRotation
    {
        get
        {
            var a = Vector3.Angle(Vector3.right, Prograde);
            if (Prograde.y < 0)
            {
                a = 360f - a;
            }

            return a;
        }
    }

    public OrbitalElements OrbitalElements => orbitalElements;

    private void Awake()
    {
        if (centerOfMass == null)
        {
            centerOfMass = GameObject.FindGameObjectWithTag("Planet").transform;
        }
    }

    public void InitializeOrbit(float time)
    {
        orbitalElements.SetCircularOrbit(time + timeOffset, transform.position - centerOfMass.position);
        UpdatePositionAndVelocityAtTime(0);
    }

    public void Recalculate(float time)
    {
        UpdatePositionAndVelocityAtTime(time + timeOffset);
    }

    public void AddDeltaV(float time, Vector3 deltaV)
    {
        var oldOrbitalElements = orbitalElements;
        orbitalElements.SetOrbit(time + timeOffset, positionPci, velocityPci + deltaV);
        if (orbitalElements.ra > maximumOrbitalRadius || orbitalElements.rp < minimumOrbitalRadius)
        {
            orbitalElements = oldOrbitalElements;
            Debug.LogWarning("Orbit radius limits exceeded - delta-v change ignored.");
            return;
        }
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
        if (centerOfMass == null)
        {
            return null;
        }

        if (numberOfPoints == 0)
        {
            var currentLength = orbitCache?.Length ?? 0;
            numberOfPoints = currentLength > 1 ? currentLength : 100;
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