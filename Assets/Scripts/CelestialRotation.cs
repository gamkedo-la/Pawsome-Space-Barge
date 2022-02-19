using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialRotation : MonoBehaviour
{
    /// <summary> Asteroid Ring Quads </summary>
    private GameObject rings;

    /// <summary> The planet Caturn. </summary>
    private GameObject caturn;

    /// <summary> Lights representing the sun. </summary>
    private GameObject sun;


    private void Awake()
    {
        rings = GameObject.FindGameObjectWithTag("AsteroidRings");
        caturn = GameObject.FindGameObjectWithTag("Planet");
        sun = GameObject.FindGameObjectWithTag("Sun");
    }


    private void Update()
    {
        // rotate rings
        rings.transform.Rotate(
            new Vector3(0, 0, AsteroidField.Instance.fieldSpeed / 100f) * Time.deltaTime
        );

        // rotate sun
        sun.transform.Rotate(new Vector3(0,0,0.25f * Time.deltaTime), Space.World);

        // rotate planet
        caturn.transform.Rotate(
            new Vector3(0, 0,
                // baseSpeed is used for offset so planet is always rotating.
                (AsteroidField.Instance.fieldSpeed + AsteroidField.baseSpeed) / 100f * Time.deltaTime
            ),
            // because we're in space
            Space.World
        );
    }
}
