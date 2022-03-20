using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    [SerializeField] [Min(0)] [Tooltip("Turning speed, degrees per second")]
    private float turningSpeed = 90;

    [SerializeField] [Min(0)] [Tooltip("Thruster force")]
    private float thrusterForce = 500;

    private Rigidbody2D rb2d;
    private EnemyAIStateMachine enemyAI;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAIStateMachine>();
        // Turn off by default
        enabled = false;
    }

    private void FixedUpdate()
    {
        if (enemyAI.Barge == null) return;

        var target = enemyAI.Barge.transform.position;
        var headingToTarget = (target - transform.position).normalized;

        var headingDifference = Vector3.SignedAngle(transform.right, headingToTarget, Vector3.forward);
        if (Mathf.Abs(headingDifference) > 15f)
        {
            TurnTowardsTarget(headingDifference);
            return;
        }

        TurnTowardsTarget(headingDifference);
        MoveForward();
    }

    private void MoveForward()
    {
        rb2d.AddForce(transform.right * thrusterForce, ForceMode2D.Force);
    }

    private void TurnTowardsTarget(float headingChange)
    {
        headingChange = Mathf.Clamp(headingChange, -turningSpeed * Time.fixedDeltaTime, turningSpeed * Time.fixedDeltaTime);
        rb2d.rotation += headingChange;
    }
}