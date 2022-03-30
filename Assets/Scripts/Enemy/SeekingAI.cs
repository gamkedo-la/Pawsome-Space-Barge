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
        Vector2 target, nextTarget;

        (target, nextTarget) = enemyAI.Targeting.AquireTarget();

        if (target != Vector2.zero && nextTarget != Vector2.zero)
        {
            enemyAI.Navigation.ApplySteer(target, nextTarget);
        }
        else
        {
            // something else
        }
    }
}