using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BargeCollisionFX : MonoBehaviour
{
    private BargeCollision barge;
    private void Start()
    {
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
