using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target, nextTarget;


    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();

        // Turn off by default
        enabled = false;
    }

    private void OnEnable()
    {
        // setup systems

        // engines
        enemyAI.Engines.Braking = 8;
        enemyAI.Engines.MaxSpeed = 80;
    }

    private void OnDisable()
    {
        Debug.Log("PatrolAI disabled.");
    }

    private void FixedUpdate()
    {
        // query targeting system
        (target, nextTarget) = enemyAI.Targeting.TrackBarge();

        // instruct navigation system
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