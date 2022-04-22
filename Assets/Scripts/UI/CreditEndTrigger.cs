using UnityEngine;

public class CreditEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Credits Done!");
        CreditsManagement.Instance.CreditsDone();
    }
}
