using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;


    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();

        // Turn off by default
        enabled = false;
    }

    private void FixedUpdate()
    {
        if (enemyAI.Navigation.Barge == null) return;

        var target = enemyAI.Navigation.Barge.transform.position;
        var headingToTarget = (target - transform.position).normalized;

        var headingDifference = Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward);
        if (Mathf.Abs(headingDifference) > 15f)
        {
            enemyAI.Engines.TurnTowardsTarget(headingDifference);
            return;
        }

        enemyAI.Engines.TurnTowardsTarget(headingDifference);
        enemyAI.Engines.MoveForward();
    }
}