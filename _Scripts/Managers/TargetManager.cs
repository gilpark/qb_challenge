using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using UniRx;
using UnityEngine;

public class TargetManager : SingletonBehaviour<TargetManager>
{
	private AppStateBroker _appStateBroker;
	public IntReactiveProperty LaneConfigSwich = new IntReactiveProperty();
	public bool PlottingTestMode;
	private void Awake()
	{
		_appStateBroker = AppStateBroker.Instance;

		var len = GameManager.Instance.Open4thLane ? 3 : 2;
		Observable
			.EveryUpdate()
			.Where(_ => PlottingTestMode && Input.GetKeyDown(KeyCode.Space))
			.Subscribe(x =>
			{
				LaneConfigSwich.Value++;
				if (LaneConfigSwich.Value > len) LaneConfigSwich.Value = 0;
			})
			.AddTo(gameObject);
	}
}

