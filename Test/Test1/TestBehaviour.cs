using UnityEngine;
using System.Collections;

public class TestBehaviour : StateMachineBehaviourFix {

    public override void StateEnter(Animator animator) {
        TestController.ReportStateEnter(stateName);
    }

    public override void StateUpdate(Animator animator) {
        TestController.ReportStateUpdate(stateName);
    }

    public override void StateExit(Animator animator) {
        TestController.ReportStateExit(stateName);
    }

    public override void StateMachineEnter(Animator animator) {
        TestController.ReportStateMachineEnter(stateName);
    }

    public override void StateMachineUpdate(Animator animator) {
        TestController.ReportStateMachineUpdate(stateName);
    }

    public override void StateMachineExit(Animator animator) {
        TestController.ReportStateMachineExit(stateName);
    }

}
