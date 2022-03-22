using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private EnemyEngineSystem controller;


    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();
        controller = GetComponent<EnemyEngineSystem>();

        // Turn off by default
        enabled = false;
    }

    private void FixedUpdate()
    {
        if (enemyAI.Barge == null) return;

        var target = enemyAI.Barge.transform.position;
        var headingToTarget = (target - transform.position).normalized;

        var headingDifference = Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward);
        if (Mathf.Abs(headingDifference) > 15f)
        {
            controller.TurnTowardsTarget(headingDifference);
            return;
        }

        controller.TurnTowardsTarget(headingDifference);
        controller.MoveForward();
    }
}