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

    /// <summary> Rotational center of things. </summary>
    [HideInInspector] public Vector3 planet;


    [Header("World Settings")]
    [Tooltip("Asteroid field 'speed'.")]
    [Range(-50,50)]
    [SerializeField] public float fieldSpeed = 25;

    [Tooltip("Max random rotational speed for asteroids (degrees / second).")]
    [SerializeField] public float maxAsteroidTumbleSpeed = 5;

    [Tooltip("Speed variance of asteroids.")]
    [Range(0, 0.2f)]
    [SerializeField] public float orbitalSpeedVariance = 0.1f;


    [Header("Asteroid Factories")]
    [SerializeField, Tooltip("Outer ring factory.")] public RandomPrefabFactory outerRing;
    [SerializeField, Tooltip("Middle ring factory.")] public RandomPrefabFactory middleRing;
    [SerializeField, Tooltip("Inner ring factory.")] public RandomPrefabFactory innerRing;


    [Header("Asteroid Particle Systems")]
    [SerializeField, Tooltip("Outer ring particle system.")] private ParticleSystem outerRingParticles;
    [SerializeField, Tooltip("Middle ring particle system.")] private ParticleSystem middleRingParticles;
    [SerializeField, Tooltip("Inner ring particle system")] private ParticleSystem innerRingParticles;


    [Header("Asteroid Spawn Zone Settings")]
    [SerializeField] private bool individualSpawnZones = false;
    public bool IndividualSpawnZones => individualSpawnZones;


    // to enable editing of asteroid ring factories
    [HideInInspector] public bool outerRingFoldout;
    [HideInInspector] public bool middleRingFoldout;
    [HideInInspector] public bool innerRingFoldout;



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
    }


    /// <summary> Returns a random speed suitable to an asteroid in this particular orbit </summary>
    public float RandomSpeed(Vector3 position)
    {
        // Asteroids with lower orbits are rotating faster around the planet than asteroids in higher orbits
        //
        // This means that if the speed we set on asteroids are their speed relative to the barge, then asteroids
        // in lower orbits have a lower speed relative to the barge, compared to asteroids in higher orbits.
        //
        // But that looks kinda weird, because the ring graphics is on a fixed speed, so I'll leave this commented
        // out for now.

        // TODO: try this again
        // var zeroRadius = planet.magnitude;
        // var maxRadius = (fieldBounds.bounds.extents.y) + zeroRadius;
        // var asteroidRadius = (position - planet).magnitude;
        // return (maxRadius - asteroidRadius) * 0 *
        //        orbitalSpeedVariance / 100f;

        // original method
        return Random.Range(-orbitalSpeedVariance, orbitalSpeedVariance);
    }


    /// <summary> Returns a random Vector3. For use as rotational speed degrees/s for debris and asteroids.  </summary>
    public Vector3 RandomRotationalVelocity()
    {
        return new Vector3(
            Random.Range(-maxAsteroidTumbleSpeed, maxAsteroidTumbleSpeed),
            Random.Range(-maxAsteroidTumbleSpeed, maxAsteroidTumbleSpeed),
            Random.Range(-maxAsteroidTumbleSpeed, maxAsteroidTumbleSpeed)
        );
    }


    /// <summary>
    /// Adds player collider to triggers list for each asteroid ring.
    /// </summary>
    /// <param name="playerCollider"></param>
    public void AddPlayerCollider(SphereCollider playerCollider)
    {
        outerRingParticles.trigger.AddCollider(playerCollider);
        middleRingParticles.trigger.AddCollider(playerCollider);
        innerRingParticles.trigger.AddCollider(playerCollider);
    }


    /// <summary>
    /// Removes player collider from triggers list of each asteroid ring.
    /// </summary>
    /// <param name="playerCollider"></param>
    public void RemovePlayerCollider(SphereCollider playerCollider)
    {
        outerRingParticles.trigger.RemoveCollider(playerCollider);
        middleRingParticles.trigger.RemoveCollider(playerCollider);
        innerRingParticles.trigger.RemoveCollider(playerCollider);
    }
}
