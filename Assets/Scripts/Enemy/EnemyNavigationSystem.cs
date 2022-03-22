using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNavigationSystem : MonoBehaviour
{
    private GameObject barge;
    public GameObject Barge => barge;

    [Tooltip("Maximum range at which the barge can be detected")]
    [SerializeField][Min(0f)] private float bargeDetectionRange = 500f;

    [Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [SerializeField] [Min(0f)] private float bargeContactRange = 20f;

    [Tooltip("Required time to complete a scan for the barge")]
    [SerializeField] [Min(0.1f)] private float scanInterval = 3f;


    void Start()
    {
        StartCoroutine(ScanForBarge());
    }


    public bool IsBargeContact()
    {
        return barge != null && Vector3.Distance(transform.position, barge.transform.position) <= bargeContactRange;
    }


    private IEnumerator ScanForBarge()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);
            barge = GameObject.FindGameObjectWithTag("Barge");
            if (barge == null) continue;
            
            if (Vector3.Distance(transform.position, barge.transform.position) > bargeDetectionRange)
            {
                barge = null;
            }
        }
    }
}
