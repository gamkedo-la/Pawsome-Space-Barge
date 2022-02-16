using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asteroid Field Manger.
/// Mostly holds variables for other scripts.
/// </summary>
public class AsteroidField : MonoBehaviour
{
    /// <summary> AsteroidField Singleton. </summary>
    [HideInInspector] public static AsteroidField instance;

    /// <summary>
    /// Base speed of planet rotation.
    /// When fieldSpeed == 0 the planet still rotates at this speed.
    /// </summary>
    public static float baseSpeed = 100f;

    /// <summary> Asteroid field boundary. </summary>
    [HideInInspector] public BoxCollider2D fieldBounds;

    /// <summary> Rotational center of things. </summary>
    [HideInInspector] public Vector3 planet;


    [Tooltip("Asteroid field 'speed'.")]
    [Range(-50,50)]
    [SerializeField] public float fieldSpeed = 25;

    [Tooltip("Max random rotational speed for asteroids (degrees / second).")]
    [SerializeField] public float maxAsteroidTumbleSpeed = 5;

    [Tooltip("Speed variance of asteroids.")]
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
        planet = GameObject.FindGameObjectWithTag("Planet").transform.position;
    }

    // private void Update()
    // {
    //     // 
    // }
}
