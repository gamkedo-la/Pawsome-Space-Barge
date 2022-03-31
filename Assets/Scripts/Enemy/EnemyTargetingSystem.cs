using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetingSystem : MonoBehaviour
{
    private List<Transform> nodes;
    private int currentNode = 0, nextNode = 1;
    private float stunTimer = 0;
    private Vector2 previousBargePosition;
    private Vector2 target = Vector2.zero;
    private Vector2 nextTarget = Vector2.zero;
    private bool targetLocked = false;
    private GameObject barge;



    [Header("Barge Detection Settings")]
    [SerializeField][Tooltip("Track the barge?")]
    private bool trackBarge = true;

    [SerializeField][Tooltip("Maximum range at which the barge can be detected")]
    [Min(0f)] private float bargeDetectionRange = 2000f;

    [SerializeField][Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [Min(0f)] private float bargeContactRange = 50f;

    [SerializeField][Tooltip("Required time to complete a scan for the barge")]
    [Min(0.1f)] private float scanInterval = 3f;



    [Header("Path Follow Settings")]
    [SerializeField][Tooltip("Path of nodes to follow")]
    private GameObject path;



    [Header("Waypoint Threshold")]
    [Tooltip("How close is close enough?")]
    [SerializeField] private float waypointThreshold = 30;



    [Header("Stun Timer Settings")]
    [SerializeField][Tooltip("Stun time, in seconds.")]
    [Min(0)] private float asteroidStunTime = 7f;

    [SerializeField][Tooltip("Stun time, in seconds.")]
    [Min(0)] private float playerStunTime = 0f;



    // *********************** Accessors ***********************
    /// <summary> Targeting system status check. </summary>
    public bool Online => stunTimer <= 0;

    /// <summary> Target lock check. </summary>
    public bool TargetLocked => targetLocked;



    private void Awake()
    {
        barge = GameObject.FindGameObjectWithTag("Barge");

        if (!trackBarge && path == null) trackBarge = true;
    }


    void Start()
    {
        if (path != null)
        {
            Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
            nodes = new List<Transform>();

            foreach (Transform node in pathTransforms)
            {
                if (node != path.transform || pathTransforms.Length == 1)
                {
                    nodes.Add(node);
                    Debug.Log($"Added node waypoint: {node.gameObject.transform.position}");
                }
            }
        }

        // begin barge range check
        StartCoroutine(ScanForBarge());
    }


    private void Update()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                // check barge position
                // set bool for animator if out-of-range
                if ( !IsBargeInRange() )
                {
                    targetLocked = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // draw detection radius
        Gizmos.DrawWireSphere(transform.position, bargeDetectionRange);
    }


    // TODO:
    // should return nearest point on nearest path
    // to be called when entering patrol state
    public (Vector2, Vector2) NearestPathPoint(Vector2 loctaion)
    {
        target = Vector2.zero;
        nextTarget = Vector2.zero;

        return (target, nextTarget);
    }


    /// <summary>
    /// Tracks path waypoints.
    /// Returns targets for current waypoint on path and next waypoint on path.
    /// Probably does not need to give the next target... but that'd be a refactor task.
    /// </summary>
    /// <returns>(target, nextTarget)</returns>
    public (Vector2, Vector2) TrackPathTwoTarget()
    {
        target = Vector2.zero;
        nextTarget = Vector2.zero;
        
        // if close enough switch node target
        if (Vector2.Distance(transform.position, nodes[currentNode].position) < waypointThreshold)
        {
            currentNode++;

            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }

            nextNode = currentNode + 1;

            if (nextNode >= nodes.Count)
            {
                nextNode = 0;
            }
        }

        // define targets
        target = nodes[currentNode].position;
        nextTarget = nodes[nextNode].position;

        return (target, nextTarget);
    }


    /// <summary>
    /// Tracks barge.
    /// Returns targets for current barge position and
    /// future barge position based on previous position.
    /// </summary>
    /// <returns>(target, nextTarget)</returns>
    public (Vector2, Vector2) TrackBargeTwoTarget()
    {
        target = Vector2.zero;
        nextTarget = Vector2.zero;

        if (previousBargePosition == null)
        {
            previousBargePosition = barge.transform.position;    
        }

        if (stunTimer <= 0)
        {
            target = barge.transform.position;

            Vector2 bargeVelocity = previousBargePosition - (Vector2)barge.transform.position;

            nextTarget = (Vector2)barge.transform.position - bargeVelocity * 100;

            previousBargePosition = barge.transform.position;
        }

            

        return (target, nextTarget);
    }


    /// <summary>
    /// Tracks path waypoints.
    /// Returns target for current waypoint on path.
    /// </summary>
    /// <returns></returns>
    public Vector2 TrackPath()
    {
        target = Vector2.zero;
        
        // if close enough switch node target
        if (Vector2.Distance(transform.position, nodes[currentNode].position) < waypointThreshold)
        {
            currentNode++;

            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }
        }

        // define target
        target = nodes[currentNode].position;

        return target;
    }


    /// <summary>
    /// Tracks barge.
    /// Returns target for current barge position.
    /// </summary>
    /// <returns></returns>
    public Vector2 TrackBarge()
    {
        target = Vector2.zero;

        if (previousBargePosition == null)
        {
            previousBargePosition = barge.transform.position;    
        }

        if (stunTimer <= 0)
        {
            target = barge.transform.position;
        }

        return target;
    }


    /// <summary>
    /// Returns true if barge is in contact range.
    /// </summary>
    /// <returns></returns>
    public bool IsBargeContact()
    {
        return Vector3.Distance(transform.position, barge.transform.position) <= bargeContactRange;
    }


    /// <summary>
    /// Returns true if barge is in detection range.
    /// </summary>
    /// <returns></returns>
    private bool IsBargeInRange()
    {
        return Vector3.Distance(transform.position, barge.transform.position) <= bargeDetectionRange;
    }


    /// <summary>
    /// Periodically checks distance to barge.
    /// 
    /// Toggles targetLocked; true if barge is in detection range.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScanForBarge()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);
            
            if ( IsBargeInRange() )
            {
                targetLocked = true;
            }
            else
            {
                targetLocked = false;
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (stunTimer <= 0)
        {
            if (other.gameObject.CompareTag("Asteroid"))
            {
                stunTimer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                stunTimer = playerStunTime;
            }
        }
    }
}
