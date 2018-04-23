 using System;
 using UnityEngine;
 using System.Collections;
 using UniRx;

 public class StateMachineObservalbes : StateMachineBehaviour
 {
     #region OnStateEnter

     private Subject <AnimatorStateInfo> onStateEnterSubject = new Subject <AnimatorStateInfo> ();

     public IObservable <AnimatorStateInfo> OnStateEnterObservable {get {return onStateEnterSubject. AsObservable ();}}

     public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
         onStateEnterSubject. OnNext (stateInfo);
     }

     #endregion

     #region OnStateExit

     private Subject <AnimatorStateInfo> onStateExitSubject = new Subject <AnimatorStateInfo> ();

     public IObservable <AnimatorStateInfo> OnStateExitObservable {get {return onStateExitSubject. AsObservable ();}}

     public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
         onStateExitSubject. OnNext (stateInfo);
     }

     #endregion

     #region OnStateMachineEnter

     private Subject <int> onStateMachineEnterSubject = new Subject <int> ();

     public IObservable <int> OnStateMachineEnterObservable {get {return onStateMachineEnterSubject. AsObservable ();}}

     public override void OnStateMachineEnter (Animator animator, int stateMachinePathHash)
     {
         onStateMachineEnterSubject. OnNext (stateMachinePathHash);
     }

     #endregion

     #region OnStateMachineExit

     private Subject <int> onStateMachineExitrSubject = new Subject <int> ();

     public IObservable <int> OnStateMachineExitObservable {get {return onStateMachineExitrSubject. AsObservable ();}}

     public override void OnStateMachineExit (Animator animator, int stateMachinePathHash)
     {
         onStateMachineExitrSubject. OnNext (stateMachinePathHash);
     }

     #endregion

     #region OnStateMove

     private Subject <AnimatorStateInfo> onStateMoveSubject = new Subject <AnimatorStateInfo> ();

     public IObservable <AnimatorStateInfo> OnStateMoveObservable {get {return onStateMoveSubject. AsObservable ();}}

     public override void OnStateMove (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
         onStateMoveSubject. OnNext (stateInfo);
     }

     #endregion

     #region OnStateMove

     private Subject <AnimatorStateInfo> onStateUpdateSubject = new Subject <AnimatorStateInfo> ();

     public IObservable <AnimatorStateInfo> OnStateUpdateObservable {get {return onStateUpdateSubject. AsObservable ();}}

     public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
         onStateUpdateSubject. OnNext (stateInfo);
     }

     #endregion


     #region OnStateIK

     private Subject <AnimatorStateInfo> onStateIKSubject = new Subject <AnimatorStateInfo> ();

     public IObservable <AnimatorStateInfo> OnStateIKObservable {get {return onStateIKSubject. AsObservable ();}}

     public override void OnStateIK (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
         onStateIKSubject. OnNext (stateInfo);
     }

     #endregion
 }
 
