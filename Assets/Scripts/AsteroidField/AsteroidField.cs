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

    // /// <summary> Asteroid field boundary. </summary>
    // [HideInInspector] public BoxCollider2D fieldBounds;

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

    [Tooltip("Objects to choose randomly amongst when respawning debris.")]
    [SerializeField] private List<AsteroidPrefab<GameObject, int>> asteroidList
        = new List<AsteroidPrefab<GameObject, int>>();

    private AsteroidDebrisData[] debris;

    private int totalWeights;


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
        // fieldBounds = GameObject.FindGameObjectWithTag("AsteroidBoundary").GetComponent<BoxCollider2D>();

        // cache rotational center
        planet = GameObject.FindGameObjectWithTag("Planet").transform.position;

        // setup debris data for spawning
        debris = new AsteroidDebrisData[asteroidList.Count];
        for (int i = 0; i < asteroidList.Count; i++)
        {
            debris[i] = new AsteroidDebrisData
            (
                asteroidList[i].prefab.GetComponent<MeshFilter>().sharedMesh,
                asteroidList[i].prefab.GetComponent<Rigidbody2D>().mass,
                asteroidList[i].prefab.GetComponent<CircleCollider2D>().radius,
                asteroidList[i].weight
            );

            totalWeights += asteroidList[i].weight;
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

        // var zeroRadius = planet.magnitude;
        // var maxRadius = (fieldBounds.bounds.extents.y) + zeroRadius;
        // var asteroidRadius = (position - planet).magnitude;
        // return (maxRadius - asteroidRadius) * 0 *
        //        orbitalSpeedVariance / 100f;
        return Random.Range(-orbitalSpeedVariance, orbitalSpeedVariance);
    }


    /// <summary> Returns random position within spawn area. </summary>
    public Vector2 RandomSpawnPosition()
    {
        var bounds = spawnBounds.bounds;
        var min = bounds.min;
        var max = bounds.max;
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }


    /// <summary> Selects a random debris field item, influenced by probability weighting. </summary>
    /// <returns>AsteroidDebrisData</returns>
    public AsteroidDebrisData RandomDebris()
    {
        int randomNumber = Random.Range(1, totalWeights + 1);
        int pos = 0;

        for (int i = 0; i < debris.Length; i++)
        {
            if (randomNumber <= debris[i].probability + pos)
            {
                return debris[i];
            }

            pos += asteroidList[i].weight;
        }

        // if that fails (which it shouldn't...)
        // return default asteroid (index 0)
        return debris[0];
    }

    public Vector3 RandomRotationalVelocity()
    {
        return new Vector3(
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed)
        );
    }
}


/// <summary> Holds prefab and probability weighting data. </summary>
/// <typeparam name="GameObject">Debris Prefab</typeparam>
/// <typeparam name="Integer">Probability Weight</typeparam>
[System.Serializable] public struct AsteroidPrefab<GameObject, Integer>
{
    public AsteroidPrefab(GameObject _prefab, int _weight) => (prefab, weight) = (_prefab, _weight);

    [Tooltip("Debris Prefab.")]
    [field: SerializeField] public GameObject prefab { get; private set; }

    [Tooltip("Item probability weighting.")]
    [field: SerializeField] public int weight { get; private set; }
}


/// <summary>
/// Stores necessary data to swap mesh of an asteroid.
/// Includes mesh, mass, radius, and probability data.
/// Created from asteroidList at runtime.
/// </summary>
public struct AsteroidDebrisData
{
    public AsteroidDebrisData(Mesh _mesh, float _mass, float _radius, int _probability)
        => (prefabMesh, mass, radius, probability) = (_mesh, _mass, _radius, _probability);
    public Mesh prefabMesh;
    public float mass;
    public float radius;
    public int probability;
}
