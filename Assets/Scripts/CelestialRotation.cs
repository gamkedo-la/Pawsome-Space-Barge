using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialRotation : MonoBehaviour
{
    // void Start()
    // {

    // }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, (float)AsteroidField.instance.fieldSpeed / 100f) * Time.deltaTime);
    }
}
