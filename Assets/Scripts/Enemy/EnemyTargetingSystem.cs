using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetingSystem : MonoBehaviour
{
    [Tooltip("Maximum range at which the barge can be detected")]
    [SerializeField][Min(0f)] private float bargeDetectionRange = 500f;

    [Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [SerializeField] [Min(0f)] private float bargeContactRange = 20f;

    [Tooltip("Required time to complete a scan for the barge")]
    [SerializeField] [Min(0.1f)] private float scanInterval = 3f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float asteroidStunTime = 7f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float playerStunTime = 0f;

    private Rigidbody2D rb2d;

    private float stunTimer = 0;
    public bool Status => stunTimer <= 0;


    private Vector2 target = Vector2.zero;
    private Vector2 nextTarget = Vector2.zero;


    [Header("Waypoint Threshold")]
    [Tooltip("How close is close enough?")]
    [SerializeField] private float waypointThreshold = 30;


    [Header("Barge Tracking Settings")]
    [Tooltip("Track the barge?")]
    [SerializeField] private bool trackBarge = true;

    private Vector2 previousBargePosition;

    [Tooltip("Barge object to track.")]
    [SerializeField] private GameObject barge;
    private bool targetLocked = false;
    public bool TargetLocked => targetLocked;


    [Header("Path Follow Settings")]
    [Tooltip("Path of nodes to follow")]
    [SerializeField] private GameObject path;
    private List<Transform> nodes;
    private int currentNode = 0, nextNode = 1;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
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
        Gizmos.DrawWireSphere(transform.position, bargeDetectionRange);
    }
    
    // This logic feels error prone so I split it.
    // Do we need a generic method for this?

    // public (Vector2, Vector2) AquireTarget()
    // {
    //     target = Vector2.zero;
    //     nextTarget = Vector2.zero;

    //     if ( (!trackBarge && path != null) || ( trackBarge && stunTimer <= 0 ))
    //     {
    //         (target, nextTarget) = TrackPath();
    //     }
    //     else if (stunTimer <= 0)
    //     {
    //         (target, nextTarget) = TrackBarge();
    //     }

    //     return (target, nextTarget);
    // }


    // TODO:
    // should return nearest point on nearest path
    // to be called when entering patrol state
    public (Vector2, Vector2) NearestPathPoint(Vector2 loctaion)
    {
        target = Vector2.zero;
        nextTarget = Vector2.zero;

        return (target, nextTarget);
    }


    public (Vector2, Vector2) TrackPath()
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


    public (Vector2, Vector2) TrackBarge()
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


    public bool IsBargeContact()
    {
        return Vector3.Distance(transform.position, barge.transform.position) <= bargeContactRange;
    }


    private bool IsBargeInRange()
    {
        return Vector3.Distance(transform.position, barge.transform.position) <= bargeDetectionRange;
    }


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
