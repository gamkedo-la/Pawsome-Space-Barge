using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] int targetFPS = 60;
    [SerializeField] GameObject cameraPlane;
    [SerializeField] GameObject sampleShip;


    void Awake()
    {
        cameraPlane.SetActive(false);
        sampleShip.SetActive(false);
        Application.targetFrameRate = targetFPS;
    }


    // void Update()
    // {
    //     // follow player
    // }
}
