using UnityEngine;
using System.Collections;

public class Test1Behaviour : StateMachineBehaviourFix {

    public override void StateEnter(Animator animator) {
        Test1Controller.ReportOnStateEnter(stateName);
    }

    public override void StateUpdate(Animator animator) {
        Test1Controller.ReportOnStateUpdate(stateName);
    }

    public override void StateExit(Animator animator) {
        Test1Controller.ReportOnStateExit(stateName);
    }

}
