using System;
using System.Linq;
using M1.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlotCollider : MonoBehaviour
{
	public bool Verbose;
	public int Lane = 0;
	public RectTransform Target;
	public RectTransform Plot;
	public float MaxDistance;
	public Text PlcOutText;
	
	private AppStateBroker _appStateBroker;
	private InputModule _inputModule;
	private IObservable<Vector3> _mOSstream;
	private RectTransform _plot_Internal;
	
	private void Awake()
	{
		 MaxDistance = float.Parse(Config.Read(CONFIG_KEYS.targetthreshold));
		_appStateBroker = AppStateBroker.Instance;
		_inputModule = InputModule.Instance;
		_plot_Internal = this.GetComponent<RectTransform>();

	}

	void Start ()
	{
		_mOSstream = Observable.EveryUpdate().Select(_ => Input.mousePosition);
		OnTimWondowsOpen(Lane).AddTo(gameObject);
	}
	
	private IDisposable OnTimWondowsOpen(int lane)
	{
		
		var timeWindow = lane == 0 ? _appStateBroker.P1TimeWindow
					   : lane == 1 ? _appStateBroker.P2TimeWindow
					   : lane == 2 ? _appStateBroker.P3TimeWindow : _appStateBroker.P4TimeWindow;

		var result =  lane == 0 ? _appStateBroker.P1Result
					: lane == 1 ? _appStateBroker.P2Result
					: lane == 2 ? _appStateBroker.P3Result : _appStateBroker.P4Result;
		
		var PlayerData = lane == 0 ? _inputModule.P1Data
					   : lane == 1 ?  _inputModule.P2Data
					   : lane == 2 ?  _inputModule.P3Data :  _inputModule.P4Data;
		var offsetval = GameManager.Instance.Open4thLane ? 960 : 800;

	
		return 
		timeWindow.Where(open=> open)
			.Subscribe(b =>
			{
				var chasedPos = Vector2.zero;
				var chasedResult = new float[3]; 
				_plot_Internal.anchoredPosition = Vector2.zero;
				PlcOutText.text = "";
				
				if (GameManager.AutoTest_External)
				{
					if (GameManager.TestCount % 2 == 0)
					{
						var testval = AutoTestIndexProvider.GetTestTargetVal();
						PlayerData.OnNext(new []{GameManager.TestCount,testval.x,testval.y});	
					}
				}
				
				_mOSstream
					.Where(_=>GameManager.ManualInputAllowed_External )
					.TakeWhile(_ => !Input.GetMouseButtonDown(0) )
					.Do(mPos=>  chasedPos  = mPos)
					.DoOnCompleted(() =>
					{
						var offsetTotal = offsetval * Lane;
						chasedPos.x =  chasedPos.x - offsetTotal; //x
						PlayerData.OnNext(new[]{83,chasedPos.x,chasedPos.y});
					})
					.Subscribe()
					.AddTo(this);
				
				
				PlayerData
					.Take(1)
					.TakeWhile(_ => timeWindow.Value)
					.DoOnCompleted(() =>
					{
						MainThreadDispatcher.Post(a =>
						{
							if (chasedResult[1] != 0)
							{
								_plot_Internal.anchoredPosition = 
									new Vector2(GameManager.ManualInputAllowed_External?chasedResult[1]:chasedResult[1].FromTo(0, 960, 960, 0), chasedResult[2]);
								var dist = CheckDistance(Plot, Target) ;
								var isHit = dist < MaxDistance;
								var tosend = chasedResult;
								tosend[1] = GameManager.ManualInputAllowed_External ? chasedResult[1] : chasedResult[1].FromTo(0, 960, 960, 0);

								PlcOutText.text = "P: {tosend[1]},{tosend[2]} \n D: {dist}";
								result.OnNext(new UniRx.Tuple<float[], bool>(tosend,isHit));
							}
							else
							{

								_plot_Internal.anchoredPosition = 
									new Vector2(GameManager.ManualInputAllowed_External?chasedResult[1]:chasedResult[1].FromTo(0, 960, 960, 0), chasedResult[2]);
								var dist = CheckDistance(Plot, Target) ;
								var isHit = dist < MaxDistance;
								var tosend = chasedResult;
								tosend[0] = 0;
								tosend[1] = 0;
								PlcOutText.text = "P: {tosend[1]},{tosend[2]} \n D: {dist}";
								result.OnNext(new UniRx.Tuple<float[], bool>(tosend,isHit));
							}
						},null);
					})
					.Subscribe(x =>
					{
						chasedResult = x;		
					}).AddTo(this);
			});
	}
	
	private float CheckDistance(RectTransform rectTrans1, RectTransform rectTrans2)
	{	
		if(Verbose)Debug.LogFormat("[Lane {2}] Disance : {0}, Max : {1}",Vector2.Distance(rectTrans1.localPosition,rectTrans2.localPosition), MaxDistance, Lane);
		return Vector2.Distance(rectTrans1.localPosition,rectTrans2.localPosition);
	}
}
