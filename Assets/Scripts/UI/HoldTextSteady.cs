using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldTextSteady : MonoBehaviour
{
    private Vector3 deliveryWindow => GameManagement.Instance.DeliveryWindow.transform.position;

    void LateUpdate()
    {
        Vector3 newPosition = new Vector3(deliveryWindow.x - 1500, deliveryWindow.y, deliveryWindow.z - 1500);
        transform.position = newPosition;
        transform.rotation = Quaternion.identity;
    }
}
