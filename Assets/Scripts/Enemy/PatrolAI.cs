using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class PatrolAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target;


    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();

        // Turn off by default
        enabled = false;
    }


    /// <summary>
    /// Can be used to setup system for current state.
    /// </summary>
    private void OnEnable()
    {
        // engines
        enemyAI.Engines.Braking = 0;
        enemyAI.Engines.MaxSpeed = 100;

        // find nearest patrol waypoint
        target = enemyAI.Targeting.NearestPathPoint(transform.position);
    }


    private void OnDisable()
    {
        Debug.Log("PatrolAI disabled.");
    }


    private void FixedUpdate()
    {
        // query targeting system
        target = enemyAI.Targeting.TrackPath();

        // instruct navigation system
        if (target != Vector2.zero)
        {
            enemyAI.Navigation.NavigateToTarget(target);
        }
        else
        {
            // something else?
        }
    }
}
