using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private const float MaxAllowedDeltaV = 0.2f;


    // *************************************** THIS LIFTS-RIGHT-OUT
    public float orbitalDot;
    public Vector3 orbitalVelocity;
    public float orbitalVMag;
    public Vector3 previousOrbitalPosition;
    public Vector3 previousBargePosition;
    public float orbitalDistance2Planet;
    public float bargeDistance2Planet;

    public GameObject correctedBarge;
    private Rigidbody2D bargeCollider;

    private OrbitalState orbitalState;
    private enum OrbitalState
    {
        Descending,
        Stable,
        Ascending
    }

    public float scale = 1f;
    // *************************************** END OF LIFTS-RIGHT-OUT


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();


        // *************************************** THIS LIFTS-RIGHT-OUT
        correctedBarge.transform.position = orbitalBody.Position;
        bargeCollider = correctedBarge.GetComponent<Rigidbody2D>();

        orbitalDot = Vector3.Dot(orbitalBody.Velocity, orbitalBody.PositionPci.normalized);
        orbitalVelocity = orbitalBody.Velocity;
        orbitalVMag = orbitalVelocity.magnitude;

        previousOrbitalPosition = orbitalBody.Position;
        previousBargePosition = correctedBarge.transform.position;

        orbitalDistance2Planet = Vector3.Distance(orbitalBody.Position, orbitalBody.CenterOfMass.position);
        bargeDistance2Planet = Vector3.Distance(correctedBarge.transform.position, orbitalBody.CenterOfMass.position);
        // *************************************** END OF LIFTS-RIGHT-OUT
    }

    private void FixedUpdate()
    {
        orbitalBody.Recalculate(Time.fixedTime);
        rb2d.velocity = orbitalBody.Velocity;


        // *************************************** THIS LIFTS-RIGHT-OUT
        // gather data
        var tempOrbitalPos = orbitalBody.Position;
        var tempOrbitalRot = orbitalBody.ProgradeRotation;
        var tempOrbitalDistance = Vector3.Distance(tempOrbitalPos, orbitalBody.CenterOfMass.position);
        var diff = tempOrbitalPos - previousOrbitalPosition;

        orbitalVelocity = orbitalBody.Velocity;
        orbitalVMag = orbitalVelocity.magnitude;
        orbitalDot = Vector3.Dot(orbitalBody.Velocity, orbitalBody.PositionPci.normalized);

        // magnitude threshold (17.45329) for stable orbit in middle ring
        if (orbitalVMag < 17.4f && orbitalState != OrbitalState.Descending)
        {
            Debug.Log("Descending Orbit");
            orbitalState = OrbitalState.Descending;
            // barge.velocity = 17.4f - orbitalVelocity.magnitude
        }
        else if (orbitalVMag > 17.5f && orbitalState != OrbitalState.Ascending)
        {
            Debug.Log("Ascending Orbit.");
            orbitalState = OrbitalState.Ascending;
            // barge.velocity = orbitalVelocity.magnitude - 17.4f
        }
        else if (orbitalVMag <= 17.5f && orbitalVMag >= 17.4f && orbitalState != OrbitalState.Stable)
        {
            Debug.Log("Stable Orbit");
            orbitalState = OrbitalState.Stable;
        }

        // if (orbitalState == OrbitalState.Descending)
        // {

        //     correctedBarge.transform.position = new Vector3(0, -Mathf.Abs(tempOrbitalDistance), 0) + orbitalVelocity;
        // }

        // apply corrected position
        // correctedBarge.transform.position = correctedOrbitalPosition;
        // correctedBarge.transform.position = new Vector3(0, -Mathf.Abs(tempOrbitalDistance), 0);


        // correctedBarge.transform.position = new Vector3(correctedBarge.transform.position.x, -Mathf.Abs(tempOrbitalDistance), correctedBarge.transform.position.z);

        Quaternion q = Quaternion.AngleAxis(((orbitalVMag * scale)) * Time.deltaTime, Vector3.forward);
        bargeCollider.MovePosition(q * (bargeCollider.transform.position - AsteroidField.Instance.planet) + AsteroidField.Instance.planet);
        bargeCollider.MoveRotation(bargeCollider.transform.rotation * q);



        // bargeCollider.velocity = orbitalBody.Velocity * scale;
        // correctedBarge.transform.position = new Vector3(0, -Mathf.Abs(tempOrbitalDistance), 0);
        // correctedBarge.transform.RotateAround(orbitalBody.CenterOfMass.position, Vector3.forward, 5); //orbitalVMag * scale);
        // correctedBarge.transform.LookAt(orbitalBody.CenterOfMass); //, Vector3.forward);

        // update inspector properties
        previousOrbitalPosition = orbitalBody.Position;
        previousBargePosition = correctedBarge.transform.position;
        orbitalDistance2Planet = tempOrbitalDistance;
        bargeDistance2Planet = Vector3.Distance(correctedBarge.transform.position, orbitalBody.CenterOfMass.position);

        // // move barge to corrected location
        // // !! really messes with orbital physics !!
        // rb2d.MovePosition(tempOrbitalPos);
        // rb2d.rotation = tempOrbitalRot;
        // // OR
        // *************************************** END OF LIFTS-RIGHT-OUT


        rb2d.MovePosition(orbitalBody.Position);
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Delta-V transferred from a collision = the impulse along the contact normal / the mass of this body
        var ourMass = rb2d.mass;
        var contact = col.GetContact(0);
        var deltaV = contact.normalImpulse / ourMass * contact.normal;
        var deltaVMagnitude = deltaV.magnitude;
        if (deltaVMagnitude > MaxAllowedDeltaV)
        {
            Debug.LogWarning($"[ENTER] Δv: {deltaV}, |Δv|: {deltaV.magnitude}. LIMIT ENFORCED.");
            deltaV = deltaV * MaxAllowedDeltaV / deltaVMagnitude;
        }

        orbitalBody.AddDeltaV(Time.fixedTime, deltaV);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        var ourMass = rb2d.mass;
        var contact = col.GetContact(0);
        var deltaV = contact.normalImpulse / ourMass * contact.normal;
        var deltaVMagnitude = deltaV.magnitude;
        if (deltaVMagnitude > MaxAllowedDeltaV)
        {
            Debug.LogWarning($"[STAY]  Δv: {deltaV}, |Δv|: {deltaV.magnitude}. LIMIT ENFORCED.");
            deltaV = deltaV * MaxAllowedDeltaV / deltaVMagnitude;
        }

        orbitalBody.AddDeltaV(Time.fixedTime, deltaV);
    }
}