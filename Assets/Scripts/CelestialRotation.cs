using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialRotation : MonoBehaviour
{
    // private void Start()
    // {
    //     // not used
    // }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, (float)AsteroidField.instance.fieldSpeed / 100f) * Time.deltaTime);
    }
}
