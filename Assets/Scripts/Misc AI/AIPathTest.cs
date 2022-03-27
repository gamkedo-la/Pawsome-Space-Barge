using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    [SerializeField][Tooltip("Track the barge?")]
    private bool trackBarge = false;

    private Vector2 previousBargePosition;

    [SerializeField] [Tooltip("Barge")]
    private Transform barge;

    [SerializeField] [Tooltip("Path of nodes to follow")]
    private GameObject path;

    [SerializeField] [Tooltip("How close is close enough?")]
    private float positionThreshold = 30;

    private List<Transform> nodes;

    private int currentNode = 0, nextNode = 1;

    private EngineSystemTest engines;

    [Header("Sensors")]
    public int layerMask;
    public float sensorLength = 20;
    public float frontSensorPosition = 9f;
    public float frontSideSensorPosition = 5f;
    public float frontSensorAngle = 20f;


    private void Awake()
    {
        engines = GetComponent<EngineSystemTest>();
        Application.targetFrameRate = 60;
        layerMask = LayerMask.GetMask("Obstacle");
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


    private void Sensors()
    {
        RaycastHit2D hit;
        Vector2 sensorStartPos = transform.position;
        Vector2 frontSensorStart = sensorStartPos + (Vector2)transform.right * frontSensorPosition;
        Vector2 leftSensorStart = frontSensorStart + (Vector2)transform.up * frontSideSensorPosition;
        Vector2 rightSensorStart = frontSensorStart - (Vector2)transform.up * frontSideSensorPosition;

        Quaternion angle;



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

        // front left half angled sensor
        angle = Quaternion.AngleAxis(frontSensorAngle*0.5f, transform.forward);
        hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, sensorLength*0.75f, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.75f), Color.white);
        }

        // front left angled sensor
        angle = Quaternion.AngleAxis(frontSensorAngle, transform.forward);
        hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, sensorLength*0.5f, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.5f), Color.white);
        }

        // front left 1.5-angled sensor
        angle = Quaternion.AngleAxis(frontSensorAngle*1.5f, transform.forward);
        hit = Physics2D.Raycast(leftSensorStart, angle * transform.right, sensorLength*0.25f, layerMask);
        if (hit)
        {
            Debug.DrawLine(leftSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(leftSensorStart, leftSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.25f), Color.white);
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

        // front right half angled sensor
        angle = Quaternion.AngleAxis(-frontSensorAngle/2, transform.forward);
        hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, sensorLength*0.75f, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.75f), Color.white);
        }

        // front right angled sensor
        angle = Quaternion.AngleAxis(-frontSensorAngle, transform.forward);
        hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, sensorLength*0.5f, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.5f), Color.white);
        }

        // front right 1.5-angled sensor
        angle = Quaternion.AngleAxis(-frontSensorAngle*1.5f, transform.forward);
        hit = Physics2D.Raycast(rightSensorStart, angle * transform.right, sensorLength*0.25f, layerMask);
        if (hit)
        {
            Debug.DrawLine(rightSensorStart, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(rightSensorStart, rightSensorStart + (Vector2)(angle * transform.right) * (sensorLength*0.25f), Color.white);
        }
    }


    private void FixedUpdate()
    {
        Vector2 target = Vector2.zero, nextTarget = Vector2.zero;

        Sensors();

        if (!trackBarge)
        {
            // if close enough switch node target
            if (Vector2.Distance(transform.position, nodes[currentNode].position) < positionThreshold)
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

            // define target and sent to steering function
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
