using UnityEngine;

/// <summary>
/// Manages SphereCollider for player ships.
/// Creates and adds collider when initialized.
/// </summary>
class PlayerSpawnCollider : MonoBehaviour
{
    public SphereCollider playerCollider { get; private set; }
    [SerializeField, Tooltip("Spawning zone center.")] private Vector3 center;
    [SerializeField, Tooltip("Spawning zone radius.")] private float radius;

    private void Awake()
    {
        if (!AsteroidField.Instance.IndividualSpawnZones)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }

    public SphereCollider InitializePlayerSpawnArea()
    {
        playerCollider = gameObject.AddComponent<SphereCollider>();
        playerCollider.radius = radius;
        playerCollider.center = center;

        return playerCollider;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            gameObject.transform.parent.position + gameObject.transform.parent.transform.right * center.magnitude,
            radius
        );
    }
}
