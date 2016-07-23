using UnityEngine;
using System.Collections;

public class Test2Controller : TestController {

    private IMessageMatcher[] _expectedOrder = {
        new Message(MessageType.StateEnter, "State 1"), 
        new Message(MessageType.StateUpdate, "State 1"), 
        new Message(MessageType.StateExit, "State 1"),

        new Message(MessageType.SubStateMachineEnter, "SubStateMachine"),

        new Message(MessageType.StateEnter, "State 2"),
        new OrderedMessages(
            new Message(MessageType.StateUpdate, "State 2"),
            new Message(MessageType.SubStateMachineUpdate, "SubStateMachine")), 
        new Message(MessageType.StateExit, "State 2"),

        new Message(MessageType.SubStateMachineExit, "SubStateMachine"),

        new Message(MessageType.StateEnter, "State 3"),
        new Message(MessageType.StateUpdate, "State 3"),
    };

    protected override IMessageMatcher[] expectedOrder {
        get { return _expectedOrder; }
    }

}
