using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlottingPresenter : MonoBehaviour
{

	public bool Verbose;
	public int Lane;
	public RectTransform Panel;
	public RectTransform PlottingPoint;
	public Slider scaleX, scaleY;
	public Slider posX, posY;
	public Slider RadiusSlider;

	public GameObject ControlBox;
	public Toggle ConnerToggle;
	public GameObject Conners;
	public Image RadiusImage;
	public Button SaveButton;
	public Button HikeButton;
	
	public Text PosXText, 
			    PosYText,
		        ScaleXText,
		        ScaleYText,
				RadiusText;

	public InputField PosXInput,
					  PosYInput,
					  ScaleXInput,
					  ScaleYInput;
	
	[SerializeField]
	private LaneConfig Config = new LaneConfig();
	
	private CanvasGroup _uipanel;
	private TargetManager _targetManager;
	private InputModule _inputModule;
	
	private void Awake()
	{
		_inputModule = InputModule.Instance;
		
		if (!File.Exists(Application.persistentDataPath + "/Lane" + Lane + ".config"))
		{
			Config.PosX = Panel.anchoredPosition.x;
			Config.PosY = Panel.anchoredPosition.y;
			Config.ScaleX = Panel.localScale.x;
			Config.ScaleY = Panel.localScale.y;
			Config.Radius = 300f;
			Save();
		}

		Config = Load();
		posX.minValue = Panel.anchoredPosition.x - 500;
		posX.maxValue = Panel.anchoredPosition.x + 500;
		posY.minValue = Panel.anchoredPosition.y - 500;
		posY.maxValue = Panel.anchoredPosition.y + 500;
		
		
		RadiusSlider.maxValue = 500;
		RadiusSlider.value = Config.Radius;
		//if(PlotCollider)PlotCollider.MaxDistance = Config.Radius;
		//PlottingPoint.sizeDelta = new Vector2(Config.Radius,Config.Radius);
		posX.value = Config.PosX;
		posY.value = Config.PosY;
		scaleX.value = Config.ScaleX;
		scaleY.value = Config.ScaleY;

		_uipanel = Panel.GetComponent<CanvasGroup>();
		_targetManager = TargetManager.Instance;
		
		Panel.anchoredPosition = new Vector2(Config.PosX, Config.PosY);
		Panel.localScale = new Vector3(Config.ScaleX, Config.ScaleY);
		//PlottingPoint.sizeDelta = new Vector2(Config.Radius,Config.Radius);
		DisableUi();	
		
		BindPlot2PLC(Lane).AddTo(gameObject);
		if(_targetManager.PlottingTestMode)Init1();
	}

	private void Init1()
	{
		//control box 
		var toggle = false;
		Observable
			.EveryUpdate()
			.Select(_=> _targetManager.PlottingTestMode&&_targetManager.LaneConfigSwich.Value == Lane&&Input.GetKeyUp(KeyCode.C))
			.DistinctUntilChanged()
			.Subscribe(x=>
			{
				if (x) toggle = !toggle;
				ControlBox.SetActive(toggle);
				ControlBox.transform.position = Input.	mousePosition;
				_uipanel.alpha = toggle ? 1 : 0;
			})
			.AddTo(gameObject);
		
		Observable
			.EveryUpdate()
			.Select(_ => _targetManager.PlottingTestMode && _targetManager.LaneConfigSwich.Value != Lane)
			.DistinctUntilChanged()
			.Subscribe(x => DisableUi())
			.AddTo(gameObject);
		
		ConnerToggle
			.OnValueChangedAsObservable()
			.Subscribe(x => Conners.SetActive(x))
			.AddTo(gameObject);
		
		//binding values
		Observable
			.EveryUpdate()
			.Subscribe(_ =>
			{
				PosXText.text = Panel.anchoredPosition.x.ToString();
				PosYText.text = Panel.anchoredPosition.y.ToString();
				ScaleXText.text = Panel.localScale.x.ToString();
				ScaleYText.text = Panel.localScale.y.ToString();
				RadiusText.text = RadiusImage.rectTransform.sizeDelta.x.ToString();
			})
			.AddTo(gameObject);

		scaleX.OnValueChangedAsObservable()
			.Subscribe(x=>
			{
				Panel.localScale = new Vector2(x, Panel.localScale.y);
				Config.ScaleX = x;
			})
			.AddTo(gameObject);
		
		ScaleXInput.OnEndEditAsObservable().Subscribe(val =>
		{
			float fval = scaleX.value;
			float.TryParse(val, out fval);
			scaleX.value = fval;
		}).AddTo(gameObject);
		
		scaleY.OnValueChangedAsObservable()
			.Subscribe(x=>
			{
				Panel.localScale = new Vector2(Panel.localScale.x, x);
				Config.ScaleY = x;
			})
			.AddTo(gameObject);
		
		ScaleYInput.OnEndEditAsObservable().Subscribe(val =>
		{		
			float fval = scaleY.value;
			float.TryParse(val, out fval);
			scaleY.value = fval;
		}).AddTo(gameObject);
		
		posX.OnValueChangedAsObservable()
			.Subscribe(x=>
			{
				Panel.anchoredPosition = new Vector2(x, Panel.anchoredPosition.y);
				Config.PosX = x;
			})
			.AddTo(gameObject);
		
		PosXInput.OnEndEditAsObservable().Subscribe(val =>
		{
			float fval = posX.value;
			float.TryParse(val, out fval);
			posX.value = fval;
		}).AddTo(gameObject);
		
		posY.OnValueChangedAsObservable()
			.Subscribe(x=>
			{
				Panel.anchoredPosition = new Vector2(Panel.anchoredPosition.x, x);
				Config.PosY = x;
			})
			.AddTo(gameObject);

		PosYInput.OnEndEditAsObservable().Subscribe(val =>
		{
			float fval = posY.value;
			float.TryParse(val, out fval);
			posY.value = fval;
		}).AddTo(gameObject);
		
		RadiusSlider.OnValueChangedAsObservable()
			.Subscribe(x =>
			{
				RadiusImage.rectTransform.sizeDelta = new Vector2(x, x);
				Config.Radius = x;
				//PlottingPoint.sizeDelta = new Vector2(x,x);
			})
			.AddTo(gameObject);
		
		//save button
		SaveButton
			.OnClickAsObservable()
			.Subscribe(_ => Save())
			.AddTo(gameObject);
		
		//hike button
		HikeButton
			.OnClickAsObservable()
			.Subscribe(_ =>
			{
				PlottingPoint.anchoredPosition = Vector2.zero;
				PLCModule.Instance.TestHike();
				HikeButton.interactable = false;
				Observable.Timer(TimeSpan.FromSeconds(10f)).Take(1).Subscribe(a => HikeButton.interactable = true);
			})
			.AddTo(gameObject);
	}

	private IDisposable BindPlot2PLC(int lane)
	{
		switch (lane)
		{
			case 0 :
				return 
					_inputModule
					.P1Data
					.Subscribe(x =>
						{
					
						MainThreadDispatcher.Post(_ =>
						{
							var pos = new Vector2(GameManager.ManualInputAllowed_External?x[1]:x[1].FromTo(0, 960, 960, 0), x[2]);
							 Debug.Log("Lane 1 " + pos);
							PlottingPoint.anchoredPosition = pos;
						}, null);
					});
			case 1 :
				return 
					_inputModule
					.P2Data
					.Subscribe(x =>
					{
					
						MainThreadDispatcher.Post(_ =>
						{
							var pos = new Vector2(GameManager.ManualInputAllowed_External?x[1]:x[1].FromTo(0, 960, 960, 0), x[2]);
							if (Verbose) Debug.Log("Lane 2 " + pos);
							PlottingPoint.anchoredPosition = pos;
						},null);
					});
			case 2 :
				return
					_inputModule
						.P3Data
						.Subscribe(x =>
						{
							
							MainThreadDispatcher.Post(_ =>
							{
								var pos = new Vector2(GameManager.ManualInputAllowed_External?x[1]:x[1].FromTo(0, 960, 960, 0), x[2]);
							
								if (Verbose) Debug.Log("Lane 3 " + pos);
								PlottingPoint.anchoredPosition = pos;
							}, null);
						});
			default :
				return
					_inputModule
						.P4Data
						.Subscribe(x =>
						{
							
							MainThreadDispatcher.Post(_ =>
							{
								var pos = new Vector2(GameManager.ManualInputAllowed_External?x[1]:x[1].FromTo(0, 960, 960, 0), x[2]);
								if (Verbose) Debug.Log("Lane 4 " + pos);
								PlottingPoint.anchoredPosition =pos;
							}, null);
						});
		}	
	}

	private void DisableUi()
	{
		Conners.SetActive(false);
		ControlBox.SetActive(false);
		_uipanel.alpha = 0;
	}

	
	private LaneConfig Load()
	{
		if(File.Exists(Application.persistentDataPath + "/Lane"+Lane+".config")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/Lane"+Lane+".config", FileMode.Open);
			var config = (LaneConfig)bf.Deserialize(file);
			file.Close();
			if (Verbose)
				Debug.LogFormat("[{0}] data loaded \nfrom : {1}", name,
					Application.persistentDataPath + "/Lane" + Lane + ".config");
			return config;
		}
		return null;
	}
	
	private void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/Lane"+Lane+".config");
		bf.Serialize(file, Config);
		file.Close();
		if (Verbose)
			Debug.LogFormat("[{0}] data Saved \nfrom : {1}", name,
				Application.persistentDataPath + "/Lane" + Lane + ".config");
		DisableUi();
	}   
	
}
