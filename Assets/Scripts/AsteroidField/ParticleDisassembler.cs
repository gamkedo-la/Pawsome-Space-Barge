using UnityEngine;

public class ParticleDisassembler : MonoBehaviour
{
    [HideInInspector] private int layerMask;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("ActiveAsteroidZone");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // only trigger exit if not in an active zone
        // avoids asteroids rapidly spawning/despawning where areas overlap
        if (!other.IsTouchingLayers(layerMask))
        {
            ParticleDuality pd;

            if (other.gameObject.TryGetComponent<ParticleDuality>(out pd))
            {
                pd.TurnIntoParticle();
            }
        }
    }
}
