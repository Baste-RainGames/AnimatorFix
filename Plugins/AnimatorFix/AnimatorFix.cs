using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// This combines with StateMachineBehaviourFix to allow OnStateMachineExit to function properly.
/// </summary>
public class AnimatorFix : MonoBehaviour, ISerializationCallbackReceiver {

    private Animator animator;
    private Dictionary<int, StateInfo>[] hashToStateInfo;
    private int lastNameHash = -1;
    private StateInfo lastStateInfo;
    private StateMachineBehaviourFix[] behavioursFixToExit;

    public void OnBeforeSerialize() {
#if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode) {
            var anim = GetComponent<Animator>();

            var serializedObject = new SerializedObject(anim);
            var serializedProperty = serializedObject.FindProperty("m_Controller");
            var objectReferenceValue = serializedProperty.objectReferenceValue;

            if (objectReferenceValue == null) {
                // This happens in one of the many possible OnBeforeSerialize steps that happen
                // if you recompile with the controller open. Other OnBeforeSerialize steps saves
                // the correct data. We don't do null-checks on the serializedProperty - if that's null,
                // something internally is broken and this script must be rewritten.

                // Of course, the object reference value is null if the controller isn't assigned. That's cool too.
                return;
            }
            AnimatorController controller = (AnimatorController) objectReferenceValue;
            

            hashToStateInfo = new Dictionary<int, StateInfo>[controller.layers.Length];
            for (int i = 0; i < controller.layers.Length; i++) {
                hashToStateInfo[i] = new Dictionary<int, StateInfo>();
            }

            for (int i = 0; i < controller.layers.Length; i++) {
                var layer = controller.layers[i];
                FindStateInfoRecursively(layer.stateMachine, hashToStateInfo[i], new List<AnimatorStateMachine>());
            }

            SerializeHashToStateInfoDict();
        }
#endif
    }

    public void OnAfterDeserialize() {
        DeserializeHashToStateInfoDict();
    }

    private void Awake() {
        animator = GetComponent<Animator>();
    }

#if UNITY_EDITOR
    private static void FindStateInfoRecursively(AnimatorStateMachine stateMachine, Dictionary<int, StateInfo> dict,
                                                 List<AnimatorStateMachine> stateMachineStack) {
        stateMachineStack.Add(stateMachine);

        foreach (var state in stateMachine.states) {
            var actualState = state.state;
            int hash = actualState.nameHash;

            if (!dict.ContainsKey(hash)) {
                dict[hash] = new StateInfo(actualState.name);
            }

            foreach (var machine in stateMachineStack) {
                foreach (var behaviour in machine.behaviours) {
                    var smartBehaviour = behaviour as StateMachineBehaviourFix;
                    if (smartBehaviour) {
                        smartBehaviour.parentMachine = machine.name;
                    }
                }
                dict[hash].containingStateMachines.Add(machine.name);
            }
        }

        foreach (var machine in stateMachine.stateMachines) {
            FindStateInfoRecursively(machine.stateMachine, dict, stateMachineStack);
        }
        stateMachineStack.Remove(stateMachine);
    }
#endif

    private void Update() {
        if (hashToStateInfo == null) {
            Debug.LogWarning("missing information about the Animator on " + name + ", please update the prefab/object");
            return;
        }

        if (behavioursFixToExit == null)
            behavioursFixToExit = animator.GetBehaviours<StateMachineBehaviourFix>();

        var currentNameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (lastNameHash == -1) {
            lastNameHash = currentNameHash;
            lastStateInfo = hashToStateInfo[0][lastNameHash];

        }

        if (currentNameHash != lastNameHash) {
            if (!hashToStateInfo[0].ContainsKey(currentNameHash)) {
                Debug.LogWarning("missing information about the Animator on " + name + ", please update the prefab/object");
                return;
            }

            StateInfo currentStateInfo = hashToStateInfo[0][currentNameHash];

            for (int i = 0; i < lastStateInfo.containingStateMachines.Count; i++) {
                var machine = lastStateInfo.containingStateMachines[i];
                if (!currentStateInfo.containingStateMachines.Contains(machine)) {
                    for (int j = 0; j < behavioursFixToExit.Length; j++) {
                        var behaviour = behavioursFixToExit[j];
                        if (behaviour.parentMachine == machine) {
                            try {
                                behaviour.OnStateMachineExit(animator);
                            }
                            catch {
                                Debug.LogWarning("error in state machine " + machine);
                                throw;
                            }
                        }
                    }
                }
            }

            lastNameHash = currentNameHash;
            lastStateInfo = currentStateInfo;
        }

    }

    public bool IsInSubStateMachine(string name, int layerIndex) {
        return GetCurrentStateInfo(layerIndex).containingStateMachines.Contains(name);
    }

    public string GetCurrentStateName(int layerIndex) {
        return GetCurrentStateInfo(layerIndex).stateName;
    }

    public StateInfo GetCurrentStateInfo(int layerIndex) {
        return hashToStateInfo[layerIndex][animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash];
    }

    [SerializeField]
    private int numLayers;
    [SerializeField]
    private IntStateInfoPair[] layer0Serialized;
    [SerializeField]
    private IntStateInfoPair[] layer1Serialized;
    [SerializeField]
    private IntStateInfoPair[] layer2Serialized;
    [SerializeField]
    private IntStateInfoPair[] layer3Serialized;

    public void SerializeHashToStateInfoDict() {
        numLayers = hashToStateInfo.Length;
        for (int i = 0; i < hashToStateInfo.Length; i++) {
            if (i > 3)
                break;
            IntStateInfoPair[] arr = null;

            if (i == 0) {
                layer0Serialized = new IntStateInfoPair[hashToStateInfo[i].Count];
                arr = layer0Serialized;
            }

            if (i == 1) {
                layer1Serialized = new IntStateInfoPair[hashToStateInfo[i].Count];
                arr = layer1Serialized;
            }

            if (i == 2) {
                layer2Serialized = new IntStateInfoPair[hashToStateInfo[i].Count];
                arr = layer2Serialized;
            }

            if (i == 3) {
                layer3Serialized = new IntStateInfoPair[hashToStateInfo[i].Count];
                arr = layer3Serialized;
            }

            int cntr = 0;
            foreach (var keyValuePair in hashToStateInfo[i]) {
                arr[cntr++] = new IntStateInfoPair {
                    intVal = keyValuePair.Key,
                    stateInfo = keyValuePair.Value
                };
            }
        }
        if (numLayers < 4)
            layer3Serialized = null;
        if (numLayers < 3)
            layer2Serialized = null;
        if (numLayers < 2)
            layer1Serialized = null;
    }

    private void DeserializeHashToStateInfoDict() {
        hashToStateInfo = new Dictionary<int, StateInfo>[numLayers];
        for (int i = 0; i < numLayers; i++) {
            hashToStateInfo[i] = new Dictionary<int, StateInfo>();
            IntStateInfoPair[] arr = null;

            if (i == 0) {
                arr = layer0Serialized;
            }

            if (i == 1) {
                arr = layer1Serialized;
            }

            if (i == 2) {
                arr = layer2Serialized;
            }

            if (i == 3) {
                arr = layer3Serialized;
            }

            for (int j = 0; j < arr.Length; j++) {
                hashToStateInfo[i][arr[j].intVal] = arr[j].stateInfo;
            }
        }
    }

}

/// <summary>
/// Contains all the information about a state
/// </summary>
[System.Serializable]
public class StateInfo {

    public StateInfo(string stateName) {
        this.stateName = stateName;
        containingStateMachines = new List<string>();
    }

    public string stateName;
    public List<string> containingStateMachines;

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(stateName);
        foreach (var machine in containingStateMachines) {
            sb.Append("<-" + machine);
        }

        return sb.ToString();
    }
}

[Serializable]
public class IntStateInfoPair {
    public int intVal;
    public StateInfo stateInfo;
}
