using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R3State :StateMachineObservalbes 
{
	private AppStateBroker _appBroker;
	private QBPlay _qbPlay;
	private void Awake()
	{
		_appBroker = AppStateBroker.Instance;
		_qbPlay = QBPlay.Instance;
	}

	private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		_appBroker.CurrentRound.Value = Rounds.R3;
		if(GameManager.Instance.DebugStateMachine)Debug.LogFormat("[{0}] OnEnter - {1}", "Round 3", Time.time);
		_qbPlay.OnR3Enter();
	}

	private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if(GameManager.Instance.DebugStateMachine)Debug.LogFormat("[{0}]  onExit- {1}", "Round 3",Time.time);
		_qbPlay.OnR3Exit();
	}
}
