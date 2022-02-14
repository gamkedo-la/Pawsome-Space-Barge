using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    // private int fieldSpeed;
    private float speed;
    private Rigidbody2D rb2d;
    private Vector3 rotationalVelocity;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        Randomize();
    }

    // randomize asteroid speed, rotation, and velocity
    private void Randomize()
    {
        speed = Random.Range(-AsteroidField.instance.orbitalSpeedVariance, AsteroidField.instance.orbitalSpeedVariance);
        
        transform.rotation = Random.rotation;

        rotationalVelocity = new Vector3(
            Random.Range(AsteroidField.instance.minAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed),
            Random.Range(AsteroidField.instance.minAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed),
            Random.Range(AsteroidField.instance.minAsteroidTumbleSpeed, AsteroidField.instance.maxAsteroidTumbleSpeed)
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
                (float)AsteroidField.instance.fieldSpeed / (100f + speed) * Time.deltaTime
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
        float xPos = (rb2d.position.x > 0) ?
            Random.Range(-AsteroidField.instance.fieldXextent, -AsteroidField.instance.fieldXextent + 50) :
            Random.Range(AsteroidField.instance.fieldXextent, AsteroidField.instance.fieldXextent - 50);
        
        float yPos = Random.Range(-AsteroidField.instance.fieldYextent + 150, AsteroidField.instance.fieldYextent - 150);

        rb2d.position = new Vector2(
            xPos,
            yPos
        );

        Randomize();
    }
}
