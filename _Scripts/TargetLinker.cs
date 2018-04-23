using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TargetLinker : MonoBehaviour {

	public GameObject Target;
	private Image _image;

	private void Awake()
	{
		_image = transform.GetComponent<Image>();
	
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
			})
			.AddTo(gameObject);
		
	}
}
