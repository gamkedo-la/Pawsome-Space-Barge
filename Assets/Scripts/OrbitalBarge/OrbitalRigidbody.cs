using System;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    [SerializeField] private UpdateMethod method = UpdateMethod.FollowOrbit;
    [SerializeField] private float playerMultiplier = 2f;

    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private float CurrentTime => Time.fixedTime;

    private const float MaxAllowedDeltaV = 0.2f;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();
        rb2d.isKinematic = method != UpdateMethod.Forces;
    }

    private void Start()
    {
        orbitalBody.Recalculate(0);
        FollowOrbit();
    }

    private void FixedUpdate()
    {
        orbitalBody.Recalculate(CurrentTime);
        if (method == UpdateMethod.FollowOrbit)
        {
            FollowOrbit();
        }
        else
        {
            UseForces();
        }
    }

    private void UseForces()
    {
        orbitalBody.SetOrbit(CurrentTime, rb2d.position, rb2d.velocity);
        var gravity = orbitalBody.GravitationalForce * rb2d.mass;
        rb2d.AddForce(gravity, ForceMode2D.Force);
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private void FollowOrbit()
    {
        rb2d.velocity = orbitalBody.Velocity;
        rb2d.MovePosition(orbitalBody.Position);
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private Vector2 GetDeltaV(Collision2D col)
    {
        // Delta-V transferred from a collision = the impulse along the contact normal / the mass of this body
        float ourMass = rb2d.mass;
        ContactPoint2D contact = col.GetContact(0);

        float scale = (col.gameObject.CompareTag("Player")) ? playerMultiplier : 1f;

        Vector2 deltaV = contact.normalImpulse * scale / ourMass * contact.normal;

        float deltaVMagnitude = deltaV.magnitude;

        if (deltaVMagnitude > MaxAllowedDeltaV)
        {
            Debug.LogWarning($"Δv: {deltaV}, |Δv|: {deltaV.magnitude}. LIMIT ENFORCED.");
            deltaV = deltaV * MaxAllowedDeltaV / deltaVMagnitude;
        }

        return deltaV;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (method == UpdateMethod.Forces)
        {
            return;
        }

        orbitalBody.AddDeltaV(CurrentTime, GetDeltaV(col));
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (method == UpdateMethod.Forces)
        {
            return;
        }

        orbitalBody.AddDeltaV(CurrentTime, GetDeltaV(col));
    }

    [Serializable]
    public enum UpdateMethod
    {
        Forces,
        FollowOrbit,
    }
}