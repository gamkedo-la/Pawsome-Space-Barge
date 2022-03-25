using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    [SerializeField] [Tooltip("Path of nodes to follow")]
    private GameObject path;

    [SerializeField] [Tooltip("How close is close enough?")]
    private float positionThreshold = 30;

    [SerializeField] [Range(0, 50)] [Tooltip("Maximum steering multiplier.")]
    private float maxSteer = 5;

    private List<Transform> nodes;

    private int currentNode = 0;

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
#if UNITY_EDITOR
        // draw debug visualization
        Debug.DrawLine(nodes[currentNode].position, transform.position, Color.red);
        Debug.DrawLine(transform.position, transform.position + (Vector3)engines.Velocity, Color.magenta);
#endif
        // if close enough switch node target
        if (Vector2.Distance(transform.position, nodes[currentNode].position) < positionThreshold)
        {
            currentNode++;
            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }
        }

        // define target and sent to steering function
        Vector2 target = nodes[currentNode].position;

        ApplySteer(target);
    }


    private void ApplySteer(Vector2 target)
    {
        // vector to target
        Vector2 relativeVector = transform.InverseTransformPoint(target);
        float newSteer = relativeVector.x / relativeVector.magnitude;

        // velocity vector
        Vector2 velocityVector = transform.InverseTransformPoint((Vector2)transform.position + engines.Velocity);
        float velocitySteer = velocityVector.magnitude == 0 ? 0 : velocityVector.x / velocityVector.magnitude;

        // steering weighting
        // turn down relativeVector weight as target approaches
        // increase as target further away
        float steeringWeight = engines.Velocity.magnitude == 0 ? 0 : relativeVector.magnitude / engines.Velocity.magnitude;
        steeringWeight = Mathf.Clamp(steeringWeight, 0f, maxSteer);


        engines.TurnTowardsTarget((newSteer * steeringWeight) - velocitySteer);

        // calculate desired heading
        Vector2 headingToTarget = (target - (Vector2)transform.position).normalized;
        float headingDifference = Mathf.Abs(Vector3.SignedAngle(-transform.up, headingToTarget, Vector3.forward));

        // scale thrust
        // when facing away from target thrust is 0
        float thrustWeight = Mathf.Abs((headingDifference/180) - 1);


        engines.MoveForward(thrustWeight);
    }
}
