using UnityEngine;
using Variables;

namespace Mafia
{
    public class MafiaBarge : MonoBehaviour
    {
        public IntVariable missionType;
        public MinMaxIntVariable bargeHealth;
        
        [Tooltip("Maximum health")]
        [Min(1)] public int maxHealth = 1_000;
        
        [Tooltip("How sensitive is the barge to impact: 0=immune to impact")]
        [Range(0, 1)]
        public float impactSensitivity = 1f;

        [Tooltip("How sensitive is the barge to impact from the player tug: 0=immune to impact")]
        [Range(0, 1)]
        public float impactSensitivityPlayer = 0f;

        [Tooltip("Cargo model prefab")]
        public GameObject cargoPrefab;

        private void Awake()
        {
            // Remove this component if the mission type isn't Mafia
            if (missionType.Value != (int)MissionType.Mafia)
            {
                Destroy(this);
                return;
            }

            bargeHealth.MaxValue = maxHealth;
            bargeHealth.MinValue = 0;
            bargeHealth.Value = maxHealth;

            Instantiate(cargoPrefab, transform);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            var numberOfContacts = col.contactCount;
            var totalImpulse = 0f;
            for (var i = 0; i < numberOfContacts; i++)
            {
                totalImpulse += col.GetContact(i).normalImpulse;
            }

            var actualImpactSensitivity = col.gameObject.CompareTag("Player") ? impactSensitivityPlayer : impactSensitivity;
            
            var damage = Mathf.RoundToInt(totalImpulse * actualImpactSensitivity);

            if (damage > 0)
            {
                bargeHealth.Subtract(damage);

                if (bargeHealth.Value <= 0)
                {
                    GameManagement.Instance.MissionFailed();
                }
            }
        }
    }
}