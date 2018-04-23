using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class IdleState : StateMachineObservalbes
{
    private QBIdle _QbIdle;
    private AppStateBroker _appBroker;
    
    private void Awake()
    {
        _appBroker = AppStateBroker.Instance;
        _QbIdle = QBIdle.Instance;
    }

    private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
       _appBroker.CurrentRound.Value = Rounds.Idle;
        if(GameManager.Instance.DebugStateMachine)Debug.LogFormat("[{0}] OnEnter - {1}", "Idle", Time.time);
        _QbIdle.OnEnter();
    }

    private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if(GameManager.Instance.DebugStateMachine)Debug.LogFormat("[{0}]  onExit- {1}", "Idle", Time.time);
        _QbIdle.OnExit();
    }
}
