using System;
using UnityEngine;

[Serializable]
public struct OrbitalElements
{
    public float semiMajorAxis;
    public float eccentricity;
    public float nu;
    public float T;
    public float omega;
    public float rp;
    public float ra;
    public float speedMultiplier;

    public float Mu
    {
        get
        {
            if (speedMultiplier <= 0)
            {
                speedMultiplier = 0.0001f;
            }

            return 1218470*speedMultiplier;
        }
    }

    // https://elainecoe.github.io/orbital-mechanics-calculator/scripts/calculator.js
    // https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
    //
    // Good lectures on Spacecraft Dynamic & Control:
    // http://control.asu.edu/MAE462_frame.htm

    // Orbital Elements do not include inclination, since we will assume all orbits in the equatorial plane

    public (Vector3, Vector3) ToCartesian(float t)
    {
        var meanAnomaly = Mathf.Sqrt(Mu / (semiMajorAxis * semiMajorAxis * semiMajorAxis)) * (t - T);
        meanAnomaly = Mathf.Repeat(meanAnomaly, 2 * Mathf.PI);
        var eccentricAnomaly = SolveEccentricAnomaly(meanAnomaly);
        var trueAnomaly = 2 * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(eccentricAnomaly / 2),
            Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(eccentricAnomaly / 2));
        var r = semiMajorAxis * (1 - eccentricity * Mathf.Cos(eccentricAnomaly));
        var p = semiMajorAxis * (1 - eccentricity * eccentricity);
        var h = Mathf.Sqrt(Mu * p);

        const float cosOmega = 1;
        const float sinOmega = 0;
        var lat = trueAnomaly + omega;
        
#if UNITY_EDITOR
        if (float.IsNaN(lat) || float.IsNaN(r))
        {
            Debug.LogError($"NaN in OrbitalElements({this}).ToCartesian({t}). Variables: ma: {meanAnomaly}, ea: {eccentricAnomaly}, ta: {trueAnomaly}, r: {r}, p: {p}, h: {h}, lat: {lat}");
        }
#endif
        
        var cosLat = Mathf.Cos(lat);
        var sinLat = Mathf.Sin(lat);

        // creates { NaN, NaN } vector...
        var pos = r * new Vector3(
            cosOmega * cosLat - sinOmega * sinLat,
            sinOmega * cosLat + cosOmega * sinLat,
            0
        );

        var herp = h * eccentricity / (r * p);

        // creates { NaN, NaN } vector...
        var vel = new Vector3(
            pos.x * herp * Mathf.Sin(trueAnomaly) -
            h / r * (cosOmega * sinLat + sinOmega * cosLat),
            pos.y * herp * Mathf.Sin(trueAnomaly) - h / r * (sinOmega * sinLat - cosOmega * cosLat),
            0
        );

        return (pos, vel); // returns the  { NaN, NaN } vectors...
    }

    public void GetOrbitCoordinates(Vector3[] points)
    {
        var n = points.Length;
        for (var i = 0; i < n; i++)
        {
            var meanAnomaly = 2f * Mathf.PI * i / n;
            var eccentricAnomaly = SolveEccentricAnomaly(meanAnomaly);
            var trueAnomaly = 2 * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(eccentricAnomaly / 2),
                Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(eccentricAnomaly / 2));
            var r = semiMajorAxis * (1 - eccentricity * Mathf.Cos(eccentricAnomaly));
            var lat = trueAnomaly + omega;

            points[i] = r * new Vector3(Mathf.Cos(lat), Mathf.Sin(lat), 0);
        }
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

    /// <summary>
    /// Sets a circular orbit from an observation of a position at a certain time
    ///
    /// This function calculates what velocity is necessary in order to have a stable circular orbit at the observed distance
    /// </summary>
    public void SetCircularOrbit(float time, Vector3 position)
    {
        var velocity = Vector3.Cross(position, Vector3.forward).normalized * Mathf.Sqrt(Mu / position.magnitude);

        SetOrbit(time, position, velocity);
    }

    /// <summary>
    /// Set an orbit from an observation of a position and velocity at a certain time
    /// </summary>
    public void SetOrbit(float time, Vector3 position, Vector3 velocity)
    {
        var r = position.magnitude;
        var vSquared = velocity.sqrMagnitude;

        var rDotV = Vector3.Dot(position, velocity);

        var ev = 1 / Mu * ((vSquared - Mu / r) * position - rDotV * velocity);
        var e = ev.magnitude;

        var specE = vSquared / 2 - Mu / r;
        var a = -Mu / (2 * specE);

        if (e is < 0 or >= 1)
        {
            Debug.LogWarning($"Refusing SetOrbit({time}, {position}, {velocity}) because calculated e is {e}");
            return;
        }

        if (a <= 0)
        {
            Debug.LogWarning($"Refusing SetOrbit({time}, {position}, {velocity}) because calculated a is {a}");
            return;
        }

        eccentricity = e;
        semiMajorAxis = a;

        nu = 0f;
        if (eccentricity > 1e-3)
        {
            var arg = Vector3.Dot(position, ev) / (r * eccentricity);
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

        rp = semiMajorAxis * (1 - eccentricity);
        ra = semiMajorAxis * (1 + eccentricity);

        var ea = 2 * Mathf.Atan(Mathf.Sqrt(rp / ra) * Mathf.Tan(nu / 2));
        var n = Mathf.Sqrt(Mu / Mathf.Pow(semiMajorAxis, 3));

        T = time - (ea - eccentricity * Mathf.Sin(ea)) / n;
    }

    public override string ToString()
    {
        return
            $"a: {semiMajorAxis}, e: {eccentricity}, ν: {nu * Mathf.Rad2Deg}, ω: {omega * Mathf.Rad2Deg}, T: {T}, μ: {Mu}, rp: {rp}, ra: {ra}, sx: {speedMultiplier}";
    }
}