using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    // engine script reference
    private EngineSystemTest engines;

    /// <summary> Known / planned scan modes. </summary>
    private enum ScanType { Whiskers, Radar, CircleCast, Network }


    [Tooltip("How close is close enough?")]
    [SerializeField] private float waypointThreshold = 30;


    [Header("Barge Tracking Settings")]
    [Tooltip("Track the barge?")]
    [SerializeField] private bool trackBarge = false;

    private Vector2 previousBargePosition;

    [Tooltip("Barge object to track.")]
    [SerializeField] private Transform barge;


    [Header("Path Follow Settings")]
    [Tooltip("Path of nodes to follow")]
    [SerializeField] private GameObject path;
    private List<Transform> nodes;
    private int currentNode = 0, nextNode = 1;


    [Header("Sensors")]
    [Tooltip("Raycast layer.")]
    [SerializeField] public LayerMask collisionMask;
    private int layerMask;


    [Tooltip("Scanning method.")]
    [SerializeField] private ScanType scanType = ScanType.Whiskers;

    [Tooltip(" How far to scan from ship.")]
    [SerializeField] private float sensorLength = 100;
    [SerializeField] private float frontSensorPositionOffset = 9f;
    [SerializeField] private float frontSideSensorOffset = 5f;
    [SerializeField] private int whiskersCount = 4;
    [SerializeField] private float whiskerAngle = 15;

    [Tooltip("Arc in degrees to scan for obstacles,\nin degrees, centered on ship forward.")]
    [SerializeField][Range(0,360)] private float radarScanRadius = 180;

    /// <summary> Returns scan radius normalized between -1 and 1 </summary>
    private float ScanRadius => 1 - radarScanRadius/180;


    private void Awake()
    {
        engines = GetComponent<EngineSystemTest>();
        Application.targetFrameRate = 60;
        layerMask = collisionMask.value;
    }


    private void Start()
    {
        if (!trackBarge)
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
        else
        {
            previousBargePosition = barge.position;
        }
    }


    private void FixedUpdate()
    {
        Vector2 target = Vector2.zero;
        Vector2 nextTarget = Vector2.zero;

        Sensors();

        if (!trackBarge)
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
        else
        {
            target = barge.position;

            Vector2 bargeVelocity = previousBargePosition - (Vector2)barge.position;

            nextTarget = (Vector2)barge.position - bargeVelocity * 100;

            previousBargePosition = barge.position;
        }

        #if UNITY_EDITOR
            // draw debug visualization
            Debug.DrawLine(target, transform.position, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + (Vector3)engines.Velocity, Color.magenta);
            Debug.DrawLine(nextTarget, transform.position, Color.green);
        #endif

        ApplySteer(target, nextTarget);
    }


    /// <summary>
    /// Steers towards target. Considers heading, velocity drift, and next waypoint location.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="nextTarget"></param>
    private void ApplySteer(Vector2 target, Vector2 nextTarget)
    {
        // steering vectors array
        Vector2[] vectors = new Vector2[3];


        // vectors[0] -> vector to target
        vectors[0] = transform.InverseTransformPoint(target);
        float newSteer = vectors[0].y / vectors[0].magnitude;


        // vectors[1] -> velocity vector
        vectors[1] = transform.InverseTransformPoint((Vector2)transform.position + engines.Velocity);
        float velocitySteer = vectors[1].magnitude == 0 ? 0 : vectors[1].y / vectors[1].magnitude;


        // vectors[2] -> vector to next target
        // zero parameters
        vectors[2] = Vector2.zero;
        float nextTargetSteer = 0;

        // if close to current target, calculate values
        if (Vector2.Distance(transform.position, target) < vectors[1].magnitude)
        {
            // vector to next target
            vectors[2] = transform.InverseTransformPoint(nextTarget);
            nextTargetSteer = vectors[2].y / vectors[2].magnitude;
        }


        // get vector weights
        float[] vectorWeights = WeightVectors(vectors);

        // apply steering
        engines.TurnTowardsTarget((newSteer * vectorWeights[0]) - (velocitySteer * vectorWeights[1]) + (nextTargetSteer * vectorWeights[2]));
        

        // calculate desired heading
        Vector2 headingToTarget = (target - (Vector2)transform.position).normalized;
        float headingDifference = Mathf.Abs(Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward));

        // scale thrust
        // when facing away from target thrust is 0
        float thrustWeight = Mathf.Abs((headingDifference/180) - 1);

        // if close to target, slow down
        if (engines.Velocity.magnitude / vectors[0].magnitude > 1)
        {
            thrustWeight *= -1;
        }


        // apply thrust
        engines.MoveForward(thrustWeight);
    }


    /// <summary>
    /// Executes desired sensor method. See: scanType
    /// Checks forward sensors, then checks choosen scan type.
    /// </summary>
    private void Sensors()
    {
        // sensor locations
        Vector2 sensorStartPos = transform.position;
        Vector2 frontSensorStart = sensorStartPos + (Vector2)transform.right * frontSensorPositionOffset;
        Vector2 leftSensorStart = frontSensorStart + (Vector2)transform.up * frontSideSensorOffset;
        Vector2 rightSensorStart = frontSensorStart - (Vector2)transform.up * frontSideSensorOffset;

        // we always need forward sensors
        ForwardSensors(frontSensorStart, leftSensorStart, rightSensorStart);

        // supplementary sensor types
        switch (scanType)
        {
            case ScanType.Whiskers:
                {
                    WhiskerScan(leftSensorStart, rightSensorStart);
                }
                break;
            case ScanType.Radar:
                {
                    RadarScan(frontSensorStart);
                }
                break;

            case ScanType.CircleCast:
            case ScanType.Network:
            default:
                Debug.Log("Scan mode not implemented");
                break;
        }
    }


    /// <summary>
    /// Front pointing sensors.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private void ForwardSensors(Vector2 frontSensorStart, Vector2 leftSensorStart, Vector2 rightSensorStart)
    {
        RaycastHit2D hit;

        // front centre sensor
        hit = Physics2D.Raycast(frontSensorStart, transform.right, sensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(frontSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(frontSensorStart, frontSensorStart + (Vector2)transform.right * sensorLength, Color.white);
        }

        // front left sensor
        hit = Physics2D.Raycast(leftSensorStart, transform.right, sensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)transform.right * sensorLength, Color.white);
        }

        // front right sensor
        hit = Physics2D.Raycast(rightSensorStart, transform.right, sensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)transform.right * sensorLength, Color.white);
        }
    }


    /// <summary>
    /// Scans a radius in front of ship for obstacles.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    private void RadarScan(Vector2 frontSensorStart)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(frontSensorStart, sensorLength *2, layerMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toTarget = (hits[i].transform.position - transform.position).normalized;
            float dot = Vector2.Dot(transform.right.normalized, toTarget);
            if (dot > ScanRadius)
            {
                Debug.DrawLine(frontSensorStart, hits[i].ClosestPoint(transform.position) , Color.red);
            }
        }
    }


    /// <summary>
    /// 'Whiskers' scanning method.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private void WhiskerScan(Vector2 leftSensorStart, Vector2 rightSensorStart)
    {
        RaycastHit2D hit;
        Quaternion angle;


        // left sensors
        for (int i = 1; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(whiskerAngle * i, transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount;
            hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, sensorLength*length, layerMask);
            if (hit)
            {
                Debug.DrawLine(leftSensorStart, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * transform.right) * (sensorLength*length), Color.white);
            }
        }


        // right sensors
        for (int i = 1; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(-whiskerAngle * i, transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount;
            hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, sensorLength*length, layerMask);
            if (hit)
            {
                Debug.DrawLine(rightSensorStart, hit.point, Color.red);
            }
            else
            {
                Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * transform.right) * (sensorLength*length), Color.white);
            }
        }
    }


    /// <summary>
    /// Weights vectors by magnitude, returns float values.
    /// [0] -> vector to target,
    /// [1] -> velocity vector,
    /// [2] -> vector to next target
    /// </summary>
    /// <param name="vectors"></param>
    /// <returns></returns>
    private float[] WeightVectors(Vector2[] vectors)
    {
        float[] vectorWeights = new float[vectors.Length];
        float totalWeight = 0;

        foreach (Vector2 vector in vectors)
        {
            totalWeight += vector.magnitude;
        } 

        for (int i = 0; i < vectors.Length; i++)
        {
            vectorWeights[i] = vectors[i].magnitude / totalWeight;
        }

        return vectorWeights;
    }
}
