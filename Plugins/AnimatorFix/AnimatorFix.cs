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

    private Dictionary<int, StateInfo>[] stateInfos;
    private Dictionary<int, StateMachineBehaviourFix> behaviours;

    private Animator animator;
    private int lastNameHash = -1;
    private StateInfo lastStateInfo;

    public void OnBeforeSerialize() {
#if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode) {
            var anim = GetComponent<Animator>();

            var serializedObject = new SerializedObject(anim);
            var animatorControllerProperty = serializedObject.FindProperty("m_Controller");
            var animatorControllerObject = animatorControllerProperty.objectReferenceValue;

            if (animatorControllerObject == null) {
                // This happens in one of the many possible OnBeforeSerialize steps that happen
                // if you recompile with the controller open. Other OnBeforeSerialize steps saves
                // the correct data. We don't do null-checks on the serializedProperty - if that's null,
                // Unity changed the name of the AnimatorController field and this script must be rewritten.

                // Of course, the object reference value is null if the controller isn't assigned. That's cool too.
                return;
            }
            AnimatorController controller = (AnimatorController) animatorControllerObject;

            stateInfos = new Dictionary<int, StateInfo>[controller.layers.Length];
            for (int i = 0; i < controller.layers.Length; i++) {
                stateInfos[i] = new Dictionary<int, StateInfo>();
            }

            for (int i = 0; i < controller.layers.Length; i++) {
                var layer = controller.layers[i];
                FindStateInfoRecursively(layer.stateMachine, stateInfos[i], new List<AnimatorStateMachine>());
            }

            SerializeHashToStateInfoDict();
        }
#endif
    }

#if UNITY_EDITOR
    private static void FindStateInfoRecursively(AnimatorStateMachine stateMachine, Dictionary<int, StateInfo> dict,
                                                 List<AnimatorStateMachine> stateMachineStack) {
        stateMachineStack.Add(stateMachine);

        foreach (var state in stateMachine.states) {
            AnimatorState actualState = state.state;
            int hash = actualState.nameHash;

            if (!dict.ContainsKey(hash)) {
                dict[hash] = new StateInfo(actualState.name);
            }

            foreach (var behaviour in actualState.behaviours) {
                var behaviourFix = behaviour as StateMachineBehaviourFix;
                if (behaviourFix != null) {
                    behaviourFix.stateName = actualState.name;
                    behaviourFix.stateHash = actualState.nameHash;
                }
            }

            foreach (var machine in stateMachineStack) {
                foreach (var behaviour in machine.behaviours) {
                    var behaviourFix = behaviour as StateMachineBehaviourFix;
                    if (behaviourFix) {
                        behaviourFix.parentMachine = machine.name;
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

    public void OnAfterDeserialize() {
        DeserializeHashToStateInfoDict();
    }

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        if (stateInfos == null) {
            Debug.LogWarning("missing information about the Animator on " + name + ", please update the prefab/object");
            return;
        }

        if (behaviours == null) {
            behaviours = InitBehaviours();
        }

        var currentNameHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        bool firstUpdate = lastNameHash == -1;
        if (firstUpdate) {
            lastNameHash = currentNameHash;
            lastStateInfo = stateInfos[0][lastNameHash];
        }

        bool changedState = currentNameHash != lastNameHash;
        if (firstUpdate || changedState) {
            HandleStateChanged(currentNameHash, firstUpdate);
        }
        else {
            behaviours[currentNameHash].StateUpdate(animator);
        }

    }

    private Dictionary<int, StateMachineBehaviourFix> InitBehaviours() {
        var behaviourDict = new Dictionary<int, StateMachineBehaviourFix>();
        var allBehaviours = animator.GetBehaviours<StateMachineBehaviourFix>();

        for (int i = 0; i < allBehaviours.Length; i++) {
            behaviourDict[allBehaviours[i].stateHash] = allBehaviours[i];
        }

        return behaviourDict;
    }

    private void HandleStateChanged(int currentNameHash, bool firstUpdate) {
        if (!stateInfos[0].ContainsKey(currentNameHash)) {
            Debug.LogWarning("missing information about the Animator on " + name + ", please update the prefab/object");
            return;
        }

        if (!firstUpdate) {
            behaviours[lastNameHash].StateExit(animator);
        }
        behaviours[currentNameHash].StateEnter(animator);

        StateInfo currentStateInfo = stateInfos[0][currentNameHash];

        //for (int i = 0; i < lastStateInfo.containingStateMachines.Count; i++) {
        //    var machine = lastStateInfo.containingStateMachines[i];
        //    if (!currentStateInfo.containingStateMachines.Contains(machine)) {
        //        for (int j = 0; j < behaviours.Length; j++) {
        //            var behaviour = behaviours[j];
        //            if (behaviour.parentMachine == machine) {
        //                try {
        //                    behaviour.StateMachineExit(animator);
        //                }
        //                catch {
        //                    Debug.LogWarning("error in state machine " + machine);
        //                    throw;
        //                }
        //            }
        //        }
        //    }
        //}

        lastNameHash = currentNameHash;
        lastStateInfo = currentStateInfo;
    }

    public bool IsInSubStateMachine(string name, int layerIndex) {
        return GetCurrentStateInfo(layerIndex).containingStateMachines.Contains(name);
    }

    public string GetCurrentStateName(int layerIndex) {
        return GetCurrentStateInfo(layerIndex).stateName;
    }

    public StateInfo GetCurrentStateInfo(int layerIndex) {
        return stateInfos[layerIndex][animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash];
    }

    [SerializeField] private int numLayers;
    [SerializeField] private HashStateInfoPair[] layer0Serialized;
    [SerializeField] private HashStateInfoPair[] layer1Serialized;
    [SerializeField] private HashStateInfoPair[] layer2Serialized;
    [SerializeField] private HashStateInfoPair[] layer3Serialized;

    private void SerializeHashToStateInfoDict() {
        numLayers = stateInfos.Length;
        for (int i = 0; i < stateInfos.Length; i++) {
            if (i > 3)
                break;
            HashStateInfoPair[] arr = null;

            if (i == 0) {
                layer0Serialized = new HashStateInfoPair[stateInfos[i].Count];
                arr = layer0Serialized;
            }

            if (i == 1) {
                layer1Serialized = new HashStateInfoPair[stateInfos[i].Count];
                arr = layer1Serialized;
            }

            if (i == 2) {
                layer2Serialized = new HashStateInfoPair[stateInfos[i].Count];
                arr = layer2Serialized;
            }

            if (i == 3) {
                layer3Serialized = new HashStateInfoPair[stateInfos[i].Count];
                arr = layer3Serialized;
            }

            int cntr = 0;
            foreach (var keyValuePair in stateInfos[i]) {
                arr[cntr++] = new HashStateInfoPair(keyValuePair.Key, keyValuePair.Value);
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
        stateInfos = new Dictionary<int, StateInfo>[numLayers];
        for (int i = 0; i < numLayers; i++) {
            stateInfos[i] = new Dictionary<int, StateInfo>();
            HashStateInfoPair[] arr = null;

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
                stateInfos[i][arr[j].intVal] = arr[j].stateInfo;
            }
        }
    }

}

/// <summary>
/// Contains all the information about a state
/// </summary>
[Serializable]
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
public class HashStateInfoPair {

    public int intVal;
    public StateInfo stateInfo;

    public HashStateInfoPair(int intVal, StateInfo stateInfo) {
        this.intVal = intVal;
        this.stateInfo = stateInfo;
    }

}
