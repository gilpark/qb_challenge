using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscTest : MonoBehaviour
{

	public string Address, Msg;

	public void Test()
	{
		var message = new SharpOSC.OscMessage(Address,Msg);
		var sender = new SharpOSC.UDPSender("127.0.0.1", 53001);
		sender.Send(message);
	}


}
