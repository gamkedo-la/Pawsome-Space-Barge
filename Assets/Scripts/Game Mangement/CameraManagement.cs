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

    [SerializeField, Tooltip("Minimap Text Objects")]
    private GameObject[] mapText;

    [Tooltip("Camera used for certain dialogs. Faces in to planet.")] [SerializeField]
    private Camera inwardDialogCamera;

    [Tooltip("Camera used for certain dialogs. Faces out from planet.")] [SerializeField]
    private Camera outwardDialogCamera;

    [Tooltip("The camera mode used when the scene starts")] [SerializeField]
    private CameraMode initialCameraMode;

    [Tooltip("If enabled, the minimap will track the player ship if there is just one player")] [SerializeField]
    private bool minimapFollowSinglePlayer;

    /// <summary>
    /// Array of objects to be hidden on game load.
    /// Things like the sample ship to help visualize scales.
    /// </summary>
    [SerializeField] private GameObject[] hiddenObjects;

    private CameraMode cameraMode;
    private Dictionary<int, Camera> playerCameras = new Dictionary<int, Camera>();
    private bool refreshCameras;

    // minimap display
    private bool minimapEnlarged = false;
    private Rect smallMinimap = new Rect(0, 0, 0.22f, 0.38f);
    private Rect enlargedMinimap = new Rect(0, 0, 0.58f, 1f);



    public CameraMode CurrentCameraMode => cameraMode;



    private void Awake()
    {
        // set vsync on
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;

        // hide all objects in array
        foreach (GameObject o in hiddenObjects)
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

        ToggleMinimapText(false);
    }


    public void EnlageMinimap()
    {
        minimapCamera.rect = enlargedMinimap;

        // ToggleMinimapText(true);

        minimapEnlarged = true;
    }


    public void DispalyMinimapText(bool enabled)
    {
        ToggleMinimapText(enabled);
    }


    public void ShrinkMinimap()
    {
        minimapCamera.rect = smallMinimap;

        ToggleMinimapText(false);

        minimapEnlarged = false;
    }


    public void ToggleMinimapSize()
    {
        if (minimapEnlarged)
        {
            ShrinkMinimap();
        }
        else
        {
            EnlageMinimap();
        }
    }


    public void ToggleMinimapText(bool enabled)
    {
        foreach (var text in mapText)
        {
            text.SetActive(enabled);
        }
    }


    private int firstPlayer = -1;
    private int maxPlayerIndex = -1;
    public void PlayerJoined(PlayerInput pi)
    {
        var isFirstPlayer = playerCameras.Count == 0;

        var playerCamera = pi.GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = pi.transform.parent.GetComponentInChildren<Camera>();

        playerCameras.Add(pi.playerIndex, playerCamera);

        if (AsteroidField.Instance.IndividualSpawnZones)
        {
            PlayerSpawnCollider psc = pi.gameObject.GetComponentInChildren<PlayerSpawnCollider>(false);

            if (psc != null)
            {
                AsteroidField.Instance.AddPlayerCollider(psc.InitializePlayerSpawnArea());
            }
        }


        if (isFirstPlayer)
        {
            cameraMode = CameraMode.ThirdPerson;
            firstPlayer = pi.playerIndex;
            playerCamera.gameObject.AddComponent<AudioListener>();
        }

        if (pi.playerIndex > maxPlayerIndex) maxPlayerIndex = pi.playerIndex;

        if (playerCameras.Count > 1)
        {
            GameManagement.Instance.InputManager.splitScreen = true;
        }


        refreshCameras = true;
    }


    public void PlayerLeft(PlayerInput pi)
    {
        playerCameras.Remove(pi.playerIndex);

        if (AsteroidField.Instance.IndividualSpawnZones)
        {
            AsteroidField.Instance.RemovePlayerCollider(
                pi.gameObject.GetComponentInChildren<PlayerSpawnCollider>().playerCollider
            );
        }

        if (playerCameras.Count == 0)
        {
            cameraMode = CameraMode.Overhead;
        }

        refreshCameras = true;
    }


    public void DeviceLost(PlayerInput pi)
    {
        Destroy(pi.gameObject.GetComponent<AudioListener>());

        PlayerLeft(pi);

        if (pi.playerIndex == firstPlayer)
        {
            if (playerCameras.Count > 0)
            {
                for (int i = 0; i <= maxPlayerIndex; i++)
                {
                    Camera thing;

                    if (playerCameras.TryGetValue(i, out thing))
                    {
                        firstPlayer = i;
                        thing.gameObject.AddComponent<AudioListener>();
                        break;
                    }
                }
            }
            else
            {
                cameraMode = CameraMode.Overhead;
            }
        }

        if (playerCameras.Count < 2)
        {
            GameManagement.Instance.InputManager.splitScreen = false;
        }

        Destroy(pi.gameObject);
    }


    /// <summary>
    /// For retrieving the camera currently active for the given PlayerInput. Intended for use with joystick input.
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
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


    private CameraMode prevCameraMode;
    public void ExplainerOverhead(bool enable)
    {
        refreshCameras = true;

        if (enable)
        {
            prevCameraMode = cameraMode;
            cameraMode = CameraMode.Overhead;
        }
        else
        {
            cameraMode = prevCameraMode;
        }
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
        if (GameManagement.Instance.MissionManager.Direction == MissionDirection.Inwards)
        {
            inwardDialogCamera.gameObject.SetActive(true);
            outwardDialogCamera.gameObject.SetActive(false);
        }
        else
        {
            inwardDialogCamera.gameObject.SetActive(false);
            outwardDialogCamera.gameObject.SetActive(true);
        }

        overheadCamera.gameObject.SetActive(false);
        SetActivePlayerCameras(false);
    }


    private void SetThirdPersonMode()
    {
        SetActivePlayerCameras(true);

        overheadCamera.gameObject.SetActive(false);
        inwardDialogCamera.gameObject.SetActive(false);
        outwardDialogCamera.gameObject.SetActive(false);
    }


    private void SetOverheadMode()
    {
        overheadCamera.gameObject.SetActive(true);

        SetActivePlayerCameras(false);
        inwardDialogCamera.gameObject.SetActive(false);
        outwardDialogCamera.gameObject.SetActive(false);
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
