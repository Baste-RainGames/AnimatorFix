# AnimatorFix
A lean project that aims to fix a bunch of the many issues with Unity's Animator and StateMachineBehaviour systems.
The Animator is a really powerfull system, but it has some major disadvantages. First of all, the API for getting information about the current state of the Animator is horribly obtuse and cumbersome to use. Secondly, the StateMachineBehaviour's messages - especially OnStateMachineEnter/Exit - doesn't fire when you expect them to. Both of these disadvantages are by design, so this project aims to create the tools needed to use the Animator comfortably. 

I want the code to work with your existing Animator-based projects, so this isn't a replacement or a framework. The start of this project is code I wrote for World To The West, in order to work with Animators there. I have seen a lot of people on Unity Answers and the Unity forums that have the same gripes with this stuff as I do, so I figured that there would be interest both in using the code, and helping improve it.

There's two main scripts:
- StateMachineBehaviourFix is supposed to be a replacement for StateMachineBehaviour. It's got an OnStateMachineExit method that's called whenever a state machine is exited. The default Unity StateMachineBehaviour generally doesn't do that.
- AnimatorFix is neccessary to make StateMachineBehaviourFix work. It's attached to the same object as the Animator, and works by parsing the Animator's AnimatorController to find out what methods should be called when. It's also got some convenience methods that the Animator should have had - like GetCurrentStateName(int layer) and IsInSubStateMachine(string name, int layer)
 
This is a very work in progress project. The code from the inital commit is exactly what I needed to work with Animators in World To The West. There's a couple of things that are problematic, and some things that I think this needs to be a worthy stand-alone project:

Bugs: 
- The AnimatorFix script requires you to select the object it's attached to once after you update the corresponding AnimatorController. That's a big inconvenicene

Problems:
- AnimatorFix isn't a really good name. This project isn't about fixing bugs, but about a fundamental disagreement with Unity about the Animator/StateMachineBehaviour API. Name suggestions are welcome!

To be considered:
- Right now, the system only replaces OnStateMachineExit. This is the most broken method. The system could easily be expanded to handle all of the state machine messages. That would probably be advantageous; OnStateEnter and OnStateExit are not neccessarily called when you expect them to. 
