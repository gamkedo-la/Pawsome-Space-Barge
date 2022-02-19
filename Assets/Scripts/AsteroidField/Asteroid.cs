using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asteroid Motion.
/// Handles asteroid orbit, rotational velocity, and respawn.
/// Attached to every orbiting object.
/// </summary>
public class Asteroid : MonoBehaviour
{
    /// <summary> Speed relative to overall field. </summary>
    [HideInInspector] private float speed;

    /// <summary> Rotational velocity for asteroid. </summary>
    [HideInInspector] private Vector3 rotationalVelocity;

    /// <summary> In case it ever becomes feasible to track a full orbit of asteroids. </summary>
    [SerializeField] private bool fullOrbit = false;
    
    /// <summary> Asteroid RigidBody2D. </summary>
    [HideInInspector] private Rigidbody2D rb2d;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }


    private void Start()
    {
        Randomize();
    }


    private void Update()
    {
        transform.Rotate(rotationalVelocity * Time.deltaTime);
    }


    private void FixedUpdate()
    {
        // this is the simplest solution
        rb2d.transform.RotateAround(
            AsteroidField.Instance.planet,
            Vector3.forward,
            ((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime
        );

        // // https://answers.unity.com/questions/10093/rigidbody-rotating-around-a-point-instead-on-self.html
        // // Works and produces more realistic collision rotations of asteroids on the barge,
        // // buuuut... this blocks the tug from interacting with the asteroids.
        // // Solution: make player kinematic type. But this raises further issues, lol.
        // Quaternion q = Quaternion.AngleAxis(((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime, Vector3.forward);
        // rb2d.MovePosition(q * (rb2d.transform.position - AsteroidField.Instance.planet) + AsteroidField.Instance.planet);
        // rb2d.MoveRotation(rb2d.transform.rotation * q);


        // // // this works too, modifying velocity instead... tug still cannot interact
        // Quaternion q = Quaternion.AngleAxis(((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime, Vector3.forward);
        // Vector2 newPosition = q * (rb2d.transform.position - AsteroidField.Instance.planet) + AsteroidField.Instance.planet;
        // Vector2 temp = newPosition - rb2d.position;
        // rb2d.velocity = temp * Mathf.Abs(AsteroidField.Instance.fieldSpeed);


        // // With RigidBody2D.AddForce, rather wonky
        // Quaternion q = Quaternion.AngleAxis(((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime, Vector3.forward);
        // Vector2 newPosition = q * (rb2d.transform.position - AsteroidField.Instance.planet) + AsteroidField.Instance.planet;
        // Vector2 temp = newPosition - rb2d.position;
        // rb2d.AddForce(temp * Mathf.Abs(AsteroidField.Instance.fieldSpeed*100f));
        // rb2d.MoveRotation(rb2d.transform.rotation * q);


        // // I have not yet tried this:
        // // https://forum.unity.com/threads/orbital-physics-maintaining-a-circular-orbit.403077/
        // float r = Vector3.Distance(star.Position, planet.Position);
        // float totalForce = -(Constants.G * star.Mass * planet.Mass) / (r * r);
        // Vector3 force = (planet.Position - star.Position).normalized * totalForce;
        // planet.GetComponent<Rigidbody>().AddForce(force);
    }


    /// <summary>
    /// Randomize asteroid speed, rotation, and relative velocity.
    /// </summary>
    private void Randomize()
    {
        speed = Random.Range(-AsteroidField.Instance.orbitalSpeedVariance, AsteroidField.Instance.orbitalSpeedVariance);
        
        transform.rotation = Random.rotation;

        rotationalVelocity = new Vector3(
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.Instance.maxAsteroidTumbleSpeed, AsteroidField.Instance.maxAsteroidTumbleSpeed)
        );
    }


    /// <summary>
    /// Respawn asteroids at opposite side of field.
    /// </summary>
    private void resetPosition()
    {
        // which side to spawn on
        float xPos = (rb2d.position.x > 0) ?
            Random.Range(-AsteroidField.Instance.fieldBounds.bounds.extents.x, -AsteroidField.Instance.fieldBounds.bounds.extents.x + 50) :
            Random.Range(AsteroidField.Instance.fieldBounds.bounds.extents.x, AsteroidField.Instance.fieldBounds.bounds.extents.x - 50);
        
        // y axis spawn
        float yPos = Random.Range(-AsteroidField.Instance.fieldBounds.bounds.extents.y + 150, AsteroidField.Instance.fieldBounds.bounds.extents.y - 150);

        // set new position
        transform.position = new Vector2(xPos, yPos);

        // re-randomize asteroid properties
        Randomize();
    }


    // not used atm, hopefully in future
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "AsteroidBoundary") {
            if (fullOrbit)
            {
                // enable physics and mesh rendering
            }
            else
            {
                //
            }
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "AsteroidBoundary") {
            if (fullOrbit)
            {
                // disable physics and mesh rendering
            }
            else
            {
                resetPosition();
            }
        }
    }
}
