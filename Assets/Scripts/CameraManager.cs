using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // [SerializeField] GameObject sampleShip;
    [SerializeField] private GameObject[] hidenObjects;


    void Awake()
    {
        // sampleShip.SetActive(false);
        QualitySettings.vSyncCount = 1;

        foreach (GameObject o in hidenObjects)
        {
            o.SetActive(false);
        }
    }


    // void Update()
    // {
    //     // follow player
    // }
}
