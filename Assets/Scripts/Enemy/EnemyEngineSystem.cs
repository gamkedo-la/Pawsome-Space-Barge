using UnityEngine;

/// <summary>
/// Enemy AI Engine System.
/// "Scotty, we need more power"
/// </summary>
[RequireComponent( typeof(Rigidbody2D) )]
public class EnemyEngineSystem : MonoBehaviour
{
    // private properties
    private Rigidbody2D rb2d;

    [ReadOnly][SerializeField]
    private float heading;

    [ReadOnly][SerializeField]
    private float stunTimer = 0;



    [Header("Velocity Settings")]
    [SerializeField] [Min(0)] [Tooltip("Turning speed, degrees per second")]
    private float turningSpeed = 300;

    [SerializeField] [Range(1, 200)] [Tooltip("Maximum enemy velocity.")]
    private float maxSpeed = 100;

    [SerializeField] [Range(1, 200)] [Tooltip("Minimum enemy velocity.")]
    private float minSpeed = 25;

    [SerializeField] [Min(0)] [Tooltip("Thruster force")]
    private float thrusterForce = 1000;

    [SerializeField] [Range(0,10)] [Tooltip("Tug braking coefficient, more => faster.")]
    private float decelerationCoefficient = 1;



    [Header("Stun Timer Settings")]
    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    private float asteroidStunTime = 0f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float playerStunTime = 3f;



    [Header("Thrusters")] 
    [SerializeField] private Thruster rearLeftThruster;
    
    [SerializeField] private Thruster rearRightThruster;



    // *********************** Accessors ***********************
    /// <summary> RigidBody2D InstanceID. For comparators. </summary>
    public int rb2dID => rb2d.GetInstanceID();

    /// <summary> Engine system status check. </summary>
    public bool Online => stunTimer <= 0;

    /// <summary> Current ship velocity. </summary>
    public Vector2 Velocity => rb2d.velocity;

    /// <summary> Set maximum velocity. </summary>
    public float MaxSpeed { set { maxSpeed = value; } }

    /// <summary> Set minimum velocity. </summary>
    public float MinSpeed { set { minSpeed = value; } }

    /// <summary> Set braking coefficient. </summary>
    public float Braking { set { decelerationCoefficient = value; } }



    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        heading = rb2d.rotation;
    }


    private void Update()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
    }


    /// <summary>
    /// Accelerate ship. Applies maximum speed enforcement.
    /// </summary>
    /// <param name="thrust"></param>
    public void ApplyThrust(float thrust)
    {
        if (stunTimer <= 0 && !GameManagement.Instance.DialogActive)
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
            
            AdjustThrusters(thrust);
        }
        else
        {
            AdjustThrusters(0f);
        }
    }


    /// <summary>
    /// Decelerate ship. Applies minimum speed enforcement.
    /// </summary>
    public void ApplyBrakes()
    {
        rb2d.velocity -= rb2d.velocity.normalized * decelerationCoefficient;

        if (rb2d.velocity.magnitude < minSpeed)
        {
            rb2d.velocity = rb2d.velocity.normalized * minSpeed;
        }
    }


    /// <summary>
    /// Turns ship relative to direction (+/-) and magnitude of 'headingChange'.
    /// </summary>
    /// <param name="headingChange"></param>
    public void RotateShip(float headingChange)
    {
        heading = ClampAngle( rb2d.rotation + headingChange * turningSpeed * Time.fixedDeltaTime );

        // TODO: dampen this rotation?
        if (stunTimer <= 0)
        {
            rb2d.MoveRotation( heading );
        }
    }
    

    /// <summary>
    /// Adjusts thrusters. On if thrustInput > 0 else off.
    /// </summary>
    /// <param name="thrustInput"></param>
    private void AdjustThrusters(float thrustInput)
    {        
        switch (thrustInput)
        {
            case > 0:
                rearLeftThruster.On(thrustInput);
                rearRightThruster.On(thrustInput);
                break;
            default:
                rearLeftThruster.Off();
                rearRightThruster.Off();
                break;
        }
    }


    /// <summary>
    /// Clamps 'angle' to 360 degree values. Handles rollover.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float ClampAngle(float angle)
    {
        return (angle % 360) + (angle < 0 ? 360 : 0);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (stunTimer <= 0)
        {
            if (other.gameObject.CompareTag("Asteroid"))
            {
                stunTimer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                stunTimer = playerStunTime;
            }
        }
    }
}
