using UnityEngine;

/// <summary>
/// A game object that can be created from a particle and turned back into a particle
/// </summary>
public class ParticleDuality : MonoBehaviour
{
    [HideInInspector] private int layerMask;

    private ParticleSystem originatingSystem;
    private ParticleSystem.Particle originalParticle;
    private Rigidbody2D rb2d;
    private float speed;
    private Vector3 rotationalVelocity;
    private bool alive = false;
    private int orphanCheckFrame;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        speed = AsteroidField.Instance.RandomSpeed(transform.position);
        rotationalVelocity = AsteroidField.Instance.RandomRotationalVelocity();
        transform.rotation = Random.rotation;
        layerMask = LayerMask.GetMask("ActiveAsteroidZone");
        orphanCheckFrame = Random.Range(0, AsteroidField.Instance.OrphanInterval);
    }

    public void CreateFromParticle(ParticleSystem ps, ParticleSystem.Particle particle)
    {
        originatingSystem = ps;
        originalParticle = particle;

        rb2d.position = particle.position;
        rb2d.rotation = particle.rotation;
        rb2d.velocity = particle.velocity;
        rb2d.angularVelocity = particle.angularVelocity;

        alive = true;
    }

    public void TurnIntoParticle()
    {
        alive = false;

        originalParticle.position = rb2d.position;
        originalParticle.rotation = rb2d.rotation;
        originalParticle.velocity = Vector3.zero; // rb2d.velocity;
        originalParticle.angularVelocity = rb2d.angularVelocity;
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.particle = originalParticle;
        
        originatingSystem.Emit(emitParams, 1);
        
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (orphanCheckFrame == AsteroidField.Instance.FrameCounter)
        {
            if (!rb2d.IsTouchingLayers(layerMask))
            {
                Debug.Log($"Particle'izing orphan asteroid at: {transform.position.ToString()}, on frame: {orphanCheckFrame}");
                TurnIntoParticle();
            }
        }

        if (alive)
        {
            transform.Rotate(rotationalVelocity * Time.fixedDeltaTime);

            rb2d.transform.RotateAround(
                AsteroidField.Instance.planet,
                Vector3.forward,
                speed * Time.fixedDeltaTime
            );
        }
    }
}