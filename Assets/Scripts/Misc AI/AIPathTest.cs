using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    public GameObject ship;
    public Transform path;

    [Range(0,1)]
    public float steeringThreshold = 0.15f;
    private List<Transform> nodes;

    private int currentNode = 0;

    private EngineSystemTest engines;
    public float newSteer = 0, velocitySteer = 0, headingDifference = 0, thing = 0;


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
        Debug.DrawLine(nodes[currentNode].position, transform.position, Color.red);

        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 30)
        {
            currentNode++;
            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }
        }

        Vector3 target = nodes[currentNode].position;

        ApplySteer(target);
    }


    private void ApplySteer(Vector3 target)
    {
        Vector3 velocity = new Vector3(engines.Velocity.x, engines.Velocity.y, 0);
        Debug.DrawLine(transform.position, transform.position + velocity, Color.magenta);

        Vector3 relativeVector = transform.InverseTransformPoint(target);
        newSteer = relativeVector.x / relativeVector.magnitude;

        Vector3 velocityVector = transform.InverseTransformPoint(transform.position + velocity);
        velocitySteer = velocityVector.magnitude == 0 ? 0 : velocityVector.x / velocityVector.magnitude;

        engines.TurnTowardsTarget(newSteer - velocitySteer);

        // thing = Vector3.Angle(target + transform.position, -transform.up + transform.position);

        // Debug.DrawLine(transform.position, transform.InverseTransformDirection(-transform.up) * 15f, Color.yellow);

        // if (newSteer < steeringThreshold && newSteer > -steeringThreshold)
        // {
        engines.MoveForward(relativeVector.magnitude);
        // }
    }
}
