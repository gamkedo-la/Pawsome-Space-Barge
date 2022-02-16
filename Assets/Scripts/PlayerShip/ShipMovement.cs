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

    /// <summary> Player boundary collider. </summary>
    [HideInInspector] private BoxCollider2D playerBoundary;

    /// <summary> Player ship RigidBody2D. </summary>
    [HideInInspector] private Rigidbody2D rb2d;

    /// <summary> Most recent rotation input value. </summary>
    [HideInInspector] private float rotationInput = 0;

    /// <summary> Most recent thrust input value. </summary>
    [HideInInspector] private float thrustInput = 0;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // setup boundary
        playerBoundary = GameObject.FindGameObjectWithTag("PlayerBoundary").GetComponent<BoxCollider2D>();
    }

    // private void Update()
    // {
    //     // not used yet
    // }

    private void FixedUpdate()
    {
        // input reaction
        if (rotationInput != 0)
        {
            rb2d.rotation += rotationSpeed * Time.fixedDeltaTime * rotationInput;
        }
        if (thrustInput > 0)
        {
            rb2d.AddForce(gameObject.transform.right * thrustForce);
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
}
