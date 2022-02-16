using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera Manager.
/// Sets vsync on, hides visual aid objects,
/// and controls primary external camera.
/// </summary>
public class CameraManager : MonoBehaviour
{
    /// <summary>
    /// Array of objects to be hidden on game load.
    /// Things like the sample ship to help visualize scales.
    /// </summary>
    [SerializeField] private GameObject[] hidenObjects;

    private void Awake()
    {
        // set vsync on
        QualitySettings.vSyncCount = 1;

        // hide all objects in array
        foreach (GameObject o in hidenObjects)
        {
            o.SetActive(false);
        }
    }

    // sudo code for camera behaviour:
    // private void Update()
    // {
    //     // follow player?

    //     // if (playerCount > 1 || cameraMode == CameraMode.Mode2)
    //     // {
    //     //     // 3rd person cameras + split screen
    //     // }
    //     // else // cameraMode == CameraMode.Mode1
    //     // {
    //     //     if (player touching PlayerBoundary)
    //     //     {
    //     //         // overhead camera
    //     //     }

    //     //     if (player !touching PlayerBoundary)
    //     //     {
    //     //         // barge centered player following, zoom in as player gets farther
    //     //     }
    //     // }
    // }
}
