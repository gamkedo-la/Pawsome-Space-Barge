using UnityEngine;

namespace PlayerShip
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Thruster : MonoBehaviour
    {
        private ParticleSystem thrusterParticleSystem;
        private float defaultEmissionRateOverTime;

        private void Awake()
        {
            thrusterParticleSystem = GetComponent<ParticleSystem>();
            defaultEmissionRateOverTime = thrusterParticleSystem.emission.rateOverTime.constant;
            Off();
        }

        public void On(float amount)
        {
            AdjustRate(amount);
            if (thrusterParticleSystem.isStopped)
            {
                thrusterParticleSystem.Play();
            }
        }

        public void Off()
        {
            thrusterParticleSystem.Stop();
        }

        private void AdjustRate(float amount)
        {
            var emissionModule = thrusterParticleSystem.emission;
            emissionModule.rateOverTime = Mathf.Abs(amount) * defaultEmissionRateOverTime;
        }
    }
}