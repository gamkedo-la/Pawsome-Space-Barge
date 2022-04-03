using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target;

    [SerializeField, Tooltip("Braking factor."), Range(0,10)]
    private float braking = 8;

    [SerializeField, Tooltip("Maximum speed."), Range(1, 200)]
    private float maxSpeed = 80;


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
        enemyAI.Engines.Braking = braking;
        enemyAI.Engines.MaxSpeed = maxSpeed;
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