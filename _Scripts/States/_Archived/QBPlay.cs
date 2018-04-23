using System;
using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using UniRx;
using UnityEngine;

public class QBPlay : SingletonBehaviour<QBPlay>
{
	public CanvasGroup UiPanel, BlackFade;
	public Subject<Unit> PlayerPositionNotifier = new Subject<Unit>();
	private void Awake()
	{
		FadeManager.DisableCanvasGroup(UiPanel,true);
	}

	#region Rounds
	public void OnR1Enter()
	{
		FadeManager.FadeIn(UiPanel, 0.2f,()=>PlayerPositionNotifier.OnNext(new Unit()));
	}

	public void OnR1Exit(){}
	
	public void OnR2Enter()
	{
		FadeManager.FadeIn(BlackFade, 0.3f, ()=>
		{
			PlayerPositionNotifier.OnNext(new Unit());
			FadeManager.FadeOut(BlackFade, 0.3f);
		});
	}

	public void OnR2Exit(){}
	
	public void OnR3Enter()
	{
		FadeManager.FadeIn(BlackFade, 0.3f,()=>
		{
			PlayerPositionNotifier.OnNext(new Unit());
			FadeManager.FadeOut(BlackFade, 0.3f);
		});
	}

	public void OnR3Exit()
	{
		FadeManager.FadeIn(BlackFade, 0.3f,()=>
		{
			PlayerPositionNotifier.OnNext(new Unit());
			FadeManager.FadeOut(BlackFade, 0.3f, () => FadeManager.FadeOut(UiPanel, 0.2f));
		});
	}
	#endregion

	
}
