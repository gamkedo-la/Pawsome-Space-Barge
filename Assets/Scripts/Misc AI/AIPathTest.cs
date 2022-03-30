using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements system from tutorial series by EYEmaginary:
/// https://youtube.com/playlist?list=PLB9LefPJI-5wH5VdLFPkWfnPjeI6OSys1
/// 
/// Moddified to account for spaceship drift in a space flight context,
/// and connections to other in game scripts.
/// 
/// Also includes extra things I tried along the way.
/// </summary>
public class AIPathTest : MonoBehaviour
{
    private EngineSystemTest engines;

    [Header("Debug Data")]
    public float avoidanceAccumulator = 0;
    public float avoidanceMinimum = float.PositiveInfinity;
    public float avoidanceMaximum = float.NegativeInfinity;
    public int collisionCount = 0;


    /// <summary> Known / planned scan modes. </summary>
    private enum ScanType { Whiskers, ForwardPlus, Radar, Tutorial, CircleCast, Network }

    [Header("Waypoint Threshold")]
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
    [SerializeField] private float frontSideSensorOffset = 4f;
    [SerializeField] private int whiskersCount = 6;
    [SerializeField] private float whiskerAngle = 15;

    [Tooltip("Arc in degrees to scan for obstacles,\nin degrees, centered on ship forward.")]
    [SerializeField][Range(1,360)] private float radarScanRadius = 180;

    /// <summary> Returns scan radius normalized between -1 and 1 </summary>
    private float ScanRadius => 1 - radarScanRadius/180;

    [SerializeField] private bool velocityScan = true;
    private Vector2 heading => velocityScan ? engines.Velocity.normalized : transform.right;


    private void Awake()
    {
        engines = GetComponent<EngineSystemTest>();
        Application.targetFrameRate = 60;
        layerMask = collisionMask.value;
    }


    private void Start()
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

        previousBargePosition = barge.position;
    }


    private void FixedUpdate()
    {
        Vector2 target = Vector2.zero;
        Vector2 nextTarget = Vector2.zero;

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

        ApplySteer(target, nextTarget, Sensors());
    }


    /// <summary>
    /// Steers towards target. Considers heading, velocity drift, and next waypoint location.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="nextTarget"></param>
    private void ApplySteer(Vector2 target, Vector2 nextTarget, float sensorSteer)
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
        engines.TurnTowardsTarget((newSteer * vectorWeights[0]) - (velocitySteer * vectorWeights[1]) + (nextTargetSteer * vectorWeights[2]) + sensorSteer);
        

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
        engines.MoveForward(thrustWeight + Mathf.Abs(sensorSteer));
    }


    /// <summary>
    /// Executes desired sensor method. See: scanType
    /// Checks forward sensors, then checks choosen scan type.
    /// </summary>
    private float Sensors()
    {
        // sensor locations
        Vector2 sensorStartPos = transform.position;
        Vector2 frontSensorStart = sensorStartPos + (Vector2)transform.right * frontSensorPositionOffset;
        Vector2 leftSensorStart = frontSensorStart + (Vector2)transform.up * frontSideSensorOffset;
        Vector2 rightSensorStart = frontSensorStart - (Vector2)transform.up * frontSideSensorOffset;

        // obstacle avoidance steering amount
        float avoidSteering = 0;

        // sensor type switch
        switch (scanType)
        {
            case ScanType.Whiskers:
                {
                    avoidSteering += WhiskerScan(leftSensorStart, rightSensorStart);
                }
                break;

            case ScanType.ForwardPlus:
                {
                    avoidSteering += ForwardPlus(frontSensorStart, leftSensorStart, rightSensorStart);
                }
                break;

            case ScanType.Radar:
                {
                    // not actually implemented
                    avoidSteering += RadarScan(frontSensorStart);
                }
                break;

            case ScanType.Tutorial:
                {
                    avoidSteering += TutorialSensors(frontSensorStart, leftSensorStart, rightSensorStart);
                }
                break;

            case ScanType.CircleCast:
            case ScanType.Network:
            default:
                Debug.Log("Scan mode not implemented");
                scanType = ScanType.Whiskers;
                break;
        }

        if (avoidSteering == 0)
        {
            avoidanceAccumulator = 0;
        }
        else
        {
            avoidanceAccumulator += avoidSteering;
        }

        if (avoidanceAccumulator > avoidanceMaximum) { avoidanceMaximum = avoidanceAccumulator; }
        if (avoidanceAccumulator < avoidanceMinimum) { avoidanceMinimum = avoidanceAccumulator; }

        return avoidanceAccumulator;
    }


    /// <summary>
    /// Front pointing sensors, plus whiskers to side of detection side.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private float ForwardPlus(Vector2 frontSensorStart, Vector2 leftSensorStart, Vector2 rightSensorStart)
    {
        RaycastHit2D leftHit, rightHit;

        float avoidSteering = 0;
        bool applyBrakes = false;

        // front left sensor
        leftHit = Physics2D.Raycast(leftSensorStart, heading, sensorLength, layerMask);
        if (leftHit)
        {
            if (leftHit.transform.gameObject.CompareTag("Barge")) { return 0; }

            Debug.DrawLine(leftSensorStart, leftHit.point, Color.red);
            avoidSteering -= 1f;
            avoidSteering += WhiskerScanLeft(leftSensorStart)/4;

            if (leftHit.distance < engines.Velocity.magnitude)
            {
                applyBrakes = true;
            }
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + heading * sensorLength, Color.white);
        }

        // front right sensor
        rightHit = Physics2D.Raycast(rightSensorStart, heading, sensorLength, layerMask);
        if (rightHit)
        {
            if (rightHit.transform.gameObject.CompareTag("Barge")) { return 0; }

            Debug.DrawLine(rightSensorStart, rightHit.point, Color.red);
            avoidSteering += 1f;
            avoidSteering += WhiskerScanRight(rightSensorStart)/4;

            if (rightHit.distance < engines.Velocity.magnitude)
            {
                applyBrakes = true;
            }
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + heading * sensorLength, Color.white);
        }

        // TODO this seems wrong but does the trick, 
        if (leftHit && rightHit)
        {
            avoidSteering += WhiskerScanLeft(leftSensorStart, true)/4;
            avoidSteering += WhiskerScanRight(rightSensorStart, true)/4;
        }

        if (applyBrakes)
        {
            engines.ApplyBrakes();
        }

        return avoidSteering;
    }


    /// <summary>
    /// ScanType.Radar
    /// Scans a radius in front of ship for obstacles.
    /// Can scan from 1 to 360 degrees.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    private float RadarScan(Vector2 frontSensorStart)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(frontSensorStart, sensorLength *2, layerMask);
        float avoidSteering = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toTarget = (hits[i].transform.position - transform.position).normalized;
            float dot = Vector2.Dot(transform.right.normalized, toTarget);
            if (dot > ScanRadius)
            {
                Debug.DrawLine(frontSensorStart, hits[i].ClosestPoint(transform.position) , Color.red);

                // not quite sure how this scan method would be used just yet...
                // avoidSteering involves the dot product, distance to object, and ship velocity?
            }
        }

        return avoidSteering;
    }


    /// <summary>
    /// ScanType.Whiskers
    /// 'Whiskers' scanning method.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private float WhiskerScan(Vector2 leftSensorStart, Vector2 rightSensorStart)
    {
        RaycastHit2D hit;
        Quaternion angle;

        float avoidSteering = 0;
        bool applyBrakes = false;

        // left sensors
        for (int i = 0; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(whiskerAngle * i, transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount;
            hit = Physics2D.Raycast(leftSensorStart, angle * heading, sensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                avoidSteering -= (1f * length) / whiskersCount;

                if (hit.distance < engines.Velocity.magnitude)
                {
                    applyBrakes = true;
                }
            }
            else
            {
                Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * heading) * (sensorLength*length), Color.white);
                break;
            }
        }


        // right sensors
        for (int i = 0; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(-whiskerAngle * i, transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount;
            hit = Physics2D.Raycast(rightSensorStart, angle * heading, sensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                avoidSteering += (1f * length) / whiskersCount;

                if (hit.distance < engines.Velocity.magnitude)
                {
                    applyBrakes = true;
                }
            }
            else
            {
                Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * heading) * (sensorLength*length), Color.white);
                break;
            }
        }

        if (applyBrakes)
        {
            engines.ApplyBrakes();
        }

        return avoidSteering;
    }


    private float WhiskerScanRight(Vector2 rightSensorStart, bool fullScan=false)
    {
        RaycastHit2D hit;
        Quaternion angle;

        float avoidSteering = 0;
        bool applyBrakes = false;

        // right sensors
        for (int i = 0; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(-whiskerAngle * (i + 1), transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount * 0.75f;
            hit = Physics2D.Raycast(rightSensorStart, angle * heading, sensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                avoidSteering += (1f * length) / whiskersCount;

                if (hit.distance < engines.Velocity.magnitude)
                {
                    applyBrakes = true;
                }
            }
            else
            {
                Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * heading) * (sensorLength*length), Color.white);
                if (!fullScan) { break; }
            }
        }

        if (applyBrakes)
        {
            engines.ApplyBrakes();
        }

        return avoidSteering;
    }

    private float WhiskerScanLeft(Vector2 leftSensorStart, bool fullScan=false)
    {
        RaycastHit2D hit;
        Quaternion angle;

        float avoidSteering = 0;
        bool applyBrakes = false;

        // left sensors
        for (int i = 0; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(whiskerAngle * (i + 1), transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount * 0.75f;
            hit = Physics2D.Raycast(leftSensorStart, angle * heading, sensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                avoidSteering -= (1f * length) / whiskersCount;

                if (hit.distance < engines.Velocity.magnitude)
                {
                    applyBrakes = true;
                }
            }
            else
            {
                Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * heading) * (sensorLength*length), Color.white);
                if (!fullScan) { break; }
            }
        }

        if (applyBrakes)
        {
            engines.ApplyBrakes();
        }

        return avoidSteering;
    }


    /// <summary>
    /// ScanType.Tutorial
    /// Sensors as used in tutorial video.
    /// See: https://youtu.be/PiYffouHvuk
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private float TutorialSensors(Vector2 frontSensorStart, Vector2 leftSensorStart, Vector2 rightSensorStart)
    {
        RaycastHit2D hit;
        Quaternion angle;

        // bool avoiding = false;
        float avoidSteering = 0;

        // front right sensor
        hit = Physics2D.Raycast(rightSensorStart, transform.right, sensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
            avoidSteering += 1f;
        }
        else
        {
            // right whisker sensor
            angle = Quaternion.AngleAxis(-whiskerAngle, transform.forward);
            hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, sensorLength, layerMask);
            if (hit)
            {
                Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                avoidSteering += 0.5f;
            }
        }

        // front left sensor
        hit = Physics2D.Raycast(leftSensorStart, transform.right, sensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
            avoidSteering -= 1f;
        }
        else
        {
            // left whisker sensor
            angle = Quaternion.AngleAxis(whiskerAngle, transform.forward);
            hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, sensorLength, layerMask);
            if (hit)
            {
                Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                avoidSteering -= 0.5f;
            }
        }

        if (avoidSteering == 0)
        {
            // front centre sensor
            hit = Physics2D.Raycast(frontSensorStart, transform.right, sensorLength, layerMask);
            if (hit)
            {
                Debug.DrawLine(frontSensorStart, hit.point, Color.red);
                if (hit.normal.x > 0)
                {
                    avoidSteering -= 1f;
                }
                else
                {
                    avoidSteering += 1f;
                }
            }
        }

        return avoidSteering;
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


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Asteroid"))
        {
            collisionCount++;
            Debug.Log(collisionCount);
        }
    }
}
