using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSystemTest : MonoBehaviour
{
    [SerializeField] [Min(1)] [Tooltip("Turning speed, degrees per second.")]
    private float turningSpeed = 75;

    [SerializeField] [Range(1, 200)] [Tooltip("Maximum enemy velocity.")]
    private float maxSpeed = 100;
    public float minSpeed = 25;

    [SerializeField] [Min(0)] [Tooltip("Thruster force.")]
    private float thrusterForce = 500;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    private float asteroidStunTime = 0f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    private float playerStunTime = 3f;

    [SerializeField] [Range(0,10)] [Tooltip("Tug braking coefficient, more => faster.")]
    private float decelerationCoefficient = 4;

    private Rigidbody2D rb2d;
    public int rb2dID => rb2d.GetInstanceID();

    private float timer = 0;

    public bool Status => timer <= 0;

    public Vector2 Velocity => rb2d.velocity;
    public float heading;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        heading = rb2d.rotation;
    }


    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }


    public void MoveForward(float thrust)
    {
        if (timer <= 0)
        {
            if (thrust >= 0)
            {
                rb2d.AddForce(transform.right * thrusterForce * thrust, ForceMode2D.Force);

                rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);
            }
            else
            {
                ApplyBrakes();
            }
            
        }
    }

    public void ApplyBrakes()
    {
        rb2d.velocity -= rb2d.velocity.normalized * decelerationCoefficient;

        if (rb2d.velocity.magnitude < minSpeed)
        {
            rb2d.velocity = rb2d.velocity.normalized * minSpeed;
        }
    }


    public void TurnTowardsTarget(float headingChange)
    {
        heading = ClampAngle( rb2d.rotation + headingChange * turningSpeed * Time.fixedDeltaTime );

        // dampen this rotation?
        if (timer <= 0)
        {
            rb2d.MoveRotation( heading );
        }
    }

    private float ClampAngle(float angle)
    {
        return (angle % 360) + (angle < 0 ? 360 : 0);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (timer <= 0)
        {
            if (other.gameObject.CompareTag("Asteroid"))
            {
                timer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                timer = playerStunTime;
            }
        }
    }
}
