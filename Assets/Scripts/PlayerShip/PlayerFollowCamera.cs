using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform player;

    // for Follow Camera
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 10f, -80f);
    [SerializeField] private Vector3 angleOffset = new Vector3(0f, 70f, -90f);

    [Tooltip("Damping factor to smooth the changes in camera position.")]
    [SerializeField] private float motionDamping = 10f;

    [Tooltip("Damping factor to smooth the changes in camera position.")]
    [SerializeField] private float rotationDamping = 0.6f;

    void LateUpdate()
    {
        // apply initialrotation
        Quaternion initialRotation = Quaternion.Euler(angleOffset);

        // if (allowRotationTracking)
        // {
            Quaternion rot = Quaternion.Lerp(
                transform.rotation,
                player.rotation * initialRotation,
                Time.deltaTime * rotationDamping
            );

            transform.rotation = rot;
        // }
        // else
        // {
        //     transform.rotation = Quaternion.RotateTowards(
        //         transform.rotation,
        //         initialRotation,
        //         mDamping * Time.deltaTime);
        // }
        

        // calc transformed axes
        Vector3 forward = transform.rotation * Vector3.forward;
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 up = transform.rotation * Vector3.up;

        // calc coordinate fram offset
        Vector3 targetPosition = player.position;
        Vector3 desiredPosition = targetPosition
            + forward * positionOffset.z
            + right * positionOffset.x
            + up * positionOffset.y;

        // change camera position
        Vector3 position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * motionDamping);
        transform.position = position;
    }
}
