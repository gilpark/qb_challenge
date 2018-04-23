using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		
		 for (int i = 0;i < 20; i++) {
                     if(Input.GetKeyDown("joystick 1 button "+i)){
                         print("joystick 1 button "+i);
                     }
                 }

		if (Input.GetKeyDown(KeyCode.Alpha0)) Debug.Log(0);
		if (Input.GetMouseButton(1)) Debug.Log(1);
		if (Input.GetMouseButton(2)) Debug.Log(2);
	}
}
