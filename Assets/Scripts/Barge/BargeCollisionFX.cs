using UnityEngine;

public class BargeCollisionFX : MonoBehaviour
{
    // This should be on the parent object for correct function.
    private BargeCollision barge;


    private void Start()
    {
        // get main collision effects script
        barge = gameObject.GetComponentInParent<BargeCollision>();
    }


    /// <summary>
    /// Catches particle system callback, disables parent object.'
    /// Part of simple object pool in BargeCollision.cs
    /// </summary>
    public void OnParticleSystemStopped()
    {
        barge.ReturnEffect(gameObject.transform.parent.gameObject);
    }
}
