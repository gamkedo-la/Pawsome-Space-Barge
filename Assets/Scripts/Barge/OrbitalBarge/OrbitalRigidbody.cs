using System;
using UnityEngine;

[RequireComponent(typeof(OrbitalBody), typeof(Rigidbody2D))]
public class OrbitalRigidbody : MonoBehaviour
{
    [SerializeField] private UpdateMethod method = UpdateMethod.FollowOrbit;
    [SerializeField] private float playerMultiplier = 2f;
    [SerializeField] private int maxContacts = 5;
    private ContactPoint2D[] contactArray;

    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private float CurrentTime => Time.fixedTime;

    private const float MaxAllowedDeltaV = 0.2f;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();
        rb2d.isKinematic = method != UpdateMethod.Forces;
        contactArray = new ContactPoint2D[maxContacts];
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
        // check for bad values, hope skipping a frame doesn't make it worse :)
        if (
            float.IsNaN(orbitalBody.Velocity.x) || float.IsNaN(orbitalBody.Velocity.y)
            || float.IsNaN(orbitalBody.Position.x) || float.IsNaN(orbitalBody.Position.y)
        )
        { return; }

        rb2d.velocity = orbitalBody.Velocity;    // recieves { NaN, NaN }
        rb2d.MovePosition(orbitalBody.Position); // recieves { NaN, NaN }
        rb2d.rotation = orbitalBody.ProgradeRotation;
    }

    private Vector2 GetDeltaV(Collision2D col)
    {
        // Delta-V transferred from a collision = the impulse along the contact normal / the mass of this body
        float ourMass = rb2d.mass;

        var contactNum = col.GetContacts(contactArray);

        float impulse = 0;
        Vector2 normal = new Vector2(0,0);

        for (int i = 0; i < contactNum; i++)
        {
            var newImpulse = contactArray[i].collider.CompareTag("Player")
                                ? contactArray[i].normalImpulse * playerMultiplier
                                : contactArray[i].normalImpulse;
                
            impulse += newImpulse;
            normal += contactArray[i].normal;
            
        }

        Vector2 deltaV = impulse / ourMass * normal.normalized;

        float deltaVMagnitude = deltaV.magnitude;

        // if (deltaVMagnitude > MaxAllowedDeltaV)
        // {
        //     Debug.LogWarning($"Δv: {deltaV}, |Δv|: {deltaV.magnitude}. LIMIT ENFORCED.");
        //     deltaV = deltaV * MaxAllowedDeltaV / deltaVMagnitude;
        // }

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