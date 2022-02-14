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

    // speed of asteroid field
    [Range(-100,100)]
    [SerializeField] public int fieldSpeed = 25;
    [SerializeField] public float minAsteroidTumbleSpeed = 0;
    [SerializeField] public float maxAsteroidTumbleSpeed = 5;
    [SerializeField] public float orbitalSpeedVariance = 10;

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
        fieldBounds = GetComponent<BoxCollider2D>();
        fieldXextent = fieldBounds.bounds.extents.x;
        fieldYextent = fieldBounds.bounds.extents.y;

        // cache rotational center
        planet = GameObject.FindGameObjectWithTag("Planet").transform.position;
    }
}
