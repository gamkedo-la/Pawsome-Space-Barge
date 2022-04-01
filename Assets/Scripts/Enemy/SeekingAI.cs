using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target;


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
        Debug.Log("SeekingAI disabled.");
    }

    private void FixedUpdate()
    {
        // query targeting system
        target = enemyAI.Targeting.TrackBarge();

        // instruct navigation system
        if (target != Vector2.zero)
        {
            enemyAI.Navigation.NavigateToTarget(target);
        }
        else
        {
            // something else
        }
    }
}