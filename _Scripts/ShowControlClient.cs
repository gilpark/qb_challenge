using System;
using M1.Utilities;
using SimpleJSON;
using UniRx;
using UnityEngine;

public class ShowControlClient : ORTCPClient 
{
    
    private IObservable<ORTCPEventParams> _tcpMessageRecieved;

    private void Start()
    {
        Connect();
        
        _tcpMessageRecieved =
            Observable
                .FromEvent<TCPServerMessageRecivedEvent, ORTCPEventParams>
                (
                    h => p => h(p), 
                    h => this.OnTCPMessageRecived += h,
                    h => this.OnTCPMessageRecived -= h
                );

        _tcpMessageRecieved.Subscribe(p =>
        {
            var msg = p.message;
             switch (msg)
            {
                case "reset-application":
                    GameManager.ResetApp.Value = true;
                    break;

				case "appstatus":
				    var str = "Game Status : ";
				    var json = JSON.Parse(GameManager.StatusMsgBackup);
				    var state = json["state"].AsInt;

				    if (state == 30) str += "Idle_State \n";
				    else str += "In_Game_State \n";
                    var sensorblock = PLCModule.Instance.SensorStatus()?"true" : "false";
				    str += "Sensor Satus \n";
				    str += "  Com-Test : " + PLCModule.Instance.PLCComStatus().ToString();
				    str += "\n" + "  Sensor-Blocked : " + sensorblock;
				    Send(str);
					break;

                case "test-on":
                    UIDebug.useUIDebug = true;
                    break;
                    
                case "test-off":
                    UIDebug.useUIDebug = false;

                    break;
            }
        }).AddTo(gameObject);
    }
}



