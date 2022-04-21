using UnityEngine;
using UnityEngine.InputSystem;

public class ShipActions : MonoBehaviour
{
    private PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    // toggle view event handler
    public void OnToggleView(InputAction.CallbackContext context)
    {
        GameManagement.Instance.CameraManager.ToggleCameraMode();
    }

    // Pause key handler
    public void OnPause(InputAction.CallbackContext context)
    {
        if (input.playerIndex == 0 && context.started)
        {
            GameManagement.Instance.OnPause(context);
        }
    }

    public void OnMinimap(InputAction.CallbackContext context)
    {
        GameManagement.Instance.CameraManager.ToggleMinimapSize();
    }

    public void OnDeviceLost(PlayerInput input)
    {
        GameManagement.Instance.CameraManager.DeviceLost(input);
    }
}