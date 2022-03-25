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
            nextNode = currentNode + 1;

            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }

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


    private void ApplySteer(Vector2 target, Vector2 nextTarget)
    {
        // vector to target
        Vector2 relativeVector = transform.InverseTransformPoint(target);
        float newSteer = relativeVector.y / relativeVector.magnitude;

        // vector to next target
        Vector2 nextTargetVector = transform.InverseTransformPoint(nextTarget);
        float nextTargetSteer = nextTargetVector.y / nextTargetVector.magnitude;

        // velocity vector
        Vector2 velocityVector = transform.InverseTransformPoint((Vector2)transform.position + engines.Velocity);
        float velocitySteer = velocityVector.magnitude == 0 ? 0 : velocityVector.y / velocityVector.magnitude;

        // steering weighting
        // turn down relativeVector weight as target approaches
        // increase as target further away
        if (Vector2.Distance(transform.position, nodes[currentNode].position) < velocityVector.magnitude)
        {
            float totalMag = relativeVector.magnitude + velocityVector.magnitude + nextTargetVector.magnitude;
            float steeringWeight = relativeVector.magnitude / totalMag;
            float velocityWeight = velocityVector.magnitude / totalMag;
            float nextTargetWeight = nextTargetVector.magnitude / totalMag;

            engines.TurnTowardsTarget((newSteer * steeringWeight) + (nextTargetSteer * nextTargetWeight) - (velocitySteer * velocityWeight));
        }
        else
        {
            float totalMag = relativeVector.magnitude + velocityVector.magnitude;
            float steeringWeight = relativeVector.magnitude / totalMag;
            float velocityWeight = velocityVector.magnitude / totalMag;

            engines.TurnTowardsTarget((newSteer * steeringWeight) - (velocitySteer * velocityWeight));
        }
        


        // calculate desired heading
        Vector2 headingToTarget = (target - (Vector2)transform.position).normalized;
        float headingDifference = Mathf.Abs(Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward));

        // scale thrust
        // when facing away from target thrust is 0
        float thrustWeight = Mathf.Abs((headingDifference/180) - 1);

        if (engines.Velocity.magnitude / relativeVector.magnitude > 1)
        {
            thrustWeight *= -1;
        }


        engines.MoveForward(thrustWeight);
    }
}
