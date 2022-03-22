using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class EnemyAIStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType;

    [Header("Events")] 
    [SerializeField] private UnityEvent onIdleEnter;
    [SerializeField] private UnityEvent onIdleExit;
    [SerializeField] private UnityEvent onSeekEnter;
    [SerializeField] private UnityEvent onSeekExit;
    [SerializeField] private UnityEvent onContactEnter;
    [SerializeField] private UnityEvent onContactExit;
    
    private Animator animator;

    private EnemyEngineSystem engines;
    public EnemyEngineSystem Engines => engines;
    private EnemyNavigationSystem navigation;
    public EnemyNavigationSystem Navigation => navigation;


    private static readonly int EnemyTypeParameter = Animator.StringToHash("EnemyType");
    private static readonly int BargeDetectedParameter = Animator.StringToHash("BargeDetected");
    private static readonly int BargeContactParameter = Animator.StringToHash("BargeContact");


    private void Awake()
    {
        animator = GetComponent<Animator>();
        engines = GetComponent<EnemyEngineSystem>();
        navigation = GetComponent<EnemyNavigationSystem>();
    }


    private void Start()
    {
        animator.SetInteger(EnemyTypeParameter, (int)enemyType);
        animator.SetBool(BargeDetectedParameter, false);
        animator.SetBool(BargeContactParameter, false);
    }

    private void Update()
    {
        animator.SetBool(BargeDetectedParameter, navigation.Barge != null);
        animator.SetBool(BargeContactParameter, navigation.IsBargeContact());
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