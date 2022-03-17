using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ship Movement Controller.
/// Handles ship physics input,
/// and position mirroring when out of bounds.
/// </summary>
public class ShipMovement : MonoBehaviour
{
    [Tooltip("Tug rotation speed.")]
    [Range(100,500)]
    [SerializeField] private int rotationSpeed = 300;

    [Tooltip("Tug thrust power. Moar POWER!")]
    [Range(250,1000)]
    [SerializeField] private int thrustForce = 500;

    [Tooltip("Tug braking coefficient, more => faster.")]
    [Range(0,10)]
    [SerializeField] private float decelerationCoefficient = 4;

    [Tooltip("Tug max speed.")]
    [Range(50,300)]
    [SerializeField] private int maxSpeed = 150;

    [Tooltip("Disable for free flight!")]
    [SerializeField] private bool enforceBoundary = true;

    // [Tooltip("Select a prefab to spawn when you hit an asteroid!")]
    // [SerializeField] private GameObject collision_FX_Prefab;


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


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // setup boundary
        playerBoundary = GameObject.FindGameObjectWithTag("PlayerBoundary").GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // Spawn close to the barge, if we find it
        var barge = GameObject.FindGameObjectWithTag("Barge");
        if (barge != null)
        {
            transform.Translate(barge.transform.position);
        }
    }

    private void Update()
    {
        AdjustThrusters();
    }

    private void FixedUpdate()
    {
        // input reaction
        if (rotationInput != 0)
        {
            rb2d.rotation += rotationSpeed * Time.fixedDeltaTime * rotationInput;
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
        rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);

        // enforce player boundary
        if (enforceBoundary)
        {
            checkPosition();
        }
    }


    // check bounds, wrap around ship as necessary
    private void checkPosition()
    {
        if (!rb2d.IsTouching(playerBoundary))
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
                return;
            case < 0:
                rearLeftThruster.On(rotationInput);
                rearRightThruster.Off();
                frontLeftThruster.Off();
                frontRightThruster.On(rotationInput);
                return;
        }
        
        switch (thrustInput)
        {
            case > 0:
                rearLeftThruster.On(thrustInput);
                rearRightThruster.On(thrustInput);
                frontLeftThruster.Off();
                frontRightThruster.Off();
                break;
            case < 0:
                rearLeftThruster.On(thrustInput);
                rearRightThruster.On(thrustInput);
                frontLeftThruster.On(thrustInput);
                frontRightThruster.On(thrustInput);
                break;
            default:
                rearLeftThruster.Off();
                rearRightThruster.Off();
                frontLeftThruster.Off();
                frontRightThruster.Off();
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


    // collison event for playing a shield noise
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Asteroid" && other.relativeVelocity.magnitude > 5)
        {
            SoundManager.Instance.PlaySound("Ping", 0.5f);
            
            // if (this.collision_FX_Prefab) {
            //     Debug.Log("Spawning collision FX prefab!");
            //     Instantiate(this.collision_FX_Prefab,
            //         other.GetContact(0).point,//this.transform.position,
            //         this.transform.rotation); 
            // }
        }
    }
}