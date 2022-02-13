using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialRotation : MonoBehaviour
{
    private float rotationSpeed;

    void Start()
    {
        rotationSpeed = (float)AsteroidField.instance.fieldSpeed / 80f;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
    }
}
