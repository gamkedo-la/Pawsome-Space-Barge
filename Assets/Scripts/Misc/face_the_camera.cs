using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class face_the_camera : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
