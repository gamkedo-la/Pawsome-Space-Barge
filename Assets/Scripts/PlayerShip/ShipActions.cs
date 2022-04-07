using UnityEngine;
using UnityEngine.InputSystem;

public class ShipActions : MonoBehaviour
{
    private PlayerInput input;

    private CameraManager cameraManager;

    private void Awake()
    {
        cameraManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<CameraManager>();
        input = GetComponent<PlayerInput>();
    }

    // toggle view event handler
    public void OnToggleView(InputAction.CallbackContext context)
    {
        cameraManager.ToggleCameraMode();
    }

    // Pause key handler
    public void OnPause(InputAction.CallbackContext context)
    {
        if (input.playerIndex == 0 && context.started)
        {
            GameManagement.Instance.OnPause(context);
        }
    }
}