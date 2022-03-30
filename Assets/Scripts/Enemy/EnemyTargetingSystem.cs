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

    private float timer = 0;
    public bool Status => timer <= 0;


    private Vector2 target = Vector2.zero;
    public Vector2 Target => target;
    private Vector2 nextTarget = Vector2.zero;
    public Vector2 NextTarget => nextTarget;


    [Header("Waypoint Threshold")]
    [Tooltip("How close is close enough?")]
    [SerializeField] private float waypointThreshold = 30;


    [Header("Barge Tracking Settings")]
    [Tooltip("Track the barge?")]
    [SerializeField] private bool trackBarge = true;

    private Vector2 previousBargePosition;

    [Tooltip("Barge object to track.")]
    [SerializeField] private GameObject barge;
    public GameObject Barge => barge;


    [Header("Path Follow Settings")]
    [Tooltip("Path of nodes to follow")]
    [SerializeField] private GameObject path;
    private List<Transform> nodes;
    private int currentNode = 0, nextNode = 1;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

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

        // previousBargePosition = barge.transform.position;

        StartCoroutine(ScanForBarge());
    }


    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, bargeDetectionRange);
    }
    
    
    public (Vector2, Vector2) AquireTarget()
    {
        target = Vector2.zero;
        nextTarget = Vector2.zero;

        if (!trackBarge && path != null)
        {
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
        }
        else if (barge != null)
        {
            if (previousBargePosition == null)
            {
                previousBargePosition = barge.transform.position;    
            }

            target = barge.transform.position;

            Vector2 bargeVelocity = previousBargePosition - (Vector2)barge.transform.position;

            nextTarget = (Vector2)barge.transform.position - bargeVelocity * 100;

            previousBargePosition = barge.transform.position;
        }

        return (target, nextTarget);
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
            
            if (
                Vector3.Distance(transform.position, barge.transform.position) > bargeDetectionRange
                || timer > 0
            )
            {
                barge = null;
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (timer <= 0)
        {
            if (other.gameObject.CompareTag("Asteroid"))
            {
                timer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                timer = playerStunTime;
            }
        }
    }
}
