using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSystemTest : MonoBehaviour
{
    [SerializeField] [Min(1)] [Tooltip("Turning speed, degrees per second.")]
    private float turningSpeed = 75;

    [SerializeField] [Range(1, 200)] [Tooltip("Maximum enemy velocity.")]
    private float maxSpeed = 100;

    [SerializeField] [Min(0)] [Tooltip("Thruster force.")]
    private float thrusterForce = 500;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    private float asteroidStunTime = 0f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    private float playerStunTime = 3f;

    private Rigidbody2D rb2d;

    private float timer = 0;

    public bool Status => timer <= 0;

    public Vector2 Velocity => rb2d.velocity;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
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
            rb2d.AddForce(-transform.up * thrusterForce * thrust, ForceMode2D.Force);
        }

        rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);
    }


    public void TurnTowardsTarget(float headingChange)
    {
        if (timer <= 0)
        {
            rb2d.rotation += headingChange * turningSpeed * Time.fixedDeltaTime;
        }
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
