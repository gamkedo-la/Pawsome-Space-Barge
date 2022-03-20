using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class EnemyAIStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType;
    [Tooltip("Maximum range at which the barge can be detected")]
    [SerializeField][Min(0f)] private float bargeDetectionRange = 500f;
    [Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [SerializeField] [Min(0f)] private float bargeContactRange = 20f;
    [Tooltip("Required time to complete a scan for the barge")]
    [SerializeField] [Min(0.1f)] private float scanningTime = 1f;

    [Header("Events")] 
    [SerializeField] private UnityEvent onIdleEnter;
    [SerializeField] private UnityEvent onIdleExit;
    [SerializeField] private UnityEvent onSeekEnter;
    [SerializeField] private UnityEvent onSeekExit;
    [SerializeField] private UnityEvent onContactEnter;
    [SerializeField] private UnityEvent onContactExit;
    
    private Animator animator;
    private GameObject barge;
    
    private static readonly int EnemyTypeParameter = Animator.StringToHash("EnemyType");
    private static readonly int BargeDetectedParameter = Animator.StringToHash("BargeDetected");
    private static readonly int BargeContactParameter = Animator.StringToHash("BargeContact");

    public GameObject Barge => barge;

    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ScanForBarge());
        animator.SetInteger(EnemyTypeParameter, (int)enemyType);
        animator.SetBool(BargeDetectedParameter, false);
        animator.SetBool(BargeContactParameter, false);
    }

    private void Update()
    {
        animator.SetBool(BargeDetectedParameter, barge != null);
        animator.SetBool(BargeContactParameter, IsBargeContact());
    }

    private bool IsBargeContact()
    {
        return barge != null && Vector3.Distance(transform.position, barge.transform.position) <= bargeContactRange;
    }

    private IEnumerator ScanForBarge()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanningTime);
            barge = GameObject.FindGameObjectWithTag("Barge");
            if (barge == null) continue;
            
            if (Vector3.Distance(transform.position, barge.transform.position) > bargeDetectionRange)
            {
                barge = null;
            }
        }
    }

    public void OnStateEnter(EnemyAIState state)
    {
        switch (state)
        {
            case EnemyAIState.Idle:
                onIdleEnter.Invoke();
                break;
            case EnemyAIState.Seek:
                onSeekEnter.Invoke();
                break;
            case EnemyAIState.Contact:
                onContactEnter.Invoke();
                break;
            default:
                Debug.LogError($"EnemyAIStateMachine: Not implemented: {state}");
                break;
        }
    }

    public void OnStateExit(EnemyAIState state)
    {
        switch (state)
        {
            case EnemyAIState.Idle:
                onIdleExit.Invoke();
                break;
            case EnemyAIState.Seek:
                onSeekExit.Invoke();
                break;
            case EnemyAIState.Contact:
                onContactExit.Invoke();
                break;
            default:
                Debug.LogError($"EnemyAIStateMachine: Not implemented: {state}");
                break;
        }
    }
}