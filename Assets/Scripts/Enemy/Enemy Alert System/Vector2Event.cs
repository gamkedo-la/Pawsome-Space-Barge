using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Custom event to pass Vector2 location.
/// 
/// See:
/// https://www.monkeykidgc.com/2021/02/pass-parameters-with-scriptableobject-events-in-unity.html
/// </summary>
[System.Serializable]
public class Vector2Event : UnityEvent<Vector2> {}
