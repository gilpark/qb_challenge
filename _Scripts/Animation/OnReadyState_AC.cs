using System.Collections;
using System.Collections.Generic;
using UnityEngine;



	public class OnReadyState_AC : StateMachineObservalbes
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
			_teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.OnReadyEnter;
		}

		private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
		{
			_teamManager.PlayerSequences[PlayerSequenceIndx.GetIndex(ParentName)].Value= AnimSequence.OnReadyExit;
			var target = _teamManager.TargetPresenters[PlayerSequenceIndx.GetIndex(ParentName)].Target;
			var catchval = AnimIndexProvider.GetCatchBlendVal();
			animator.SetFloat("CatchBlend", catchval);
			target.anchoredPosition = AnimIndexProvider.GetTargetPos(catchval);
			
			var hitval = AnimIndexProvider.GetHitBlendVal();
			animator.SetFloat("HitBlend", hitval);
			var missval = AnimIndexProvider.GetCatchBlendVal();
			animator.SetFloat("MissBlend", missval);
		}
	}

