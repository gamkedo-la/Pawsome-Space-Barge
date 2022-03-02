using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAssembler : MonoBehaviour
{
    [SerializeField] private RandomPrefabFactory asteroidFactory;
    
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
            var randomPrefab = asteroidFactory.GetRandomPrefab();
            var obj = Instantiate(randomPrefab, transform);
            var pd = obj.GetComponent<ParticleDuality>();
            if (pd == null)
            {
                Debug.LogWarning($"In {asteroidFactory.name}, prefab {randomPrefab.name} is missing ParticleDuality component");
                pd = obj.AddComponent<ParticleDuality>();
            }
            pd.CreateFromParticle(ps, particle);
        }
    }
}