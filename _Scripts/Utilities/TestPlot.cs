using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlot : MonoBehaviour
{
	public Transform dot, target;

	//void Update ()
	//{
	//    Debug.Log(Vector2.Distance(dot.position, target.position));
	//}

	public bool PlotPointAndReturnResult(int x, int y)
	{
		dot.GetComponent<RectTransform>().anchoredPosition = new Vector3((x/4) * 0.845f, (y/4), 0f);

		float distance = Vector2.Distance(dot.position, target.position);

		return (distance < 300) ? true : false;
	}

	public void OnEnable()
	{
		dot.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);

	}
}
