using UnityEngine;

public class CallbackBehaviour : StateMachineBehaviour
{
    [SerializeField] private EnemyAIState state;

    private void NotifyEnter(Animator animator)
    {
        animator.GetComponent<EnemyAIStateMachine>().OnStateEnter(state);
    }

    private void NotifyExit(Animator animator)
    {
        animator.GetComponent<EnemyAIStateMachine>().OnStateExit(state);
    }
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        NotifyEnter(animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        NotifyExit(animator);
    }
}