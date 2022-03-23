using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathTest : MonoBehaviour
{
    public Transform path;
    public float turnSpeed;
    private List<Transform> nodes;

    private int currentNode = 0;

    private EnemyEngineSystem engines;


    private void Awake()
    {
        engines = GetComponent<EnemyEngineSystem>();
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
            }
        }
    }


    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 30)
        {
            currentNode++;
            if (currentNode >= nodes.Count)
            {
                currentNode = 0;
            }
        }
        ApplySteer();
    }


    private void ApplySteer()
    {
        // Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        // float newSteer = (relativeVector.x / relativeVector.magnitude) * turnSpeed;


        // if (enemyAI.Navigation.Barge == null) return;

        var target = nodes[currentNode].position;
        var headingToTarget = (target - transform.position).normalized;

        var headingDifference = Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward);
        if (Mathf.Abs(headingDifference) > 15f)
        {
            engines.TurnTowardsTarget(headingDifference);
            return;
        }

        engines.TurnTowardsTarget(headingDifference);
        engines.MoveForward();
    }
}
