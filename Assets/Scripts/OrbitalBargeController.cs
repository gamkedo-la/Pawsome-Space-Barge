using UnityEngine;

public class OrbitalBargeController : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

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
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Delta-V transferred from a collision = the impulse along the contact normal / the mass of this body
        var ourMass = rb2d.mass;
        var contact = col.GetContact(0);
        var deltaV = contact.normalImpulse / ourMass * contact.normal;

        orbitalBody.AddDeltaV(Time.fixedTime, deltaV);
    }
}