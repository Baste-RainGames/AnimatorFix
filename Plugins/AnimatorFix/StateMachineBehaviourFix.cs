using UnityEngine;
using UnityEngine.Experimental.Director;

/// <summary>
/// Use this class instead of StateMachineBehaviour, and attach an AnimatorInfo to the same object as the Animator.
/// 
/// That will give you access to a proper OnStateMachineExit
/// </summary>
public abstract class StateMachineBehaviourFix : StateMachineBehaviour {

    public string parentMachine;

    public override sealed void OnStateMachineExit(Animator animator, int stateMachinePathHash) { }

    public override sealed void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) { }

    public virtual void OnStateMachineExit(Animator animator) {}

}
