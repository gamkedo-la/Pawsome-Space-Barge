using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private int fieldSpeed;
    private float speed;
    private Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // get speed from AsteroidField script
        fieldSpeed = AsteroidField.instance.fieldSpeed;

        Randomize();
    }

    // randomize asteroid speed, rotation, and velocity
    private void Randomize()
    {
        speed = Random.Range(fieldSpeed*0.8f, fieldSpeed*1.2f);
        
        transform.rotation = Random.rotation;

        // TODO replace this by placing asteroids on a rotating parent object
        rb2d.velocity = new Vector2(speed, 0);
    }

    private void FixedUpdate()
    {
        // check bounds, respawn asteroid if outside of
        if (!rb2d.IsTouching(AsteroidField.instance.fieldBounds))
        {
            if (
                rb2d.position.x > AsteroidField.instance.fieldXextent ||
                rb2d.position.x < -AsteroidField.instance.fieldXextent ||
                rb2d.position.y > AsteroidField.instance.fieldYextent ||
                rb2d.position.y < -AsteroidField.instance.fieldYextent
            )
            {
                resetPosition();
            }
        }
    }

    // respawn asteroids at start of field
    private void resetPosition()
    {
        float xPos = Random.Range(-AsteroidField.instance.fieldXextent, -AsteroidField.instance.fieldXextent + 50);
        float yPos = Random.Range(-245, 245);

        rb2d.position = new Vector2(
            xPos,
            yPos
        );

        Randomize();
    }
}
