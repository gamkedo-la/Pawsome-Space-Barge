using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    // speed relative to overall field
    private float speed;
    
    // rotational velocity for asteroid
    private Vector3 rotationalVelocity;
    
    private Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        Randomize();
    }

    // randomize asteroid speed, rotation, and relative velocity
    private void Randomize()
    {
        speed = Random.Range(-AsteroidField.instance.orbitalSpeedVariance, AsteroidField.instance.orbitalSpeedVariance);
        
        transform.rotation = Random.rotation;

        rotationalVelocity = new Vector3(
            Random.Range(-AsteroidField.instance.maxAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.instance.maxAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed),
            Random.Range(-AsteroidField.instance.maxAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed)
        );
    }

    private void Update()
    {
        transform.Rotate(rotationalVelocity * Time.deltaTime);

        // check bounds, respawn asteroid if outside of
        if (!rb2d.IsTouching(AsteroidField.instance.fieldBounds))
        {
            resetPosition();
        }
        // orbit planet
        else
        {
            transform.RotateAround(
                AsteroidField.instance.planet,
                Vector3.forward,
                ((AsteroidField.instance.fieldSpeed / 100f) + speed) * Time.deltaTime
            );
        }
    }

    // private void FixedUpdate()
    // {
    //     // not used
    // }

    // respawn asteroids at start of field
    private void resetPosition()
    {
        // which side to spawn on
        float xPos = (rb2d.position.x > 0) ?
            Random.Range(-AsteroidField.instance.fieldXextent, -AsteroidField.instance.fieldXextent + 50) :
            Random.Range(AsteroidField.instance.fieldXextent, AsteroidField.instance.fieldXextent - 50);
        
        // y axis spawn
        float yPos = Random.Range(-AsteroidField.instance.fieldYextent + 150, AsteroidField.instance.fieldYextent - 150);

        // set new position
        rb2d.position = new Vector2(
            xPos,
            yPos
        );

        // re-randomize asteroid properties
        Randomize();
    }
}
