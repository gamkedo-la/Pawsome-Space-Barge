using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class ContactAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private GameObject target;

    [SerializeField, Tooltip("Braking factor."), Range(0,10)]
    private float braking = 5f;

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

        // lock target
        target = GameManagement.Instance.Barge;
        GameManagement.Instance.enemyContactsCount++;
    }


    private void OnDisable()
    {
        Debug.Log("ContactAI disabled.");
        GameManagement.Instance.enemyContactsCount--;
    }


    private void FixedUpdate()
    {
        enemyAI.Navigation.NavigateToTarget(target.transform.position);

        enemyAI.Targeting.sendAlert();
    }
}
