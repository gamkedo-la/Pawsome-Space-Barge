using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ***** ARCHIVED *****

/// <summary>
/// Asteroid Motion.
/// Handles asteroid orbit, rotational velocity, and respawn.
/// Attached to every orbiting object.
/// </summary>
public class Asteroid : MonoBehaviour
{
    // /// <summary> Speed relative to overall field. </summary>
    // [HideInInspector] private float speed;

    // /// <summary> Rotational velocity for asteroid. </summary>
    // [HideInInspector] private Vector3 rotationalVelocity;

    // /// <summary> In case it ever becomes feasible to track a full orbit of asteroids. </summary>
    // [SerializeField] private bool fullOrbit = false;
    
    // /// <summary> Asteroid RigidBody2D. </summary>
    // [HideInInspector] private Rigidbody2D rb2d;

    // /// <summary> Integer for 'Asteroids' layer. </summary>
    // [HideInInspector] private int layerMask;

    // private CircleCollider2D collider2d;
    // MeshFilter filter;


    // private void Awake()
    // {
    //     rb2d = GetComponent<Rigidbody2D>();
    //     layerMask = LayerMask.GetMask("Asteroids");
    //     collider2d = GetComponent<CircleCollider2D>();
    //     filter = GetComponent<MeshFilter>();
    // }


    // private void Start()
    // {
    //     Randomize();
    // }


    // private void Update()
    // {
    //     transform.Rotate(rotationalVelocity * Time.deltaTime);
    // }


    // private void FixedUpdate()
    // {
    //     // this is the simplest solution
    //     rb2d.transform.RotateAround(
    //         AsteroidField.Instance.planet,
    //         Vector3.forward,
    //         ((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime
    //     );

    //     // // https://answers.unity.com/questions/10093/rigidbody-rotating-around-a-point-instead-on-self.html
    //     // // Works and produces more realistic collision rotations of asteroids on the barge,
    //     // // buuuut... this blocks the tug from interacting with the asteroids.
    //     // // Solution: make player kinematic type. But this raises further issues, lol.
    //     // Quaternion q = Quaternion.AngleAxis(((AsteroidField.Instance.fieldSpeed / 100f) + speed) * Time.deltaTime, Vector3.forward);
    //     // rb2d.MovePosition(q * (rb2d.transform.position - AsteroidField.Instance.planet) + AsteroidField.Instance.planet);
    //     // rb2d.MoveRotation(rb2d.transform.rotation * q);
    // }


    // /// <summary>
    // /// Randomize asteroid speed, rotation, and relative velocity.
    // /// </summary>
    // private void Randomize()
    // {
    //     speed = AsteroidField.Instance.RandomSpeed(transform.position);
        
    //     transform.rotation = Random.rotation;

    //     rotationalVelocity = AsteroidField.Instance.RandomRotationalVelocity();

    //     AsteroidDebrisData randomThing = AsteroidField.Instance.RandomDebris();
    //     if (randomThing.prefabMesh.name != filter.sharedMesh.name)
    //     {
    //         rb2d.mass = randomThing.mass;
    //         collider2d.radius = randomThing.radius;
    //         filter.sharedMesh = randomThing.prefabMesh;
    //     }
    // }


    // /// <summary>
    // /// Respawn asteroids at opposite side of field.
    // /// </summary>
    // private void resetPosition()
    // {
    //     bool spotClear = false;
    //     Vector2 newPos = new Vector2(0,0);
    //     int maxIterations = 100;

    //     // If we fail to place the asteroid after 100 tries, just drop it somewhere
    //     while (spotClear == false && --maxIterations >= 0)
    //     {
    //         newPos = AsteroidField.Instance.RandomSpawnPosition();
    //         if (rb2d.position.x > 0)
    //         {
    //             newPos.x = -newPos.x;
    //         }

    //         // WARNING
    //         // the OverlapCircle() check locks up the engine if the LayerMask is not properly set
    //         // as the asteroids are always in the asteroid boundary this never comes back false
    //         if (layerMask >= 0)
    //         {
    //             // check for overlap with other asteroids
    //             if (!Physics2D.OverlapCircle(newPos, collider2d.radius, layerMask)) 
    //             {
    //                 spotClear = true;
    //             }
    //         }
    //         else
    //         {
    //             Debug.Log("Asteroid LayerMask Error!\nPlease ensure asteroids are marked with layer named 'Asteroids'.\nAsteroid overlap on respawn not being checked.");

    //             // switch to true to exit the check
    //             spotClear = true;
    //         }
    //     }

    //     if (!spotClear)
    //     {
    //         Debug.LogWarning("Unable to place asteroid after 100 tries, probably means something is wrong.");
    //     }

    //     // set new position
    //     transform.position = newPos;

    //     // re-randomize asteroid properties
    //     Randomize();
    // }


    // // not used atm, hopefully in future
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.tag == "AsteroidBoundary") {
    //         if (fullOrbit)
    //         {
    //             // enable physics and mesh rendering
    //         }
    //         else
    //         {
    //             // nothing
    //         }
    //     }
    // }


    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.tag == "AsteroidBoundary") {
    //         if (fullOrbit)
    //         {
    //             // disable physics and mesh rendering
    //         }
    //         else
    //         {
    //             resetPosition();
    //         }
    //     }
    // }
}
