using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private const float MaxAllowedDeltaV = 0.2f;
    public Vector3 orbitalVelocity;
    public float orbitalVMag;
    public Vector3 previousOrbitalPosition;
    public Vector3 previousBargePosition;
    public float orbitalDistance2Planet;
    public float bargeDistance2Planet;

    public GameObject correctedBarge;
    // private Rigidbody2D bargeCollider;

    private OrbitalState orbitalState;
    private enum OrbitalState
    {
        Descending,
        Stable,
        Ascending
    }

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();

        correctedBarge.transform.position = orbitalBody.Position;
        // bargeCollider = correctedBarge.GetComponent<Rigidbody2D>();

        // orbitalVelocity = Vector3.Dot(orbitalBody.Velocity, orbitalBody.PositionPci);
        orbitalVelocity = orbitalBody.Velocity;
        orbitalVMag = orbitalVelocity.magnitude;

        previousOrbitalPosition = orbitalBody.Position;
        previousBargePosition = correctedBarge.transform.position;

        orbitalDistance2Planet = Vector3.Distance(orbitalBody.Position, orbitalBody.CenterOfMass.position);
        bargeDistance2Planet = Vector3.Distance(correctedBarge.transform.position, orbitalBody.CenterOfMass.position);
    }

    private void FixedUpdate()
    {
        orbitalBody.Recalculate(Time.fixedTime);
        rb2d.velocity = orbitalBody.Velocity;


        // gather data
        var tempOrbitalPos = orbitalBody.Position;
        var tempOrbitalRot = orbitalBody.ProgradeRotation;
        var tempOrbitalDistance = Vector3.Distance(tempOrbitalPos, orbitalBody.CenterOfMass.position);
        var diff = tempOrbitalPos - previousOrbitalPosition;

        orbitalVelocity = orbitalBody.Velocity;
        orbitalVMag = orbitalVelocity.magnitude;

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

        // apply corrected position
        // correctedBarge.transform.position = correctedOrbitalPosition;
        correctedBarge.transform.position = new Vector3(0, -Mathf.Abs(tempOrbitalDistance), 0);


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