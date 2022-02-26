using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    public OrbitalElements orbitalElements;
    public float boosterPower = 0f;
    public float lateralPower = 0f;
    public float timeOffset;

    private Transform centerOfMass;
    private static float AngularVelocity => Mathf.Deg2Rad * AsteroidField.Instance.fieldSpeed / 100f;

    private Rigidbody2D rb2d;

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 velocity;

    [SerializeField] private Vector3 positionEci;
    [SerializeField] private Vector3 velocityEci;

    private Vector3 forward;
    private Vector3 right;
    private Vector3 up = Vector3.forward;

    private bool initializedOrbit;

    public Vector3 Position => position;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        centerOfMass = GameObject.FindGameObjectWithTag("Planet").transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(position, forward * 10);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(position, right * 10);
    }

    private void FixedUpdate()
    {
        var currentTimestamp = Time.fixedTime + timeOffset;
        if (!initializedOrbit)
        {
            initializedOrbit = true;
            // Create the initial orbital elements based on current position being in a stable, circular orbit
            var (pos, vel) = WorldToPlanetCentricInertial(rb2d.position, Vector3.zero);
            orbitalElements = OrbitalElements.CircularOrbit(currentTimestamp, pos, vel);
        }

        UpdatePositionAndVelocityAtTime(currentTimestamp);
        rb2d.MovePosition(position);

        if (boosterPower == 0 && lateralPower == 0) return;

        SetVelocity(currentTimestamp, boosterPower * forward + lateralPower * right);
        boosterPower = 0;
        lateralPower = 0;
    }

    private void SetVelocity(float time, Vector3 deltaV)
    {
        orbitalElements.SetOrbitFromObservation(time, positionEci, velocityEci + deltaV);
    }

    private (Vector3, Vector3) WorldToPlanetCentricInertial(Vector3 worldPosition, Vector3 worldVelocity)
    {
        var eciPosition = worldPosition - centerOfMass.position;
        var eciVelocity = worldVelocity + AngularVelocity * Vector3.Cross(eciPosition, Vector3.forward);
        return (eciPosition, eciVelocity);
    }

    private void UpdatePositionAndVelocityAtTime(float time)
    {
        (positionEci, velocityEci) = orbitalElements.ToCartesian(time);
        var forwardTrajectory = Vector3.Cross(up, positionEci);

        position = positionEci + centerOfMass.position;
        velocity = velocityEci - AngularVelocity * forwardTrajectory;
        forward = forwardTrajectory.normalized;
        right = positionEci.normalized;
    }

    public Vector3 GetAdjustedWorldPositionAtTime(float time)
    {
        var (pos, _) = orbitalElements.ToCartesian(time);
        return pos + centerOfMass.position;
    }
}