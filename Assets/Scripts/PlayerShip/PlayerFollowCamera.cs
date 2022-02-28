using UnityEngine;

/// <summary>
/// Follows transform, and rotates with it.
/// Has the effect of looking in front of the player heading letting the player drift to one side of the frame.
/// Helps keep camera focused on what's coming.
/// </summary>
public class PlayerFollowCamera : MonoBehaviour
{
    [Tooltip("Transform of player ship.")]
    [SerializeField]
    private Transform player;

    [Tooltip("Position offset from Player ship.")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 10f, -70f);

    [Tooltip("Angle offset from player ship.")]
    [SerializeField] private Vector3 angleOffset = new Vector3(0f, 80f, -90f);

    [Tooltip("Damping factor to smooth the changes in camera position.")]
    [SerializeField] private float motionDamping = 5f;

    [Tooltip("Damping factor to smooth the changes in camera position.")]
    [SerializeField] private float rotationDamping = 1.5f;

    void LateUpdate()
    {
        // apply initialrotation
        Quaternion initialRotation = Quaternion.Euler(angleOffset);

        Quaternion rot = Quaternion
            .Lerp(transform.rotation, player.rotation * initialRotation, Time.deltaTime * rotationDamping);

        transform.rotation = rot;
        
        // calc transformed axes
        Vector3 forward = transform.rotation * Vector3.forward;
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 up = transform.rotation * Vector3.up;

        // calc coordinate from offset
        Vector3 targetPosition = player.position;
        Vector3 desiredPosition = targetPosition
                                    + forward * positionOffset.z
                                    + right * positionOffset.x
                                    + up * positionOffset.y;

        // change camera position
        Vector3 position = Vector3
            .Lerp(transform.position, desiredPosition, Time.deltaTime * motionDamping);

        transform.position = position;
    }
}
