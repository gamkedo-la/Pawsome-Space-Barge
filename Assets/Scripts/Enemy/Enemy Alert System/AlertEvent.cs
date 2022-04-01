using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for enemy alert events.
/// 
/// See:
/// PoliceAlertEvent and PirateAlertEvent
/// 
/// See:
/// https://www.monkeykidgc.com/2021/02/pass-parameters-with-scriptableobject-events-in-unity.html
/// </summary>
public partial class AlertEvent : ScriptableObject
{
    [ReadOnly][SerializeField]
    private List<AlertEventListener> listeners = new List<AlertEventListener>();

    // call this to trigger
    public void TriggerEvent(Vector2 position)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventTriggered(position);
        }
    }

    public void AddListener(AlertEventListener listener)
    {
        listeners.Add(listener);
    }

    public void RemoveListener(AlertEventListener listener)
    {
        listeners.Remove(listener);
    }
}
