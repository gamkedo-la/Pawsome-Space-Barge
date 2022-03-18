using UnityEngine;

/// <summary>
/// Debris Motion.
/// </summary>
public class DebrisRotation : MonoBehaviour
{
    /// <summary> Speed relative to overall field. </summary>
    [HideInInspector] private float speed;

    /// <summary> Rotational velocity for asteroid. </summary>
    [HideInInspector] private Vector3 rotationalVelocity;

    private Rigidbody2D rb2d;


    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        
        speed = AsteroidField.Instance.RandomSpeed(transform.position);

        rotationalVelocity = AsteroidField.Instance.RandomRotationalVelocity();
        
        transform.rotation = Random.rotation;
    }


    private void FixedUpdate()
    {
        transform.Rotate(rotationalVelocity * Time.fixedDeltaTime);

        rb2d.transform.RotateAround(
            AsteroidField.Instance.planet,
            Vector3.forward,
            speed * Time.fixedDeltaTime
        );
    }
}
