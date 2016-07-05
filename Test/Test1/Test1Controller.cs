using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Test1Controller : MonoBehaviour {

    public Text text;

    private Message[] expectedOrder = {
                                          new Message(MessageType.Enter, "State 1"),
                                          new Message(MessageType.Update, "State 1"),
                                          new Message(MessageType.Exit, "State 1"),
                                          new Message(MessageType.Enter, "State 2"),
                                          new Message(MessageType.Update, "State 2"),
                                          new Message(MessageType.Exit, "State 2"),
                                          new Message(MessageType.Enter, "State 3"),
                                          new Message(MessageType.Update, "State 3"),
                                      };

    private static Message lastMessage;
    private static Test1Controller instance;
    private static bool testEnded;

    private int currentIndex;

    void Awake() {
        instance = this;
        currentIndex = -1;
    }

    public static void ReportOnStateEnter(string name) {
        if (testEnded)
            return;
        instance.ReportState(new Message(MessageType.Enter, name));
    }

    public static void ReportOnStateUpdate(string name) {
        if (testEnded)
            return;
        instance.ReportState(new Message(MessageType.Update, name));
    }

    public static void ReportOnStateExit(string name) {
        if (testEnded)
            return;
        instance.ReportState(new Message(MessageType.Exit, name));
    }

    private void ReportState(Message m) {
        if (m == expectedOrder[currentIndex + 1]) {
            currentIndex++;
            text.text = "Moved to: " + m;
            Debug.Log("Successfully changed to " + m);
            if (currentIndex == expectedOrder.Length - 1) {
                text.text = "All messages received in expected order!";
                testEnded = true;
                return;
            }
        }
        else if (currentIndex != -1 && m == expectedOrder[currentIndex]) {
            return;
        }
        else {
            text.text = "Failed! Expected: " + expectedOrder[currentIndex + 1] + ", but got: " + m;
            testEnded = true;
            return;
        }
    }

    private class Message {

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return (Message) obj == this;
        }

        public override int GetHashCode() {
            unchecked { return ((int) type * 397) ^ (state != null ? state.GetHashCode() : 0); }
        }

        MessageType type;
        string state;

        public Message(MessageType type, string state) {
            this.type = type;
            this.state = state;
        }

        public override string ToString() {
            return "On state " + type + ": " + state;
        }

        public static bool operator ==(Message a, Message b) {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return false;
            return a.type == b.type && a.state == b.state;
        }

        public static bool operator !=(Message a, Message b) {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return true;
            return !(a == b);
        }

    }

    private enum MessageType {

        Enter,
        Update,
        Exit

    }

}
