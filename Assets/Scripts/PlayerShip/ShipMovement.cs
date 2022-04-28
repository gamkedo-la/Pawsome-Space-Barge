using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ship Movement Controller.
/// Handles ship physics input,
/// and position mirroring when out of bounds.
/// </summary>
public class ShipMovement : MonoBehaviour
{
    public static ShipMovement instance; // Only used for billboard behavior. Could refactor out.
    private SoundManagement soundManager;
    private float steeringAccumulator = 0;



    [SerializeField, Tooltip("Tug rotation speed."), Range(100,500)]
    private int rotationSpeed = 300;

    [SerializeField, Tooltip("Should steering be accellerated? Ie start slow and turn faster as input held?")]
    private bool accellerateSteering = false;

    [SerializeField, Tooltip("Accelleration factor. Percentage."), Range(0.001f, 190f)]
    private float steeringAcceleration = 0.01f;

    [SerializeField, Tooltip("Tug thrust power. Moar POWER!"), Range(250,1000)]
    private int thrustForce = 500;

    [SerializeField, Tooltip("Tug braking coefficient, more => faster."), Range(0,10)]
    private float decelerationCoefficient = 4;

    [SerializeField, Tooltip("Tug max speed."), Range(50,300)]
    private int maxSpeed = 150;

    [SerializeField, Tooltip("Max speed as percentage of barge speed")] private bool speedLimitRelativeToBarge;


    // Disabled for now, boundary check needs to consider barge position and rotation.
    // [SerializeField, Tooltip("Disable for free flight!")]
    private bool enforceBoundary = false;


    /// <summary> Player boundary collider. </summary>
    [HideInInspector] private BoxCollider2D playerBoundary;

    /// <summary> Player ship RigidBody2D. </summary>
    [HideInInspector] private Rigidbody2D rb2d;

    /// <summary> Most recent rotation input value. </summary>
    [HideInInspector] private float rotationInput = 0;

    /// <summary> Most recent thrust input value. </summary>
    [HideInInspector] private float thrustInput = 0;



    [Header("Thrusters")] 
    [SerializeField] private Thruster rearLeftThruster;
    
    [SerializeField] private Thruster rearRightThruster;
    
    [SerializeField] private Thruster frontLeftThruster;
    
    [SerializeField] private Thruster frontRightThruster;



    private Rigidbody2D bargeRigidbody;
    
    private float MaxSpeed => (speedLimitRelativeToBarge && bargeRigidbody != null)
        ? maxSpeed * bargeRigidbody.velocity.magnitude / 100f
        : maxSpeed;



    private void Awake()
    {
        instance = this;

        rb2d = GetComponent<Rigidbody2D>();

        // setup boundary
        var boundary = GameObject.FindGameObjectWithTag("PlayerBoundary");

        if (boundary != null)
        {
            playerBoundary = boundary.GetComponent<BoxCollider2D>();
        }
        else
        {
            playerBoundary = null;
        }
    }


    private void Start()
    {
        soundManager = GameManagement.Instance.SoundManager;

        // Spawn close to the barge, if we find it
        var barge = GameObject.FindGameObjectWithTag("Barge");
        if (barge != null)
        {
            transform.Translate(barge.transform.position);
            bargeRigidbody = barge.GetComponent<Rigidbody2D>();
        }
    }


    private void Update()
    {
        if (!GameManagement.Instance.GamePaused)
        {
            AdjustThrusters();
        }
    }


    private void FixedUpdate()
    {
        // input reaction
        if (rotationInput != 0)
        {
            if (accellerateSteering)
            {
                if (steeringAccumulator < rotationSpeed)
                {
                    // steeringAccumulator += rotationSpeed * steeringAcceleration;
                    steeringAccumulator = Mathf.Lerp(steeringAccumulator, rotationSpeed, steeringAcceleration * Time.fixedDeltaTime);
                }

                rb2d.rotation += steeringAccumulator * Time.fixedDeltaTime * rotationInput;
            }
            else
            {
                rb2d.rotation += rotationSpeed * Time.fixedDeltaTime * rotationInput;
            }
        }
        else
        {
            steeringAccumulator = 0;
        }

        if (thrustInput > 0)
        {
            if (!rb2d.isKinematic)
            {
                // this is the default
                rb2d.AddForce(gameObject.transform.right * thrustForce * rb2d.mass );
            }
            else
            {
                // this is cruft (for now)

                // part of experiments to improve physics simulation,
                // this with Quaternion orbit method is promising
                rb2d.velocity += new Vector2(rb2d.transform.right.x, rb2d.transform.right.y) * thrustForce *
                                 Time.fixedDeltaTime;
                // will need collider events for interactions with asteroids,
                // else tug barges through regardless of asteroid mass
            }
        }

        if (thrustInput < 0)
        {
            rb2d.velocity -= rb2d.velocity * decelerationCoefficient * Time.fixedDeltaTime;
        }

        // enforce maximum speed
        rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, MaxSpeed);

        // enforce player boundary
        if (enforceBoundary && playerBoundary != null)
        {
            checkPosition();
        }
    }


    // TODO
    // make player boundary work based upon barge position.
    // retro throwback for overhead mode
    // remember to enable Serialization of enforceBoundary

    // check bounds, wrap around ship as necessary
    private void checkPosition()
    {
        if (playerBoundary != null && !rb2d.IsTouching(playerBoundary))
        {
            // copy current position
            Vector2 mirroredPos = rb2d.position;

            // modify new position by which axis is passed
            // x axis boundary check
            if (rb2d.position.x > playerBoundary.bounds.extents.x + playerBoundary.offset.x)
            {
                mirroredPos.x = (-playerBoundary.bounds.extents.x + playerBoundary.offset.x) * 0.95f;
            }

            if (rb2d.position.x < -playerBoundary.bounds.extents.x + playerBoundary.offset.x)
            {
                mirroredPos.x = (playerBoundary.bounds.extents.x + playerBoundary.offset.x) * 0.95f;
            }

            // y axis boundary check
            if (rb2d.position.y > playerBoundary.bounds.extents.y + playerBoundary.offset.y)
            {
                mirroredPos.y = (-playerBoundary.bounds.extents.y + playerBoundary.offset.y) * 0.95f;
            }

            if (rb2d.position.y < -playerBoundary.bounds.extents.y + playerBoundary.offset.y)
            {
                mirroredPos.y = (playerBoundary.bounds.extents.y + playerBoundary.offset.y) * 0.95f;
            }

            // set corrected transform
            rb2d.position = mirroredPos;
        }
    }


    private void AdjustThrusters()
    {
        switch (rotationInput)
        {
            case > 0:
                rearLeftThruster.Off();
                rearRightThruster.On(rotationInput);
                frontLeftThruster.On(rotationInput);
                frontRightThruster.Off();
                soundManager.PlayShipThrusters("Thrusters", 0.7f);
                soundManager.AdjustThrusterDirection(-.2f);
                return;
            case < 0:
                rearLeftThruster.On(rotationInput);
                rearRightThruster.Off();
                frontLeftThruster.Off();
                frontRightThruster.On(rotationInput);
                soundManager.PlayShipThrusters("Thrusters", 0.7f);
                soundManager.AdjustThrusterDirection(.2f);
                return;
        }
        
        switch (thrustInput)
        {
            case > 0:
                rearLeftThruster.On(thrustInput);
                rearRightThruster.On(thrustInput);
                frontLeftThruster.Off();
                frontRightThruster.Off();
                soundManager.PlayShipThrusters("Thrusters", 1f);
                soundManager.AdjustThrusterDirection(0f);
                break;
            case < 0:
                rearLeftThruster.On(thrustInput);
                rearRightThruster.On(thrustInput);
                frontLeftThruster.On(thrustInput);
                frontRightThruster.On(thrustInput);
                soundManager.PlayShipThrusters("Thrusters", .7f);
                soundManager.AdjustThrusterDirection(0f);
                break;
            default:
                rearLeftThruster.Off();
                rearRightThruster.Off();
                frontLeftThruster.Off();
                frontRightThruster.Off();
                soundManager.StopThrusters();
                break;
        }
    }


    // rotation event handler
    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<float>();
    }


    // thrust event handler
    public void OnThrust(InputAction.CallbackContext context)
    {
        thrustInput = context.ReadValue<float>();
    }


    // collsion event for playing a shield noise
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Asteroid" && other.relativeVelocity.magnitude > 5)
        {
            soundManager.PlaySound("Bump Sound", 0.5f);
        }
    }
}