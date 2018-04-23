using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using Ping = UnityEngine.Ping;

public class PingTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		using (System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping())
		{
			
			string input = "10.1.10.44";

			IPAddress address;
			if (IPAddress.TryParse(input, out address))
			{
				switch (address.AddressFamily)
				{
					case System.Net.Sockets.AddressFamily.InterNetwork:
						Debug.Log("ipv4");
						break;
					case System.Net.Sockets.AddressFamily.InterNetworkV6:
						Debug.Log("ipv6");
						break;
					default:
						// umm... yeah... I'm going to need to take your red packet and...
						break;
				}
				
				PingReply result;
				try
				{
					result = ping.Send(address);
				}
				catch (PingException)
				{
					result = null;
				}
				Debug.Log(result.Status);
				
			}
			
			
		
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
