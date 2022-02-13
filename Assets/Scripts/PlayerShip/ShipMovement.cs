using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    [Tooltip("Tug rotation speed.")]
    [Range(100,500)]
    [SerializeField] int rotationSpeed = 300;

    [Tooltip("Tug thrust power. Moar POWER!")]
    [Range(250,1000)]
    [SerializeField] int thrustForce = 500;

    [Tooltip("Tug braking coefficient, more => faster.")]
    [Range(0,10)]
    [SerializeField] float decelerationCoefficient = 4;

    [Tooltip("Tug max speed.")]
    [Range(50,300)]
    [SerializeField] int maxSpeed = 150;
    [Tooltip("Disable for free flight!")]
    [SerializeField] bool enforceBoundary = true;

    // boundary properties
    private BoxCollider2D playerBoundary;
    private float boundaryXextent;
    private float boundaryYextent;

    // player rigidbody
    private Rigidbody2D rb2d;

    // input triggers
    private float rotationInput = 0;
    private float thrustInput = 0;


    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // setup boundary values
        playerBoundary = GameObject.FindGameObjectWithTag("PlayerBoundary").GetComponent<BoxCollider2D>();
        boundaryXextent = playerBoundary.bounds.extents.x;
        boundaryYextent = playerBoundary.bounds.extents.y;
    }

    // void Update()
    // {
    //     // not used yet
    // }

    void FixedUpdate()
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

        if (enforceBoundary)
        {
            checkPosition();
        }
    }

    // check bounds, wrap around ship as necessary
    private void checkPosition()
    {
        // not in box collider
        if (!rb2d.IsTouching(playerBoundary))
        {
            // copy current position
            Vector2 correctedPos = rb2d.position;

            // modify new position by which axis is passed
            // x axis boundary check
            if (rb2d.position.x > boundaryXextent || rb2d.position.x < -boundaryXextent)
            {
                correctedPos.x *= -1f * 0.95f;
            }
            // y axis boundary check
            if (rb2d.position.y > boundaryYextent || rb2d.position.y < -boundaryYextent)
            {
                correctedPos.y *= -1f * 0.95f;
            }

            // set corrected transform
            transform.position = correctedPos;
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
