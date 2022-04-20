using UnityEngine;
using Variables;

public abstract class Barge : MonoBehaviour
{
    public abstract MissionType type { get; }
    public IntVariable missionType;
    public MinMaxIntVariable bargeHealth;
    
    [Tooltip("Maximum health")]
    [Min(1)] public int maxHealth;
    
    [Tooltip("How sensitive is the barge to impact: 0=immune to impact")]
    [Range(0, 1)]
    public float impactSensitivity;

    [Tooltip("How sensitive is the barge to impact from the player tug: 0=immune to impact")]
    [Range(0, 1)]
    public float impactSensitivityPlayer;

    [Tooltip("Cargo model prefab")]
    public GameObject cargoPrefab;
    private GameObject cargo;

    public void InitializeBarge()
    {
        // Disable this component if the mission type isn't Mafia,
        // and destroy caro mesh.
        if (missionType.Value != (int)type)
        {
            if (cargo != null)
            {
                Destroy(cargo);
            }

            enabled = false;
            return;
        }

        enabled = true;

        bargeHealth.MaxValue = maxHealth;
        bargeHealth.MinValue = 0;
        bargeHealth.Value = maxHealth;

        cargo = Instantiate(cargoPrefab, transform);
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
            ApplyDamage(damage);
        }
    }


    /// <summary>
    /// Apply damage to barge.
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int damage)
    {
        bargeHealth.Subtract(damage);

        if (bargeHealth.Value <= 0)
        {
            Debug.Log("Barge health == 0");
            GameManagement.Instance.MissionFailed();
        }
    }


    // accumulates float value for ApplyDamage
    float damageAccumulator = 0f;

    /// <summary>
    /// Accumulates a float value.
    /// Applies the damage to the barge when accumulator >= 1
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(float damage)
    {
        damageAccumulator += damage;

        var temp = Mathf.CeilToInt(damageAccumulator);

        if (temp > 0)
        {
            damageAccumulator -= temp;

            ApplyDamage(temp);
        }
    }
}
