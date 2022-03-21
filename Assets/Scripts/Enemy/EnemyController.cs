using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] [Min(0)] [Tooltip("Turning speed, degrees per second")]
    private float turningSpeed = 90;

    [SerializeField] [Min(0)] [Tooltip("Thruster force")]
    private float thrusterForce = 500;

    private Rigidbody2D rb2d;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }


    public void MoveForward()
    {
        rb2d.AddForce(transform.right * thrusterForce, ForceMode2D.Force);
    }


    public void TurnTowardsTarget(float headingChange)
    {
        headingChange = Mathf.Clamp(headingChange, -turningSpeed * Time.fixedDeltaTime, turningSpeed * Time.fixedDeltaTime);
        rb2d.rotation += headingChange;
    }
}
