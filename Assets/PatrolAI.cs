using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class PatrolAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target, nextTarget;


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
    }

    private void OnDisable()
    {
        Debug.Log("PatrolAI disabled.");
    }

    private void FixedUpdate()
    {
        // TODO:
        // should search for nearest waypoint on any orbital path on script enable
        // should then only update targeting when close to target waypoint

        // query targeting system
        (target, nextTarget) = enemyAI.Targeting.TrackPath();

        // instruct navigation system
        if (target != Vector2.zero && nextTarget != Vector2.zero)
        {
            enemyAI.Navigation.ApplySteer(target, nextTarget);
        }
        else
        {
            // something else?
        }
    }
}
