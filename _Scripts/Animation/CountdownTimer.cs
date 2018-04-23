using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour 
{
	public float time = 3f;
	public Text countdownText;

	public delegate void CountDownCompleteHandler();
	public event CountDownCompleteHandler OnCountDownComplete;

	private bool active = false;

	public void OnEnable()
	{
		Reset();
		countdownText.text = "3.00";


		transform.GetChild(0).gameObject.SetActive(true);

//		if (InputModule.Instance._currentClientState == ClientState.State.Error)
//		{
//			
//			//Debug.Log("Dindnt call");
//			return;
//		}
		
		//Debug.Log("Hikehikehike");
		//InputModule.Instance.HikePLC();
	}

	private void Update () 
	{
		if (active)
		{
			time -= Time.deltaTime;
			countdownText.text = time.ToString ("0.00");
		}

		if (time <= 0 && active) 
		{
			countdownText.text = "0.00";
			active = false;

			//Debug.Log("DONE");
			OnCountDownComplete();
			return;
		}
	}

	public void StartTimer()
	{

		active = true;
		time = 3f;
	}

	public void Reset()
	{
		active = false;
		time = 3f;
	}

	public void StopTimer()
	{
		active = false;
	}
}
