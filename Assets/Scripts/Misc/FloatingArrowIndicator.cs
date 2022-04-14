using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingArrowIndicator : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(ShipMovement.instance == null){
            return;
        }
        Vector3 cameraLoc = ShipMovement.instance.transform.position;
        cameraLoc.y = transform.position.y; // keeping icon on same plane so it won't tilt
        transform.LookAt(cameraLoc, ShipMovement.instance.transform.up);
        // transform.rotation *= Quaternion.AngleAxis(-90.0f,Vector3.up);
    }
}
