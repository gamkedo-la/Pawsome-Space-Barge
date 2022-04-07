using UnityEngine;
using UnityEngine.InputSystem;

public class ShipActions : MonoBehaviour
{
    
    private CameraManager cameraManager;

    private void Awake()
    {
        cameraManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<CameraManager>();
    }

    // toggle view event handler
    public void OnToggleView(InputAction.CallbackContext context)
    {
        cameraManager.ToggleCameraMode();
    }

    // Pause key handler
    public void OnPause(InputAction.CallbackContext context)
    {
        GameManagement.Instance.OnPause(context);
    }
}