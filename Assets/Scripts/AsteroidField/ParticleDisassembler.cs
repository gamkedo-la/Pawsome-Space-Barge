using UnityEngine;

public class ParticleDisassembler : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        var pd = other.gameObject.GetComponent<ParticleDuality>();
        if (pd != null)
        {
            pd.TurnIntoParticle();
        }
    }
}