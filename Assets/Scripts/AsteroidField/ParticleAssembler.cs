using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAssembler : MonoBehaviour
{
    [FormerlySerializedAs("asteroidPrefabs")] 
    public ParticleDuality[] prefabs;
    
    private ParticleSystem ps;
    private List<ParticleSystem.Particle> particles = new();

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        var numberOfParticles = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
        if (numberOfParticles > 0)
        {
            SpawnAsteroids(particles);
            for (var i = 0; i < numberOfParticles; i++)
            {
                var p = particles[i];
                p.remainingLifetime = 0f;
                particles[i] = p;
            }
            ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
        }
    }

    private void SpawnAsteroids(List<ParticleSystem.Particle> list)
    {
        foreach (var particle in list)
        {
            var pd = Instantiate(prefabs[Random.Range(0, prefabs.Length)], transform);
            pd.CreateFromParticle(ps, particle);
        }
    }
}