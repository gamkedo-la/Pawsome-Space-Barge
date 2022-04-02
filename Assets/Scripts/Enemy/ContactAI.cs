using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class ContactAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private GameObject target;


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

        // lock target
        target = GameObject.FindGameObjectWithTag("Barge");
    }


    private void OnDisable()
    {
        Debug.Log("ContactAI disabled.");
    }


    private void FixedUpdate()
    {
        enemyAI.Navigation.NavigateToTarget(target.transform.position);

        // call barge method to reduce value or speed it up here
    }
}
