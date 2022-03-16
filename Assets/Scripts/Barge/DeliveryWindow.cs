using UnityEngine;
using UnityEngine.Events;

public class DeliveryWindow : MonoBehaviour
{
    public UnityEvent onBargeEnter;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Barge"))
        {
            Debug.Log("Delivery window hit!");
            onBargeEnter.Invoke();
        }
    }
}