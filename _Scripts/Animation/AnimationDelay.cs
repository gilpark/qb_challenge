using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDelay : MonoBehaviour 
{
	public Animator anim;
	public float delay = 3f;
	public GameObject football;
	public Transform rightHand;

	private Vector3 snapPos;
	private Vector3 snapRot;

	private IEnumerator Start() 
	{
		anim.enabled = false;
		yield return new WaitForSeconds (delay);
		anim.enabled = true;
	}

	public void SnapTo()
	{
		//football.transform.localPosition = snapPos;
		//football.transform.localRotation = Quaternion.Euler (snapRot);
		StartCoroutine(iSnapTo());


	}

	private IEnumerator iSnapTo()
	{
		anim.enabled = false;
		football.transform.SetParent (rightHand);

		yield return null;
		football.transform.localPosition = new Vector3 (-.00055f, .0012f, .00135f);
		football.transform.localRotation = Quaternion.Euler(28f, -11f, 76f);

	}
}
