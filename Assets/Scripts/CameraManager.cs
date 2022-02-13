using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] GameObject cameraPlane;
    [SerializeField] GameObject sampleShip;


    void Awake()
    {
        cameraPlane.SetActive(false);
        sampleShip.SetActive(false);
        QualitySettings.vSyncCount = 1;
    }


    // void Update()
    // {
    //     // follow player
    // }
}
