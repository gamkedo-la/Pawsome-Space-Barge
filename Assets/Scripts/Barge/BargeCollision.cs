using System.Collections.Generic;
using UnityEngine;

public class BargeCollision : MonoBehaviour
{
    [Tooltip("Select a prefab to spawn when you hit an asteroid!")]
    [SerializeField] private GameObject collision_FX_Prefab;

    [Tooltip("Size of effect pool.")]
    [SerializeField] private int poolSize = 5;

    [Tooltip("Collision magnitude threshold for particle effect.")]
    [SerializeField] private int collisionMagnitude = 5;

    private List<GameObject> effectPool;


    private void Start()
    {
        // initialize and fill pool
        effectPool = new List<GameObject>();

        GameObject temp;

        for (int i = 0; i < poolSize; i++) {
            temp = Instantiate(collision_FX_Prefab, gameObject.transform);
            temp.SetActive(false);
            effectPool.Add(temp);
        }
    }


    /// <summary>
    /// Implements simple object pool withdrawl.
    /// 
    /// Objects returned to pool when BargeCollisionFX.cs disables object.
    /// </summary>
    /// <returns>Pooled object or null.</returns>
    private GameObject Withdraw()
    {
        GameObject pooledObject = null;

        // look for disabled objects
        for (int i = 0; i < effectPool.Count; i++)
        {
            if (!effectPool[i].activeInHierarchy)
            {
                pooledObject = effectPool[i];
            }
        }

        // all pooled objects active
        if (pooledObject == null)
        {
            // instantiate a new one?
        }

        return pooledObject;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if ( // hit asteroid hard enough
            other.gameObject.tag == "Asteroid"
            && other.relativeVelocity.magnitude > collisionMagnitude
            && this.collision_FX_Prefab
        )
        {
            var contact = other.GetContact(0);

            // null return catch for Withdraw()
            try
            {
                var temp = Withdraw();
                temp.transform.position = contact.point;
                temp.transform.rotation = Quaternion.FromToRotation(transform.right, contact.normal);
                temp.SetActive(true);
            }
            catch
            {
                Debug.Log("Collision fx pool empty.");
            }
            
        }
    }
}
