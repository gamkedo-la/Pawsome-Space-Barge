using UnityEngine;


/// <summary>
/// Event listener behaviour.
/// 
/// Attach to game object to send recieve alerts via AlertEvent.
/// 
/// See:
/// https://www.monkeykidgc.com/2021/02/pass-parameters-with-scriptableobject-events-in-unity.html
/// </summary>
public class AlertEventListener : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;

    [ReadOnly][SerializeField]
    public AlertEvent alertEvent;

    public Vector2Event onEventTriggered;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();
        alertEvent = GameManagement.Instance.GetAlertNetwork(enemyAI.Type);
    }

    private void OnEnable()
    {
        alertEvent.AddListener(this);
    }

    private void OnDisable()
    {
        alertEvent.RemoveListener(this);
    }

    public void OnEventTriggered(Vector2 position)
    {
        onEventTriggered.Invoke(position);
    }
}
