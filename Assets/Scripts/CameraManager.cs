using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject[] hidenObjects;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;

        foreach (GameObject o in hidenObjects)
        {
            o.SetActive(false);
        }
    }

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
