using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class PatrolAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target;
    private Vector2 alertTarget = Vector2.zero;


    [SerializeField, Tooltip("Braking factor."), Range(0,10)]
    private float braking = 0.1f;

    [SerializeField, Tooltip("Maximum speed."), Range(1, 200)]
    private float maxSpeed = 100;


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

        // find nearest patrol waypoint
        target = enemyAI.Targeting.NearestPathPoint(transform.position);
    }


    private void OnDisable()
    {
        // Debug.Log("PatrolAI disabled.");
    }


    private void FixedUpdate()
    {
        // query targeting system
        target = enemyAI.Targeting.TrackPath();

        // instruct navigation system
        if (alertTarget == Vector2.zero)
        {
            enemyAI.Navigation.NavigateToTarget(target);
        }
        else
        {
            // TODO: alert target navigation
            // navigate to closest path node
            //



            // navigate to target
            //



            // if targetClose && !bargeDetected
            // // alertTarget = Vector2.zero
        }
    }


    [SerializeField, ReadOnly] private int alertsRecieved = 0;

    /// <summary>
    /// Notification reciever.
    /// </summary>
    /// <param name="position"></param>
    public void recieveAlert(Vector2 position)
    {
        // Debug.Log(position.ToString());
        alertsRecieved++;

        // TODO: uncomment when above complete
        // alertTarget = position;
    }
}
