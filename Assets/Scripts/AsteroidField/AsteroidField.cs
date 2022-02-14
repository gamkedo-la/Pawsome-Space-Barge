using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidField : MonoBehaviour
{
    // Singleton
    [HideInInspector] public static AsteroidField instance;

    // asteroid field boundary
    [HideInInspector] public BoxCollider2D fieldBounds;

    // field extents x + y
    [HideInInspector] public float fieldXextent;
    [HideInInspector] public float fieldYextent;

    // rotational center
    [HideInInspector] public Vector3 planet;

    [Tooltip("asteroid field 'speed'")]
    [Range(-100,100)]
    [SerializeField] public int fieldSpeed = 25;

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

        // setup boundary values
        fieldBounds = GameObject.FindGameObjectWithTag("AsteroidBoundary").GetComponent<BoxCollider2D>();
        fieldXextent = fieldBounds.bounds.extents.x;
        fieldYextent = fieldBounds.bounds.extents.y;

        // cache rotational center
        planet = GameObject.FindGameObjectWithTag("Planet").transform.position;
    }
}
