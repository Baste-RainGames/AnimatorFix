using UnityEngine;
using UnityEngine.UI;

public abstract class TestController : MonoBehaviour {

    [SerializeField] private bool logToConsole;

    protected abstract IMessageMatcher[] expectedOrder { get; }

    private Text text;
    private static Message lastMessage;
    private static TestController instance;
    private static bool testEnded;

    private int currentIndex;

    void Awake() {
        instance = this;
        currentIndex = -1;

        Canvas c = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject textHolder = new GameObject("Text");

        textHolder.transform.parent = c.transform;
        text = textHolder.AddComponent<Text>();
        text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        var gray = 50f / 360f;
        text.color = new Color(gray, gray, gray, 1f);
        text.fontSize = 195;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var rectTransform = textHolder.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one * .3f;

        rectTransform.sizeDelta = new Vector2(3000, 30);
        rectTransform.anchoredPosition = new Vector2(0, 150f);
    }

    public static void ReportStateEnter(string name) {
        instance.ReportState(new Message(MessageType.StateEnter, name));
    }

    public static void ReportStateUpdate(string name) {
        instance.ReportState(new Message(MessageType.StateUpdate, name));
    }

    public static void ReportStateExit(string name) {
        instance.ReportState(new Message(MessageType.StateExit, name));
    }

    public static void ReportStateMachineEnter(string name) {
        instance.ReportState(new Message(MessageType.StateEnter, name));
    }

    public static void ReportStateMachineUpdate(string name) {
        instance.ReportState(new Message(MessageType.SubStateMachineUpdate, name));
    }

    public static void ReportStateMachineExit(string name) {
        instance.ReportState(new Message(MessageType.SubStateMachineExit, name));
    }

    private void ReportState(Message m) {
        if(testEnded)
            return;

        if (expectedOrder[currentIndex + 1].Matches(m)) {
            currentIndex++;
            var stateChangeMsg = "Moved to: " + m;
            text.text = stateChangeMsg;
            if (logToConsole) {
                Debug.Log(stateChangeMsg);
            }
            if (currentIndex == expectedOrder.Length - 1) {
                var successMsg = "All messages received in expected order!";
                if (logToConsole) {
                    Debug.Log(successMsg);
                }
                text.text = successMsg;
                testEnded = true;
                return;
            }
        }
        else if (currentIndex != -1 && expectedOrder[currentIndex].Matches(m)) {
            return;
        }
        else {
            var failMsg = expectedOrder[currentIndex + 1].FailMessageFor(m);
            if (logToConsole) {
                Debug.Log(failMsg);
            }
            text.text = failMsg;
            testEnded = true;
            return;
        }
    }

    protected interface IMessageMatcher {
        bool Matches(Message message);

        string FailMessageFor(Message message);
    }

    protected class Message : IMessageMatcher {

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
            return type + ": " + state;
        }

        public bool Matches(Message message) {
            return this == message;
        }

        public string FailMessageFor(Message message) {
            return "Failed! Expected: " + this + ", but got: " + message;
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

    protected class OrderedMessages : IMessageMatcher {
        private Message[] messages;
        private int currentIndex = 0;

        public OrderedMessages(params Message[] messages) {
            if (messages== null || messages.Length == 0) {
                throw new UnityException("OrderedMessages requires at least one message to work!");
            }
            this.messages = messages;
        }

        public bool Matches(Message message) {
            bool matches = message == messages[currentIndex];
            currentIndex = (currentIndex + 1) % messages.Length;
            return matches;
        }

        public string FailMessageFor(Message message) {
            return "Failed! Expected: " + messages[currentIndex] + ", but got: " + message;
        }
    }

    protected enum MessageType {

        StateEnter,
        StateUpdate,
        StateExit,
        SubStateMachineEnter,
        SubStateMachineUpdate,
        SubStateMachineExit

    }

}
