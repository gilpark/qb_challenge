using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchState_AC : StateMachineObservalbes 
{
	private string ParentName = "";
	private TeamManager _teamManager;
		
	private void Awake()
	{
		_teamManager = TeamManager.Instance;
	}

	private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		ParentName = animator.transform.parent.name;
		_teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.CatchEnter;
		//Debug.LogFormat("[{0}]  Catch Enter ",ParentName);
	}

	private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		_teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.CatchExit;
		//Debug.LogFormat("[{0}]  Catch Exit ",ParentName);
	}
	
}
