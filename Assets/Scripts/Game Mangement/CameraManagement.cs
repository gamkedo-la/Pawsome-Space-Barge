using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera Manager.
/// Sets vsync on, hides visual aid objects,
/// and controls primary external camera.
/// </summary>
public class CameraManagement : MonoBehaviour
{
    [Tooltip("Overhead camera used for full screen overhead view")] [SerializeField]
    private Camera overheadCamera;

    [Tooltip("Overhead camera used for the minimap")] [SerializeField]
    private Camera minimapCamera;

    [Tooltip("The camera mode used when the scene starts")] [SerializeField]
    private CameraMode initialCameraMode;

    [Tooltip("If enabled, the minimap will track the player ship if there is just one player")] [SerializeField]
    private bool minimapFollowSinglePlayer;

    /// <summary>
    /// Array of objects to be hidden on game load.
    /// Things like the sample ship to help visualize scales.
    /// </summary>
    [SerializeField] private GameObject[] hidenObjects;

    private CameraMode cameraMode;
    private List<Camera> playerCameras = new();
    private bool refreshCameras;

    private void Awake()
    {
        // set vsync on
        QualitySettings.vSyncCount = 1;

        // hide all objects in array
        foreach (GameObject o in hidenObjects)
        {
            o.SetActive(false);
        }

        cameraMode = initialCameraMode;
        SetActiveCameras();

        var minimapBits = GameObject.FindGameObjectsWithTag("Minimap Marker");

        foreach (GameObject thing in minimapBits)
        {
            try
            {
                thing.GetComponent<MeshRenderer>().enabled = true;
            }
            catch
            {
                //
            }
        }
    }

    public void PlayerJoined(PlayerInput pi)
    {
        var isFirstPlayer = playerCameras.Count == 0;

        // if using PlayerShip prefab:
        var playerCamera = pi.GetComponentInChildren<Camera>();
        // // if using PlayerShip-freeCamera prefab
        // // stops errors but minimap is broken
        // var playerCamera = pi.transform.parent.GetComponentInChildren<Camera>();

        playerCameras.Add(playerCamera);

        if (isFirstPlayer)
        {
            cameraMode = CameraMode.ThirdPerson;
        }

        refreshCameras = true;
    }

    public void PlayerLeft(PlayerInput pi)
    {
        var playerCamera = pi.GetComponentInChildren<Camera>();
        playerCameras.Remove(playerCamera);

        if (playerCameras.Count == 0)
        {
            cameraMode = CameraMode.Overhead;
        }

        refreshCameras = true;
    }

    public void ToggleCameraMode()
    {
        refreshCameras = true;
        // Make sure we don't switch to third person when there is no active player
        if (playerCameras.Count == 0)
        {
            cameraMode = CameraMode.Overhead;
            return;
        }

        // Toggle camera mode between overhead and third person
        cameraMode = cameraMode == CameraMode.Overhead ? CameraMode.ThirdPerson : CameraMode.Overhead;
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

    private void LateUpdate()
    {
        if (refreshCameras)
        {
            SetActiveCameras();
        }

        if (minimapCamera.gameObject.activeSelf && minimapFollowSinglePlayer && playerCameras.Count == 1)
        {
            FollowTransformXY(minimapCamera.transform, playerCameras[0].transform.parent.transform);
        }
    }

    private void FollowTransformXY(Transform follower, Transform target)
    {
        var targetPosition = target.position;
        var followerPosition = follower.position;

        followerPosition.x = targetPosition.x;
        followerPosition.y = targetPosition.y;
        follower.position = followerPosition;
    }

    private void SetActiveCameras()
    {
        if (cameraMode == CameraMode.Overhead)
        {
            SetOverheadMode();
        }
        else
        {
            SetThirdPersonMode();
        }

        refreshCameras = false;
    }

    private void SetThirdPersonMode()
    {
        overheadCamera.gameObject.SetActive(false);
        foreach (var playerCamera in playerCameras)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }

    private void SetOverheadMode()
    {
        overheadCamera.gameObject.SetActive(true);
        foreach (var playerCamera in playerCameras)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    [Serializable]
    public enum CameraMode
    {
        Overhead,
        ThirdPerson
    }
}