using UnityEngine;

public class BargeCollision : MonoBehaviour
{
    [Tooltip("Select a prefab to spawn when you hit an asteroid!")]
    [SerializeField] private GameObject collision_FX_Prefab;


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Asteroid" && other.relativeVelocity.magnitude > 5 && this.collision_FX_Prefab)
        {
            Instantiate(this.collision_FX_Prefab, other.GetContact(0).point, this.transform.rotation);
        }
    }
}
