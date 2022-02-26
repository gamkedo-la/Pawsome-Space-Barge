using System;
using UnityEngine;

// Serializable mostly to make it appear in the inspector
[Serializable]
public class OrbitalElements
{
    public float semiMajorAxis;
    public float eccentricity;
    public float nu;
    public float T;
    public float omega;

    /// <summary>
    /// Gravitational constant + mass for central body.
    ///
    /// We calculate this from the initial call to CircularOrbit, since by the given position and velocity, and the requirement
    /// that the orbit is circular, there can be only one value that fulfills it.
    ///
    /// Doing this, we do not have to tune this constant if we alter any parameters
    /// </summary>
    private float mu;

    // https://elainecoe.github.io/orbital-mechanics-calculator/scripts/calculator.js
    // https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
    //
    // Good lectures on Spacecraft Dynamic & Control:
    // http://control.asu.edu/MAE462_frame.htm

    // Orbital Elements do not include inclination, since we will assume all orbits in the equatorial plane

    public (Vector3, Vector3) ToCartesian(float t)
    {
        var meanAnomaly = Mathf.Sqrt(mu / Mathf.Pow(semiMajorAxis, 3)) * (t - T);
        meanAnomaly = Mathf.Repeat(meanAnomaly, 2 * Mathf.PI);
        var eccentricAnomaly = SolveEccentricAnomaly(meanAnomaly);
        var trueAnomaly = 2 * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(eccentricAnomaly / 2),
            Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(eccentricAnomaly / 2));
        var r = semiMajorAxis * (1 - eccentricity * Mathf.Cos(eccentricAnomaly));
        var p = semiMajorAxis * (1 - eccentricity * eccentricity);
        var h = Mathf.Sqrt(mu * p);

        const float cosOmega = 1;
        const float sinOmega = 0;
        var lat = trueAnomaly + omega;
        var cosLat = Mathf.Cos(lat);
        var sinLat = Mathf.Sin(lat);

        var pos = r * new Vector3(
            cosOmega * cosLat - sinOmega * sinLat,
            sinOmega * cosLat + cosOmega * sinLat,
            0
        );

        var herp = h * eccentricity / (r * p);
        var vel = new Vector3(
            pos.x * herp * Mathf.Sin(trueAnomaly) -
            h / r * (cosOmega * sinLat + sinOmega * cosLat),
            pos.y * herp * Mathf.Sin(trueAnomaly) - h / r * (sinOmega * sinLat - cosOmega * cosLat),
            0
        );
        return (pos, vel);
    }

    private float SolveEccentricAnomaly(float m)
    {
        // Solve E given M
        // E(n+1) = E(n) - (E(n) - e * sin(E(n)) - M) / (1 - e * cos (E(n)))
        var e0 = m;
        for (var i = 0; i < 30; i++)
        {
            var e1 = e0 - (e0 - eccentricity * Mathf.Sin(e0) - m) / (1 - eccentricity * Mathf.Cos(e0));
            var error = Mathf.Abs(e1 - e0);
            e0 = e1;
            if (error < 1e-4)
            {
                break;
            }
        }

        return e0;
    }

    public static OrbitalElements CircularOrbit(float time, Vector3 position, Vector3 velocity)
    {
        var oe = new OrbitalElements
        {
            mu = position.magnitude * velocity.sqrMagnitude,
        };

        oe.SetOrbitFromObservation(time, position, velocity);
        return oe;
    }

    public void SetOrbitFromObservation(float time, Vector3 position, Vector3 velocity)
    {
        var r = position.magnitude;
        var vSquared = velocity.sqrMagnitude;

        var h = Vector3.Cross(position, velocity);
        var rDotV = Vector3.Dot(position, velocity);

        var e = 1 / mu * ((vSquared - mu / r) * position - rDotV * velocity);
        eccentricity = e.magnitude;

        var specE = vSquared / 2 - mu / r;
        semiMajorAxis = -mu / (2 * specE);

        if (eccentricity < 1e-3)
        {
            nu = 0;
        }
        else
        {
            var arg = Vector3.Dot(position, e) / (r * eccentricity);
            nu = Mathf.Acos(arg);
            if (rDotV < 0)
            {
                nu = Mathf.PI * 2 - nu;
            }
        }

        // omega is how much we are offset from periapsis at the time of observation
        // it is still not clear to me why this is necessary, as I feel it should be baked into the T calculation.
        // It might be clearer if I watch some more lectures on the subject :)
        omega = Mathf.Repeat(Mathf.Atan2(position.y, position.x) - nu, Mathf.PI * 2);

        var ea = 2 * Mathf.Atan(Mathf.Sqrt((1 - eccentricity) / (1 + eccentricity)) * Mathf.Tan(nu / 2));
        var n = Mathf.Sqrt(mu / Mathf.Pow(semiMajorAxis, 3));

        T = time - (ea - eccentricity * Mathf.Sin(ea)) / n;
    }

    public override string ToString()
    {
        return
            $"{nameof(semiMajorAxis)}: {semiMajorAxis}, {nameof(eccentricity)}: {eccentricity}, {nameof(nu)}: {nu * Mathf.Rad2Deg}, {nameof(omega)}: {omega * Mathf.Rad2Deg}, {nameof(T)}: {T}, {nameof(mu)}: {mu}";
    }
}