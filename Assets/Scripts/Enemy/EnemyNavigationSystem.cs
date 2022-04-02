using UnityEngine;

/// <summary>
/// Waypoint navigation and obstacle avoidance system.
/// </summary>
[RequireComponent(typeof(EnemyAIStateMachine))]
public class EnemyNavigationSystem : MonoBehaviour
{
    /// <summary> Known / planned scan modes. </summary>
    private enum ScanType { Whiskers, ForwardPlus, Radar, Tutorial, CircleCast, Network }

    private EnemyAIStateMachine enemyAI;

    [ReadOnly][SerializeField] private int layerMask;



    [Header("Sensors")]
    [SerializeField][Tooltip("Raycast layer.")]
    public LayerMask collisionMask;

    [SerializeField][Tooltip("Scan based towards velocity target")]
    private bool velocityScan = true;

    [SerializeField][Tooltip("Scanning method.")]
    private ScanType scanType = ScanType.Whiskers;

    [SerializeField][Tooltip("How far to scan from ship?")]
    private float sensorLength = 80;
    private float SensorLength => velocityScan ? enemyAI.Engines.Velocity.magnitude * 1.25f : sensorLength;

    [SerializeField][Tooltip("Steering gain factor (%)")]
    [Range(0,1)] private float steeringGain = 0.05f;

    [SerializeField][Tooltip("Steering decay factor (% / timeStep)")]
    [Range(0,1)] private float steeringDecay = 0.05f;

    [SerializeField][Tooltip("Fore / Aft offset for front sensor, from ship center.")]
    private float frontSensorForeAftOffset = 9f;

    [SerializeField][Tooltip("Fore / Aft offset for side sensors, from ship center.")]
     private float sideSensorForeAftOffset = 2;

    [SerializeField][Tooltip("Left / Right offset for side sensors, from ship center.")]
     private float sideSensorWidthOffset = 12f;



    [Header("Whisker Scan Settings")]
    [SerializeField][Tooltip("How many whiskers to iterate through.")]
     private int whiskersCount = 10;

    [SerializeField][Tooltip("Angle between whiskers.")]
     private float whiskerAngle = 10;



    [Header("Radar Scan Settings")]
    [Tooltip("Arc in degrees to scan for obstacles,\nin degrees, centered on ship forward.")]
    [SerializeField][Range(1,360)] private float radarScanWidth = 180;



    [Header("Debug Data")]
    [ReadOnly] public float avoidanceAccumulator = 0;
    [ReadOnly] public float avoidanceMinimum = float.PositiveInfinity;
    [ReadOnly] public float avoidanceMaximum = float.NegativeInfinity;
    [ReadOnly] public int collisionCount = 0;




    // *********************** Accessors ***********************
    /// <summary> Returns scan radius normalized between -1 and 1 </summary>
    private float RadarScanWidth => 1 - radarScanWidth/180;

    /// <summary> Returns direction determined by velocityScan bool. </summary>
    private Vector2 heading => velocityScan ? enemyAI.Engines.Velocity.normalized : transform.right;



    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();
        layerMask = collisionMask.value;
    }


    /// <summary>
    /// Steers towards target.
    /// Considers heading, velocity drift, single target, and queries obstacle avoidance.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="nextTarget"></param>
    public void NavigateToTarget(Vector2 target)
    {
        float sensorSteer = Sensors();

        // steering vectors array
        Vector2[] vectors = new Vector2[2];

        // vectors[0] -> vector to target
        vectors[0] = transform.InverseTransformPoint(target);
        float newSteer = vectors[0].y / vectors[0].magnitude;

        // vectors[1] -> velocity vector
        vectors[1] = transform.InverseTransformPoint((Vector2)transform.position + enemyAI.Engines.Velocity);
        float velocitySteer = vectors[1].magnitude == 0 ? 0 : vectors[1].y / vectors[1].magnitude;

        // get vector weights
        float[] vectorWeights = WeightVectors(vectors);

        // apply steering
        enemyAI.Engines.RotateShip((newSteer * vectorWeights[0]) - (velocitySteer * vectorWeights[1]) + sensorSteer);
        

        // calculate desired heading
        Vector2 headingToTarget = (target - (Vector2)transform.position).normalized;
        float headingDifference = Mathf.Abs(Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward));

        // scale thrust
        // when facing away from target thrust is 0
        float thrustWeight = Mathf.Abs((headingDifference/180) - 1);

        // if close to target, slow down
        if (enemyAI.Engines.Velocity.magnitude / vectors[0].magnitude > 1)
        {
            thrustWeight *= -1;
        }


        // apply thrust
        enemyAI.Engines.ApplyThrust(thrustWeight);
    }


    /// <summary>
    /// Steers towards target.
    /// Considers heading, velocity drift, target, next waypoint, and queries obstacle avoidance.
    /// 
    /// CURRENTLY UNUSED - USE SINGLE TARGET SIGNATURE VERSION
    /// </summary>
    /// <param name="target"></param>
    /// <param name="nextTarget"></param>
    public void ApplySteer(Vector2 target, Vector2 nextTarget)
    {
        float sensorSteer = Sensors();

        // steering vectors array
        Vector2[] vectors = new Vector2[3];


        // vectors[0] -> vector to target
        vectors[0] = transform.InverseTransformPoint(target);
        float newSteer = vectors[0].y / vectors[0].magnitude;


        // vectors[1] -> velocity vector
        vectors[1] = transform.InverseTransformPoint((Vector2)transform.position + enemyAI.Engines.Velocity);
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
        enemyAI.Engines.RotateShip((newSteer * vectorWeights[0]) - (velocitySteer * vectorWeights[1]) + (nextTargetSteer * vectorWeights[2]) + sensorSteer);
        

        // calculate desired heading
        Vector2 headingToTarget = (target - (Vector2)transform.position).normalized;
        float headingDifference = Mathf.Abs(Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward));

        // scale thrust
        // when facing away from target thrust is 0
        float thrustWeight = Mathf.Abs((headingDifference/180) - 1);

        // if close to target, slow down
        if (enemyAI.Engines.Velocity.magnitude / vectors[0].magnitude > 1)
        {
            thrustWeight *= -1;
        }


        // apply thrust
        enemyAI.Engines.ApplyThrust(thrustWeight);
    }


    /// <summary>
    /// Executes desired sensor method. See: scanType
    /// Checks forward sensors, then checks choosen scan type.
    /// </summary>
    private float Sensors()
    {
        // sensor locations
        Vector2 sensorStartPos = transform.position;

        //front sensor
        Vector2 frontSensorStart = sensorStartPos + heading * frontSensorForeAftOffset;

        // side sensors
        Vector2 sideSensorOffset = sensorStartPos + heading * sideSensorForeAftOffset;

        Vector2 shipLeft = Quaternion.AngleAxis(90, Vector3.forward) * heading;
        Vector2 leftSensorStart = sideSensorOffset + shipLeft * sideSensorWidthOffset;
        Vector2 rightSensorStart = sideSensorOffset - shipLeft * sideSensorWidthOffset;

        // obstacle avoidance steering amount
        float avoidSteering = 0;

        // sensor type switch
        switch (scanType)
        {
            case ScanType.Whiskers:
                {
                    avoidSteering += WhiskerScan(frontSensorStart, leftSensorStart, rightSensorStart);
                }
                break;

            case ScanType.ForwardPlus:
                {
                    avoidSteering += ForwardPlus(frontSensorStart, leftSensorStart, rightSensorStart);
                }
                break;

            case ScanType.Radar:
                {
                    // not actually implemented... but a pretty debug
                    avoidSteering += RadarScan(frontSensorStart);

                    // so:
                    avoidSteering += WhiskerScan(frontSensorStart, leftSensorStart, rightSensorStart);
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
            avoidanceAccumulator -= avoidanceAccumulator * steeringDecay;
        }
        else
        {
            avoidanceAccumulator += avoidSteering * steeringGain;
        }

        if (avoidanceAccumulator > avoidanceMaximum) { avoidanceMaximum = avoidanceAccumulator; }
        if (avoidanceAccumulator < avoidanceMinimum) { avoidanceMinimum = avoidanceAccumulator; }

        return avoidanceAccumulator;
    }


    /// <summary>
    /// Front pointing sensors, plus whiskers to side when hit detected.
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
        leftHit = Physics2D.Raycast(leftSensorStart, heading, SensorLength, layerMask);
        if (leftHit)
        {
            if (leftHit.transform.gameObject.CompareTag("Barge"))
            {
                if (leftHit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(leftSensorStart, leftHit.point, Color.red);
                    avoidSteering -= 0.5f;
                    avoidSteering += WhiskerScanLeft(leftSensorStart) / 2;

                    if (leftHit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + heading * SensorLength, Color.white);
        }

        // front right sensor
        rightHit = Physics2D.Raycast(rightSensorStart, heading, SensorLength, layerMask);
        if (rightHit)
        {
            if (rightHit.transform.gameObject.CompareTag("Barge"))
            {
                if (rightHit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(rightSensorStart, rightHit.point, Color.red);
                    avoidSteering += 0.5f;
                    avoidSteering += WhiskerScanRight(rightSensorStart) / 2;

                    if (rightHit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + heading * SensorLength, Color.white);
        }

        // TODO this seems wrong but does the trick, 
        if (leftHit && rightHit)
        {
            // panic and scan all whiskers in hope of resolution
            avoidSteering += WhiskerScanLeft(leftSensorStart, true)/4;
            avoidSteering += WhiskerScanRight(rightSensorStart, true)/4;
        }

        if (applyBrakes)
        {
            enemyAI.Engines.ApplyBrakes();
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(frontSensorStart, SensorLength *2, layerMask);
        float avoidSteering = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toTarget = (hits[i].transform.position - transform.position).normalized;
            float dot = Vector2.Dot(transform.right.normalized, toTarget);
            if (dot > RadarScanWidth)
            {
                Debug.DrawLine(frontSensorStart, hits[i].ClosestPoint(transform.position) , Color.red);

                // not quite sure how this scan method would be used just yet...
                // avoidSteering involves the dot product, distance to object, and ship velocity?
            }
        }

        return avoidSteering;
    }


    /// <summary>
    /// ScanType.Whiskers [default]
    /// Scans through whiskers starting with front pointing ones.
    /// When a hit is detected the whiskers are iterated through
    /// on that side until no whiskers hit an obstacle.
    /// 
    /// Produces steering that turns away from congested areas.
    /// </summary>
    /// <param name="frontSensorStart"></param>
    /// <param name="leftSensorStart"></param>
    /// <param name="rightSensorStart"></param>
    private float WhiskerScan(Vector2 frontSensorStart, Vector2 leftSensorStart, Vector2 rightSensorStart)
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
            hit = Physics2D.Raycast(leftSensorStart, angle * heading, SensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { break; }

                if (hit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                    avoidSteering -= (0.5f ) / (i + 1);

                    if (hit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
            else
            {
                Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * heading) * (SensorLength*length), Color.white);
                break;
            }
        }


        // right sensors
        for (int i = 0; i <= whiskersCount; i++)
        {
            angle = Quaternion.AngleAxis(-whiskerAngle * i, transform.forward);
            float length = ((float)whiskersCount - i) / (float)whiskersCount;
            hit = Physics2D.Raycast(rightSensorStart, angle * heading, SensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { break; }

                if (hit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                    avoidSteering += (0.5f ) / (i + 1);

                    if (hit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
            else
            {
                Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * heading) * (SensorLength*length), Color.white);
                break;
            }
        }

        // cast a center ray to check for small obstacles
        avoidSteering += FrontSensor(frontSensorStart);

        if (applyBrakes)
        {
            enemyAI.Engines.ApplyBrakes();
        }

        return avoidSteering/3;
    }


    private float FrontSensor(Vector2 frontSensorStart)
    {
        RaycastHit2D hit;

        float avoidSteering = 0;

        hit = Physics2D.Raycast(frontSensorStart, heading, SensorLength, layerMask);
        if (hit)
        {

            if (!hit.transform.gameObject.CompareTag("Barge"))
            {
                if (hit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(frontSensorStart, hit.point, Color.red);

                    // transform to local space
                    float hitY = transform.InverseTransformDirection(hit.normal).y;

                    // get absolute value, division by 0 check, normalize to get signed int
                    var absHitY = Mathf.Abs(hitY);
                    if (absHitY == 0) absHitY = 0.001f;
                    var normalizedHitY = hitY / absHitY;

                    // steering force greatest when centered on obstacle
                    avoidSteering += (1 - absHitY) * normalizedHitY;
                }
            }
        }
        else
        {
            Debug.DrawLine(frontSensorStart, frontSensorStart + (Vector2)(heading) * (SensorLength), Color.white);
        }

        return avoidSteering;
    }


    /// <summary>
    /// Scans through right side whiskers.
    /// If 'fullScan' all whiskers are scanned,
    /// else the scan stops when no hit detected.
    /// </summary>
    /// <param name="rightSensorStart"></param>
    /// <param name="fullScan"></param>
    /// <returns></returns>
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
            hit = Physics2D.Raycast(rightSensorStart, angle * heading, SensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                if (hit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                    avoidSteering += (0.5f ) / (i + 1);

                    if (hit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
            else
            {
                Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * heading) * (SensorLength*length), Color.white);
                if (!fullScan) { break; }
            }
        }

        if (applyBrakes)
        {
            enemyAI.Engines.ApplyBrakes();
        }

        return avoidSteering;
    }


    /// <summary>
    /// Scans through left side whiskers.
    /// If 'fullScan' all whiskers are scanned,
    /// else the scan stops when no hit detected.
    /// </summary>
    /// <param name="leftSensorStart"></param>
    /// <param name="fullScan"></param>
    /// <returns></returns>
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
            hit = Physics2D.Raycast(leftSensorStart, angle * heading, SensorLength*length, layerMask);
            if (hit)
            {
                if (hit.transform.gameObject.CompareTag("Barge")) { return 0; }

                if (hit.rigidbody.GetInstanceID() != enemyAI.Engines.rb2dID)
                {
                    Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                    avoidSteering -= (0.5f ) / (i + 1);

                    if (hit.distance < enemyAI.Engines.Velocity.magnitude)
                    {
                        applyBrakes = true;
                    }
                }
            }
            else
            {
                Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * heading) * (SensorLength*length), Color.white);
                if (!fullScan) { break; }
            }
        }

        if (applyBrakes)
        {
            enemyAI.Engines.ApplyBrakes();
        }

        return avoidSteering;
    }


    /// <summary>
    /// ScanType.Tutorial
    /// Sensors as used in tutorial video.
    /// See: https://youtu.be/PiYffouHvuk
    /// 
    /// Not really playable, but in fairness the tutorial is guiding a car not a spaceship.
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
        hit = Physics2D.Raycast(rightSensorStart, transform.right, SensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
            avoidSteering += 1f;
        }
        else
        {
            // right whisker sensor
            angle = Quaternion.AngleAxis(-whiskerAngle, transform.forward);
            hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, SensorLength, layerMask);
            if (hit)
            {
                Debug.DrawLine(rightSensorStart, hit.point, Color.red);
                avoidSteering += 0.5f;
            }
        }

        // front left sensor
        hit = Physics2D.Raycast(leftSensorStart, transform.right, SensorLength, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
            avoidSteering -= 1f;
        }
        else
        {
            // left whisker sensor
            angle = Quaternion.AngleAxis(whiskerAngle, transform.forward);
            hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, SensorLength, layerMask);
            if (hit)
            {
                Debug.DrawLine(leftSensorStart, hit.point, Color.red);
                avoidSteering -= 0.5f;
            }
        }

        if (avoidSteering == 0)
        {
            // front centre sensor
            hit = Physics2D.Raycast(frontSensorStart, transform.right, SensorLength, layerMask);
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
}
