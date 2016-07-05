using UnityEngine;
using UnityEngine.Experimental.Director;

/// <summary>
/// Use this class instead of StateMachineBehaviour, and attach an AnimatorInfo to the same object as the Animator.
/// 
/// That will give you access to a proper OnStateMachineExit
/// </summary>
public abstract class StateMachineBehaviourFix : StateMachineBehaviour {

    [HideInInspector] public string parentMachine;
    [HideInInspector] public string stateName;
    [HideInInspector] public int stateHash;

    #region sealedOriginalMethods

    public sealed override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {}
    public sealed override void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) {}
    public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {}
    public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {}
    public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {}
    public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {}
    public sealed override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {}
    public sealed override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) {}
    public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {}
    public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {}

    #endregion

    #region customMethods

    public virtual void StateMachineExit(Animator animator) {}
    public virtual void StateMachineEnter(Animator animator) {}
    public virtual void StateEnter(Animator animator) {}
    public virtual void StateUpdate(Animator animator) {}
    public virtual void StateExit(Animator animator) {}

    #endregion
}
