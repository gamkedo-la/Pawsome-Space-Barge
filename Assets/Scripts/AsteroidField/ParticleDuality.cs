using UnityEngine;

/// <summary>
/// A game object that can be created from a particle and turned back into a particle
/// </summary>
public class ParticleDuality : MonoBehaviour
{
    private ParticleSystem originatingSystem;
    private ParticleSystem.Particle originalParticle;
    private Rigidbody2D rb2d;
    private float speed;
    private bool alive = false;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        speed = AsteroidField.Instance.RandomSpeed(transform.position);
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
        originalParticle.velocity = rb2d.velocity;
        originalParticle.angularVelocity = rb2d.angularVelocity;
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.particle = originalParticle;
        
        originatingSystem.Emit(emitParams, 1);
        
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            rb2d.transform.RotateAround(
                AsteroidField.Instance.planet,
                Vector3.forward,
                speed * Time.deltaTime
            );
        }
    }
}