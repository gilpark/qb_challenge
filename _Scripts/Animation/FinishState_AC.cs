using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishState_AC: StateMachineObservalbes
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
        _teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.FinishEnter;
        //Debug.LogFormat("[{0}]  Finsh Enter ",ParentName);
    }

    private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.FinishExit;
        //Debug.LogFormat("[{0}]  Finish Exit ",ParentName);
    }
}
