using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class IdleRandomPlay : MonoBehaviour
{

	public Animator Animator;
	private StateMachineObservalbes _stateMachineObservables;

	public void Init()
	{
		//_animator = GetComponent <Animator> ();
		//_stateMachineObservables = Animator. GetBehaviour <StateMachineObservalbes> ();
		//Animator.SetInteger("random", Random.Range(0, 4));
		//_stateMachineObservables
		//	. OnStateEnterObservable
		//	. Select(_=> Random.Range(0,4))
		//	. Subscribe (i =>
		//	{
		//		Animator.SetInteger("random", i);
		//		//Debug.Log(i);
		//	});
		
	}
	
}
