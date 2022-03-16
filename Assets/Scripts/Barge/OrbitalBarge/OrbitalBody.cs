﻿using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    [Header("Limits")] 
    [SerializeField] private float minimumOrbitalRadius = 2000f;
    [SerializeField] private float maximumOrbitalRadius = 7000f;
    
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

    private void OnEnable()
    {
        SetOrbit(0, positionPci, velocityPci);
    }

    public void SetOrbit(float time)
    {
        SetOrbit(time, positionPci, velocityPci);
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
}