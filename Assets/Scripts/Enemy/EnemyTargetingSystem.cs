using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetingSystem : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private AlertEvent callForHelp;
    private GameObject barge;
    private int layerMask;
    private List<List<Transform>> orbitalPathsNodes;
    private int currentPath, currentNode, nextNode;
    private bool bargeContact = false;

    [ReadOnly][SerializeField] private float stunTimer = 0;
    [ReadOnly][SerializeField] private Vector2 previousBargePosition;
    [ReadOnly][SerializeField] private Vector2 target = Vector2.zero;
    [ReadOnly][SerializeField] private Vector2 nextTarget = Vector2.zero;
    [ReadOnly][SerializeField] private bool targetLocked = false;



    [Header("Barge Detection Settings")]

    [SerializeField][Tooltip("Maximum range at which the barge can be detected")]
    [Min(0f)] private float bargeDetectionRange = 2000f;

    [SerializeField][Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [Min(0f)] private float bargeContactRange = 50f;

    [SerializeField][Tooltip("Required time to complete a scan for the barge")]
    [Min(0.1f)] private float scanInterval = 3f;



    [Header("Waypoint Settings")]
    [SerializeField][Tooltip("How close is close enough?")]
    private float waypointThreshold = 30;



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



    /// <summary>
    /// Example reciever method.
    /// </summary>
    /// <param name="position"></param>
    public void recieveAlert(Vector2 position)
    {
        Debug.Log(position.ToString());
    }


    /// <summary>
    /// Example notification broadcast.
    /// </summary>
    /// <param name="position"></param>
    public void sendAlert(Vector2 position)
    {
        callForHelp.TriggerEvent(position);
    }



    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();
        barge = GameObject.FindGameObjectWithTag("Barge");
        layerMask = LayerMask.GetMask("Waypoints");
    }


    private void Start()
    {
        orbitalPathsNodes = new List<List<Transform>>();

        GameObject[] existingOrbitalPaths = GameObject.FindGameObjectsWithTag("OrbitalPath");

        for (int x = 0; x < existingOrbitalPaths.Length; x++)
        {
            Transform[] pathTransforms = existingOrbitalPaths[x].GetComponentsInChildren<Transform>();

            var tempList = new List<Transform>();

            foreach (Transform node in pathTransforms)
            {
                if (node != existingOrbitalPaths[x].transform)
                {
                    tempList.Add(node);
                    // Debug.Log($"Added node waypoint: {node.gameObject.transform.position}");
                }
            }

            orbitalPathsNodes.Add(tempList);
        }

        // setup alert network
        callForHelp = enemyAI.EventNetwork.alertEvent;

        // begin barge range check
        StartCoroutine(ScanForBarge());

        // event system test
        sendAlert(transform.position);
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


    /// <summary>
    /// Return nearest point on nearest path.
    /// Send ship to next node to maintain counter-clockwise patrol.
    /// </summary>
    /// <param name="loctaion"></param>
    /// <returns></returns>
    public Vector2 NearestPathPoint(Vector2 loctaion)
    {
        target = Vector2.zero;

        // collider check for waypoints
        Collider2D[] hits;
        hits = Physics2D.OverlapCircleAll(transform.position, 2000f, layerMask);

        GameObject closestWaypoint = null;
        float closestDistance = Mathf.Infinity;

        // search for closest hit
        foreach (Collider2D waypoint in hits)
        {
            Vector3 directionToTarget = waypoint.transform.position - transform.position;
            float distanceSqr = directionToTarget.sqrMagnitude;
            if (distanceSqr < closestDistance)
            {
                closestDistance = distanceSqr;
                closestWaypoint = waypoint.gameObject;
            }
        }

        // which path?
        foreach (var thing in orbitalPathsNodes)
        {
            if (thing.Exists(e => e.transform == closestWaypoint.transform))
            {
                currentPath = orbitalPathsNodes.IndexOf(thing);
                // Debug.Log($"current path: { currentPath }");
            }
        }

        // find index in path
        currentNode = orbitalPathsNodes[currentPath].IndexOf(closestWaypoint.transform);

        // get index of next node
        int nextNode = currentNode >= orbitalPathsNodes[currentPath].Count - 1 ? 0 : currentNode + 1;

        // check dot product, head to next waypoint counter clockwise
        Vector2 vectorToNextWaypoint =
            orbitalPathsNodes[currentPath][nextNode].position - orbitalPathsNodes[currentPath][currentNode].position;
        Vector2 vectorToShip =
            transform.position - orbitalPathsNodes[currentPath][currentNode].position;

        float dot = Vector2.Dot(vectorToNextWaypoint, vectorToShip);

        if (dot >= 0) { currentNode = nextNode; }

        // finally set target
        target = orbitalPathsNodes[currentPath][currentNode].transform.position;

        return target;
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
        if (Vector2.Distance(transform.position, orbitalPathsNodes[currentPath][currentNode].transform.position) < waypointThreshold)
        {
            currentNode++;

            if (currentNode >= orbitalPathsNodes[currentPath].Count)
            {
                currentNode = 0;
            }
        }

        // define target
        target = orbitalPathsNodes[currentPath][currentNode].position;

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

        if (stunTimer <= 0)
        {
            target = barge.transform.position;
        }

        return AdjustedBarge(target);
    }


    /// <summary>
    /// Return position adjusted by bargeVelocity.
    /// Helps AI end up behind barge by aiming for point just behind.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private Vector2 AdjustedBarge(Vector2 position)
    {
        return target - GameManagement.Instance.bargeVelocity;
    }


    /// <summary>
    /// Returns true if barge is in contact range.
    /// Allows margin of error before leaving Contact mode.
    /// </summary>
    /// <returns></returns>
    public bool IsBargeContact()
    {
        bool inRange = false;

        if (Vector3.Distance(transform.position, AdjustedBarge(barge.transform.position)) <= bargeContactRange && !bargeContact)
        {
            inRange = true;
            bargeContact = true;
        }
        if (Vector3.Distance(transform.position, AdjustedBarge(barge.transform.position)) <= bargeContactRange*2 && bargeContact)
        {
            inRange = true;
        }

        if (!inRange) bargeContact = false;

        return inRange;
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
            if (other.gameObject.CompareTag("Asteroid") && !enemyAI.Engines.Online)
            {
                stunTimer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                stunTimer = playerStunTime;
            }
        }
    }



    // ******************************** CRUFT ********************************

    // /// <summary>
    // /// Tracks path waypoints.
    // /// Returns targets for current waypoint on path and next waypoint on path.
    // /// Probably does not need to give the next target... but that'd be a refactor task.
    // /// </summary>
    // /// <returns>(target, nextTarget)</returns>
    // public (Vector2, Vector2) TrackPathTwoTarget()
    // {
    //     target = Vector2.zero;
    //     nextTarget = Vector2.zero;
        
    //     // if close enough switch node target
    //     if (Vector2.Distance(transform.position, nodes[currentNode].position) < waypointThreshold)
    //     {
    //         currentNode++;

    //         if (currentNode >= nodes.Count)
    //         {
    //             currentNode = 0;
    //         }

    //         nextNode = currentNode + 1;

    //         if (nextNode >= nodes.Count)
    //         {
    //             nextNode = 0;
    //         }
    //     }

    //     // define targets
    //     target = nodes[currentNode].position;
    //     nextTarget = nodes[nextNode].position;

    //     return (target, nextTarget);
    // }


    // /// <summary>
    // /// Tracks barge.
    // /// Returns targets for current barge position and
    // /// future barge position based on previous position.
    // /// </summary>
    // /// <returns>(target, nextTarget)</returns>
    // public (Vector2, Vector2) TrackBargeTwoTarget()
    // {
    //     target = Vector2.zero;
    //     nextTarget = Vector2.zero;

    //     if (previousBargePosition == null)
    //     {
    //         previousBargePosition = barge.transform.position;    
    //     }

    //     if (stunTimer <= 0)
    //     {
    //         target = barge.transform.position;

    //         Vector2 bargeVelocity = previousBargePosition - (Vector2)barge.transform.position;

    //         nextTarget = (Vector2)barge.transform.position - bargeVelocity * 100;

    //         previousBargePosition = barge.transform.position;
    //     }

            

    //     return (target, nextTarget);
    // }
}
