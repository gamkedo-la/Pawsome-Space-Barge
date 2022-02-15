using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asteroid Field Manger script.
/// Mostly holds variables for other scripts, but also rotates planet.
/// </summary>
public class AsteroidField : MonoBehaviour
{
    /// <summary>
    /// AsteroidField Singleton.
    /// </summary>
    [HideInInspector] public static AsteroidField instance;
    public static float baseSpeed = 50f;

    /// <summary>
    /// Asteroid field boundary.
    /// </summary>
    [HideInInspector] public BoxCollider2D fieldBounds;

    /// <summary>
    /// Field extents x + y.
    /// </summary>
    // [HideInInspector] public float fieldXextent;
    // [HideInInspector] public float fieldYextent;

    // rotational center
    [HideInInspector] public Vector3 planet;
    [HideInInspector] private GameObject planetObject;

    [Tooltip("asteroid field 'speed'")]
    // [Range(-100,100)]
    [Range(-25,25)]
    [SerializeField] public float fieldSpeed = 25;

    [Tooltip("max random rotational speed for asteroids (degrees / second)")]
    [SerializeField] public float maxAsteroidTumbleSpeed = 5;

    [Tooltip("speed variance of asteroids")]
    [Range(0, 0.2f)]
    [SerializeField] public float orbitalSpeedVariance = 0.1f;

    private void Awake()
    {
        // setup singleton
        if (instance != null && instance != this) {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        // cache boundary
        fieldBounds = GameObject.FindGameObjectWithTag("AsteroidBoundary").GetComponent<BoxCollider2D>();

        // cache rotational center
        planetObject = GameObject.FindGameObjectWithTag("Planet");
        planet = planetObject.transform.position;
    }

    private void Update()
    {
        // rotate planet
        planetObject.transform.Rotate(
            new Vector3(
                0,
                0,
                // baseSpeed is used for offset so something is always moving.
                (fieldSpeed - baseSpeed) / 100f
            ) * Time.deltaTime,
            Space.World
        );
    }
}
