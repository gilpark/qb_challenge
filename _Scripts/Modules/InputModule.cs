using System;
using System.Collections.Generic;
using System.Linq;
using M1.Utilities;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using UnityEngine;

public class InputModule : SingletonBehaviour<InputModule>
{
	public bool Verbose;
	public List<JoinButton> _joinButtons = new List<JoinButton>();
	public GameObject TestElementsParent;
	
	public RectTransform[] Plots,Targets;
	
	
	private CanvasGroup _testElements;
	private AppStateBroker _appStateBroker;
	
	public Subject<float[]> P1Data = new Subject<float[]>();
	public Subject<float[]> P2Data = new Subject<float[]>();
	public Subject<float[]> P3Data = new Subject<float[]>();
	public Subject<float[]> P4Data = new Subject<float[]>();
	
	private void Awake()
	{
		if(TargetManager.Instance.PlottingTestMode)return;
		
		_appStateBroker = AppStateBroker.Instance;
		_testElements = TestElementsParent.GetComponent<CanvasGroup>();
		var everyUpdate = Observable.EveryUpdate();
		
		//debug binding
		everyUpdate
			.Select(_ => GameManager.TargetObject_External)
			.DistinctUntilChanged()
			.Subscribe(_ => Verbose = (GameManager.TargetObject_External & TargetObject.InputModule) == TargetObject.InputModule)
			.AddTo(gameObject);
		
		//snapbutton binding
			everyUpdate
				.Where(_ => Input.GetKeyDown(KeyCode.Alpha0))
				.Select(_ => new Unit())
				.Subscribe(_ => _appStateBroker.SnapButonObservable.OnNext(_))
				.AddTo(gameObject);
		
		FadeManager.DisableCanvasGroup(_testElements,true);
	}

	private void Start()
	{
		if(TargetManager.Instance.PlottingTestMode)return;

		if(GameManager.ManualTeamSelectAllowedExternal)OnTestInputAllowed();
		
//		OnTimWondowsOpen(0).AddTo(gameObject);
//		OnTimWondowsOpen(1).AddTo(gameObject);
//		OnTimWondowsOpen(2).AddTo(gameObject);
//		if(GameManager.Instance.Open4thLane) OnTimWondowsOpen(3).AddTo(gameObject);
		
		_appStateBroker
			.CurrentRound
			.Where(_=>GameManager.ManualTeamSelectAllowedExternal)
			.Where(r=> r== Rounds.Idle)
			.Subscribe(_=>
				_joinButtons.ForEach(jb=>
				{
					jb.Slider.gameObject.SetActive(true);
					jb.Slider.value = -1;
				}))
			.AddTo(gameObject);
	}

	

	private void OnTestInputAllowed()
	{
		FadeManager.EnableCanvasGroup(_testElements,true);
		
		_joinButtons.ForEach(jb =>
		{
			jb.Button.gameObject.SetActive(false);
			jb.Slider
				.OnValueChangedAsObservable()
				.Subscribe(val =>
				{
					var b = jb.Slider.value != -1;
					jb.Text.text =((TeamType)jb.Slider.value).ToString();
					jb.Button.gameObject.SetActive(b);
					
				}).AddTo(jb.Slider.gameObject);
			
			jb.Button
				.OnClickAsObservable()
				.Where(_ => jb.Slider.value != -1)
				.Subscribe(_ =>
				{
					var msg = new ORTCPEventParams();
					msg.message = "{\"state\":10,\"station_id\":"+jb.Lane+",\"team\":"+jb.Slider.value+"}";
					_appStateBroker.ComReceivingStream.OnNext(msg);
					jb.Slider.gameObject.SetActive(false);
					jb.Button.gameObject.SetActive(false);
				})
				.AddTo(jb.Button.gameObject);
		});
	}

	

}
