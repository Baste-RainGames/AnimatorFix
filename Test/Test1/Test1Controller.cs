using System.Collections;
using System;

public class Test1Controller : TestController {

    private readonly Message[] _expectedOrder = {
        new Message(MessageType.StateEnter, "State 1"),
        new Message(MessageType.StateUpdate, "State 1"),
        new Message(MessageType.StateExit, "State 1"),
        new Message(MessageType.StateEnter, "State 2"),
        new Message(MessageType.StateUpdate, "State 2"),
        new Message(MessageType.StateExit, "State 2"),
        new Message(MessageType.StateEnter, "State 3"),
        new Message(MessageType.StateUpdate, "State 3"),
    };

    protected override IMessageMatcher[] expectedOrder {
        get { return _expectedOrder; }
    }
}
