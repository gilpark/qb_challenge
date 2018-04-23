using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlottingPointLinker : MonoBehaviour
{
	public GameObject Target;
	public Text DebugText;
	private Image _image;

	private void Awake()
	{
		_image = transform.GetComponent<Image>();
		var b = bool.Parse(Config.Read(CONFIG_KEYS.showplot));
		var color = Color.red;
		var color2 = Color.white;
		color.a =  b ? 1 : 0;
		color2.a = b ? 1 : 0;
		_image.color = color;
		DebugText.color = color2;
	}

	void Start ()
	{
		//todo filter for testing
		Observable
			.EveryUpdate()
			.Subscribe(_ =>
			{
				var t = (RectTransform) transform;
				var t2 = (RectTransform) Target.transform;
				t.position = t2.position;
				t.sizeDelta = t2.sizeDelta;
			})
			.AddTo(gameObject);
		
	}
}
