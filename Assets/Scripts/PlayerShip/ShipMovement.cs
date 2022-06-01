using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ship Movement Controller.
/// Handles ship physics input,
/// and position mirroring when out of bounds.
/// </summary>
public class ShipMovement : MonoBehaviour
{
    private SoundManagement soundManager;
    private float steeringAccumulator = 0;
    private PlayerInput pi;
    private new Transform camera => GameManagement.Instance.CameraManager.GetActiveCamera(pi);
    private BoxCollider2D playerBoundary;
    private Rigidbody2D rb2d;
    private Rigidbody2D bargeRigidbody;


    [Header("Player Ship Settings")]
    [SerializeField, Tooltip("Tug rotation speed."), Range(100,500)]
    private int rotationSpeed = 300;

    [SerializeField, Tooltip("Should steering be accellerated? Ie start slow and turn faster as input held?")]
    private bool accellerateSteering = false;

    [SerializeField, Tooltip("Acceleration factor. Percentage. Does not effect thumbstick input."), Range(0.001f, 190f)]
    private float steeringAcceleration = 0.01f;

    [SerializeField, Tooltip("Tug thrust power. Moar POWER!"), Range(250,1000)]
    private int thrustForce = 500;

    [SerializeField, Tooltip("Tug braking coefficient, more => faster."), Range(0,10)]
    private float decelerationCoefficient = 4;

    [SerializeField, Tooltip("Tug max speed."), Range(50,300)]
    private int maxSpeed = 150;

    [SerializeField, Tooltip("Max speed as percentage of barge speed")]
    private bool speedLimitRelativeToBarge;

    [SerializeField, Tooltip("Thumbstick input smoothing time")]
    private float smoothInputTime = 0.2f;

    [SerializeField, Tooltip("Threshold for activating thruster animation on thumbstick rotation.")]
    public float thrustRotatingThreshold = 0.5f;

    [SerializeField, Tooltip("Rotation dampening value; higher = slower turning.")]
    public float rotationDamping = 48f;


    // Disabled for now, boundary check needs to consider barge position and rotation.
    // [SerializeField, Tooltip("Disable for free flight!")]
    private bool enforceBoundary = false;


    [Header("Input Debug")]
    // how do I get this 81.75 number from the camera... I get a number < 1 when polled during gameplay.
    [SerializeField, ReadOnly] private Vector3 antiRotation = Vector3.zero;

    /// <summary> Most recent rotation input value. </summary>
    [ReadOnly, SerializeField] private float rotationInput = 0;

    /// <summary> Most recent thrust input value. </summary>
    [ReadOnly, SerializeField] private float thrustInput = 0;

    /// <summary> Most recent thumbstick input. </summary>
    [ReadOnly, SerializeField] private Vector2 thumbstickInput = Vector2.zero;


    [ReadOnly, SerializeField] private Vector2 currentInputVector;
    [ReadOnly, SerializeField] private Vector2 smoothInputVelocity;
    [ReadOnly, SerializeField] private Vector3 cameraRelativeInputVector;
    [ReadOnly, SerializeField] private Vector3 shipForwardVector;
    [ReadOnly, SerializeField] private float dotHeadingInput;
    [ReadOnly, SerializeField] private float thrusterRotation;
    [ReadOnly, SerializeField] private float thrusterThrust;



    [Header("Thrusters")] 
    [SerializeField] private Thruster rearLeftThruster;
    
    [SerializeField] private Thruster rearRightThruster;
    
    [SerializeField] private Thruster frontLeftThruster;
    
    [SerializeField] private Thruster frontRightThruster;



    private float MaxSpeed => (speedLimitRelativeToBarge && bargeRigidbody != null)
        ? maxSpeed * bargeRigidbody.velocity.magnitude / 100f
        : maxSpeed;



    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        pi = GetComponent<PlayerInput>();

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
            bargeRigidbody = barge.GetComponent<Rigidbody2D>();
            transform.Translate(barge.transform.position + ((Vector3)bargeRigidbody.velocity*3f));
            rb2d.velocity = bargeRigidbody.velocity;
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
        // thumbstick input smoothing
        if (pi.currentControlScheme == "Catpad")
        {
            SmoothThumbstickInput();
        }

        // d-pad overules thumb stick 
        if (thrustInput != 0 || rotationInput != 0)
        {
            if (rotationInput != 0)
            {
                if (accellerateSteering)
                {
                    if (steeringAccumulator < rotationSpeed)
                    {
                        steeringAccumulator = Mathf.Lerp(steeringAccumulator, rotationSpeed, steeringAcceleration * Time.fixedDeltaTime);
                    }

                    rb2d.rotation += steeringAccumulator * Time.fixedDeltaTime * rotationInput;
                }
                else
                {
                    rb2d.rotation += rotationSpeed * Time.fixedDeltaTime * rotationInput;
                }
            }
            else if (steeringAccumulator > 0)
            {
                steeringAccumulator = 0;
            }

            thrusterRotation = rotationInput;
            thrusterThrust = thrustInput;
        }
        else if (thumbstickInput != Vector2.zero)
        {
            if (GameManagement.Instance.CameraManager.CurrentCameraMode == CameraManagement.CameraMode.ThirdPerson)
            {
                if (antiRotation == Vector3.zero)
                {
                    antiRotation = new Vector3(camera.localEulerAngles.y, 0, 0);
                }
                cameraRelativeInputVector = camera.rotation * Quaternion.Euler(antiRotation) * currentInputVector;
            }
            else
            {
                cameraRelativeInputVector = camera.rotation * currentInputVector;
            }

            cameraRelativeInputVector.z = 0;

            shipForwardVector = transform.right;

            dotHeadingInput = Vector2.Dot(shipForwardVector, cameraRelativeInputVector);

            // Draw debug line
            Debug.DrawLine(transform.position, transform.position + cameraRelativeInputVector * 200, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + shipForwardVector * 100, Color.red);

            // Vector3 booger = rb2d.velocity;
            // var thing3 = Vector3.SmoothDamp(shipForwardVector, cameraRelativeInputVector, ref booger, Time.fixedDeltaTime, rotationSpeed/4);
            var nextVector = Vector3.RotateTowards(shipForwardVector, cameraRelativeInputVector, Mathf.PI/rotationDamping, 0);
            var angleAdjustment = Vector2.SignedAngle(shipForwardVector, nextVector);

            rb2d.rotation += angleAdjustment;

            // set values for thrusters
            thrusterRotation = Mathf.Abs(angleAdjustment) > thrustRotatingThreshold ? (Mathf.Sign(angleAdjustment)) : 0;
            thrusterThrust = dotHeadingInput > 0.9 ? 1 : 0;
        }
        else
        {
            thrusterRotation = 0;
            thrusterThrust = 0;
        }

        // add thrust?
        if (thrusterThrust > 0)
        {
            rb2d.AddForce(gameObject.transform.right * thrustForce * rb2d.mass );
        }

        if (thrusterThrust < 0)
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


    /// <summary> Smooths input vector to prevent noisy values. </summary>
    void SmoothThumbstickInput()
    {
        currentInputVector = Vector2.SmoothDamp(currentInputVector, thumbstickInput, ref smoothInputVelocity, smoothInputTime);
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
        switch (thrusterRotation)
        {
            case > 0:
                rearLeftThruster.Off();
                rearRightThruster.On(thrusterRotation);
                frontLeftThruster.On(thrusterRotation);
                frontRightThruster.Off();
                soundManager.PlayShipThrusters("Thrusters", 0.7f);
                soundManager.AdjustThrusterDirection(-.2f);
                return;
            case < 0:
                rearLeftThruster.On(thrusterRotation);
                rearRightThruster.Off();
                frontLeftThruster.Off();
                frontRightThruster.On(thrusterRotation);
                soundManager.PlayShipThrusters("Thrusters", 0.7f);
                soundManager.AdjustThrusterDirection(.2f);
                return;
        }
        
        switch (thrusterThrust)
        {
            case > 0:
                rearLeftThruster.On(thrusterThrust);
                rearRightThruster.On(thrusterThrust);
                frontLeftThruster.Off();
                frontRightThruster.Off();
                soundManager.PlayShipThrusters("Thrusters", 1f);
                soundManager.AdjustThrusterDirection(0f);
                break;
            case < 0:
                rearLeftThruster.On(thrusterThrust);
                rearRightThruster.On(thrusterThrust);
                frontLeftThruster.On(thrusterThrust);
                frontRightThruster.On(thrusterThrust);
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


    public void OnThumbstick(InputAction.CallbackContext context)
    {
        thumbstickInput = context.ReadValue<Vector2>();
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