using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BargeCollisionFX : MonoBehaviour
{
    /// <summary>
    /// Catches particle system callback, disables parent object.'
    /// Part of simple object pool in BargeCollision.cs
    /// </summary>
    public void OnParticleSystemStopped()
    {
        // disable parent object to "return" to pool
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
