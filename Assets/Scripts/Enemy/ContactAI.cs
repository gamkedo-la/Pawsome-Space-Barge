using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class ContactAI : MonoBehaviour
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
        enemyAI.Engines.Braking = 5;
        enemyAI.Engines.MaxSpeed = 80;
    }


    private void OnDisable()
    {
        Debug.Log("ContactAI disabled.");
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
