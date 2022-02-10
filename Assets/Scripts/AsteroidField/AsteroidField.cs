using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidField : MonoBehaviour
{
    // Singleton
    public static AsteroidField instance;

    // asteroid field boundary
    [HideInInspector] public BoxCollider2D fieldBounds;
    // field extents x + y
    [HideInInspector] public float fieldXextent;
    [HideInInspector] public float fieldYextent;

    // speed of asteroid field
    [SerializeField] public int fieldSpeed = 10;

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
    }
}
