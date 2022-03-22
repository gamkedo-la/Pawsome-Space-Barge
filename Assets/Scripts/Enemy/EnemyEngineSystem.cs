using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEngineSystem : MonoBehaviour
{
    [SerializeField] [Min(0)] [Tooltip("Turning speed, degrees per second")]
    private float turningSpeed = 90;

    [SerializeField] [Min(0)] [Tooltip("Thruster force")]
    private float thrusterForce = 500;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float asteroidStunTime = 0f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float playerStunTime = 3f;

    private Rigidbody2D rb2d;

    private float timer = 0;


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


    public void MoveForward()
    {
        if (timer <= 0)
        {
            rb2d.AddForce(transform.right * thrusterForce, ForceMode2D.Force);
        }
    }


    public void TurnTowardsTarget(float headingChange)
    {
        if (timer <= 0)
        {
            headingChange = Mathf.Clamp(headingChange, -turningSpeed * Time.fixedDeltaTime, turningSpeed * Time.fixedDeltaTime);
            rb2d.rotation += headingChange;
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
