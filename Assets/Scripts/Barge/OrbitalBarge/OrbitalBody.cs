using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    [Header("Limits")] 
    [SerializeField] private float minimumOrbitalRadius = 2000f;
    [SerializeField] private float maximumOrbitalRadius = 7000f;
    public float MaxOrbitRadius => maximumOrbitalRadius;

    [Header("Planet Centric Inertial Coordinates")]
    [SerializeField] private Vector3 positionPci;
    [SerializeField] private Vector3 velocityPci;
    
    [SerializeField] [HideInInspector] private OrbitalElements orbitalElements = new();

    private bool orbitCached;
    private Vector3[] orbitCache;

    public Vector3 Position => positionPci;
    public Vector3 Velocity => velocityPci;
    public Vector3 Prograde => velocityPci.normalized;

    public Vector3 Retrograde => -Prograde;
    public Vector3 Zenith => positionPci.normalized;

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
    public Vector3 GravitationalForce => OrbitalElements.Mu / positionPci.sqrMagnitude * Nadir;

    private void Start()
    {
        // hack for FollowOrbit mode, doesn't solve problem, not needed for UseForces
        if (GetComponent<OrbitalRigidbody>().Method == UpdateMethod.FollowOrbit)
        {
            AddDeltaV(Time.fixedTime, velocityPci.normalized * 0.1f);
        }
    }

    public void SetOrbit(float time)
    {
        SetOrbit(time, positionPci, velocityPci);
    }


    public void SetNewOrbit()
    {
        positionPci = transform.position;
        orbitalElements.SetCircularOrbit(Time.fixedTime, positionPci);
    }

    public void Recalculate(float time)
    {
        UpdatePositionAndVelocityAtTime(time);
    }

    public void AddDeltaV(float time, Vector3 deltaV)
    {
        var oldOrbitalElements = orbitalElements;
        orbitalElements.SetOrbit(time, positionPci, velocityPci + deltaV);
        if (orbitalElements.ra > maximumOrbitalRadius || orbitalElements.rp < minimumOrbitalRadius)
        {
            orbitalElements = oldOrbitalElements;
            // TODO: trigger gameplay context warning
            // "That's all the power it's got, it is just a barge!"
            // "Any lower and you'll hit the surface.!

            Debug.LogWarning("Orbit radius limits exceeded - delta-v change ignored.");
            return;
        }
        orbitCached = false;
    }

    public void SetOrbit(float time, Vector3 worldPosition, Vector3 worldVelocity)
    {
        orbitalElements.SetOrbit(time, worldPosition, worldVelocity);
    }

    private void UpdatePositionAndVelocityAtTime(float time)
    {
        // recieves { NaN, NaN } from OrbitalElements.cs
        (positionPci, velocityPci) = orbitalElements.ToCartesian(time);
    }

    public Vector3[] GetOrbitWorldPositions(int numberOfPoints)
    {
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
        }

        return orbitCache;
    }

    /// <summary>
    /// Returns current orbital statistics.
    /// </summary>
    /// <returns></returns>
    public OrbitalStatHolder GetOrbitalStats()
    {
        return new OrbitalStatHolder(
            Position,
            Velocity,
            OrbitalElements.semiMajorAxis,
            OrbitalElements.eccentricity,
            OrbitalElements.nu,
            OrbitalElements.T,
            OrbitalElements.omega,
            OrbitalElements.rp,
            OrbitalElements.ra
        );
    }
}