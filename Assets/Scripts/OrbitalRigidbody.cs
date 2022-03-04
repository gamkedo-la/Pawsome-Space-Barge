using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private const float MaxAllowedDeltaV = 0.2f;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();
    }

    private void FixedUpdate()
    {
        orbitalBody.Recalculate(Time.fixedTime);
        rb2d.velocity = orbitalBody.Velocity;
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