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

    [Tooltip("Camera used for certain dialogs.")] [SerializeField]
    private Camera dialogCamera;

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
    private Dictionary<int, Camera> playerCameras = new Dictionary<int, Camera>();
    private bool refreshCameras;

    // minimap display
    private bool minimapEnlarged = false;
    private Rect smallMinimap = new Rect(0.01f, 0.01f, 0.17f, 0.3f);
    private Rect enlargedMinimap = new Rect(0.01f, 0.01f, 0.55f, 0.98f);



    private void Awake()
    {
        // set vsync on
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;

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


    public void ToggleMinimapSize()
    {
        if (minimapEnlarged)
        {
            minimapCamera.rect = smallMinimap;
            minimapEnlarged = false;
        }
        else
        {
            minimapCamera.rect = enlargedMinimap;
            minimapEnlarged = true;
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

        playerCameras.Add(pi.playerIndex, playerCamera);

        if (isFirstPlayer)
        {
            cameraMode = CameraMode.ThirdPerson;
        }

        refreshCameras = true;
    }


    public void PlayerLeft(PlayerInput pi)
    {
        var playerCamera = pi.GetComponentInChildren<Camera>();
        playerCameras.Remove(pi.playerIndex);

        if (playerCameras.Count == 0)
        {
            cameraMode = CameraMode.Overhead;
        }

        refreshCameras = true;
    }


    public Transform GetActiveCamera(PlayerInput pi)
    {
        if (cameraMode == CameraMode.Overhead)
        {
            return overheadCamera.transform;
        }
        else
        {
            return playerCameras[pi.playerIndex].transform;
        }
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


    public void ToggleDialogCamera()
    {
        refreshCameras = true;

        cameraMode = cameraMode == CameraMode.Dialog ? CameraMode.Overhead : CameraMode.Dialog;
    }


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
        if (cameraMode == CameraMode.Dialog)
        {
            SetDialogMode();
        }
        else if (cameraMode == CameraMode.ThirdPerson)
        {
            SetThirdPersonMode();
        }
        else
        {
            SetOverheadMode();
        }

        refreshCameras = false;
    }


    private void SetDialogMode()
    {
        dialogCamera.gameObject.SetActive(true);
        overheadCamera.gameObject.SetActive(false);
        SetActivePlayerCameras(false);
    }


    private void SetThirdPersonMode()
    {
        dialogCamera.gameObject.SetActive(false);
        overheadCamera.gameObject.SetActive(false);
        SetActivePlayerCameras(true);
    }


    private void SetOverheadMode()
    {
        dialogCamera.gameObject.SetActive(false);
        overheadCamera.gameObject.SetActive(true);
        SetActivePlayerCameras(false);
    }


    private void SetActivePlayerCameras(bool value)
    {
        foreach (var playerCamera in playerCameras)
        {
            playerCamera.Value.gameObject.SetActive(value);
        }
    }


    [Serializable]
    public enum CameraMode
    {
        Overhead,
        ThirdPerson,
        Dialog
    }
}