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
    [HideInInspector] public static AsteroidField Instance;

    /// <summary>
    /// Base speed of planet rotation.
    /// When fieldSpeed == 0 the planet still rotates at this speed.
    /// </summary>
    public static float baseSpeed = 100f;

    /// <summary> Asteroid field boundary. </summary>
    [HideInInspector] public BoxCollider2D fieldBounds;

    public BoxCollider2D spawnBounds;

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
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else
        {
            Instance = this;
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
    
    /// <summary>
    /// Returns a random speed suitable to an asteroid in this particular orbit
    /// </summary>
    public float RandomSpeed(Vector3 position)
    {
        // Asteroids with lower orbits are rotating faster around the planet than asteroids in higher orbits
        //
        // This means that if the speed we set on asteroids are their speed relative to the barge, then asteroids
        // in lower orbits have a lower speed relative to the barge, compared to asteroids in higher orbits.
        //
        // But that looks kinda weird, because the ring graphics is on a fixed speed, so I'll leave this commented
        // out for now.
        
        // var zeroRadius = planet.magnitude;
        // var maxRadius = (fieldBounds.bounds.extents.y) + zeroRadius;
        // var asteroidRadius = (position - planet).magnitude;
        // return (maxRadius - asteroidRadius) * 0 * 
        //        orbitalSpeedVariance / 100f; 
        return Random.Range(-orbitalSpeedVariance, orbitalSpeedVariance);
    }

    public Vector2 RandomSpawnPosition()
    {
        var bounds = spawnBounds.bounds;
        var min = bounds.min;
        var max = bounds.max;
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }
}
