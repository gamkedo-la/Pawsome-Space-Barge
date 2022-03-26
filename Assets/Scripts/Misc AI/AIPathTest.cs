using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    [SerializeField] [Tooltip("Path of nodes to follow")]
    private GameObject path;

    [SerializeField] [Tooltip("How close is close enough?")]
    private float positionThreshold = 30;

    private List<Transform> nodes;

    private int currentNode = 0, nextNode = 1;

    private EngineSystemTest engines;


    private void Awake()
    {
        engines = GetComponent<EngineSystemTest>();
        Application.targetFrameRate = 60;
    }


    private void Start()
    {
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        foreach (Transform node in pathTransforms)
        {
            if (node != path.transform)
            {
                nodes.Add(node);
                Debug.Log($"Added node waypoint: {node.gameObject.transform.position}");
            }
        }
    }


    private void FixedUpdate()
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
        Vector2 target = nodes[currentNode].position;
        Vector2 nextTarget = nodes[nextNode].position;

#if UNITY_EDITOR
        // draw debug visualization
        Debug.DrawLine(target, transform.position, Color.red);
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
        if (Vector2.Distance(transform.position, nodes[currentNode].position) < vectors[1].magnitude)
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
