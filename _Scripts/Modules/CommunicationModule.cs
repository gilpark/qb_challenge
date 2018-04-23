using System;
using System.Runtime.InteropServices;
using M1.Utilities;
using SimpleJSON;
using UniRx;
using UnityEngine;
using UnityEngine.PostProcessing;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(ORTCPMultiServer))]

public class CommunicationModule : MonoBehaviour
{
    private IObservable<ORTCPEventParams> _tcpMessageRecieved;
    private AppStateBroker _appStateBroker;
    private ORTCPMultiServer _server;

    void Start()
     {
         _appStateBroker = AppStateBroker.Instance;
         _server = ORTCPMultiServer.Instance;
         
          _tcpMessageRecieved =   
          Observable
              .FromEvent<ORTCPMultiServer.TCPServerMessageRecivedEvent, ORTCPEventParams>
              (
                   h => p => h(p), 
                   h => ORTCPMultiServer.Instance.OnTCPMessageRecived += h,
                   h => ORTCPMultiServer.Instance.OnTCPMessageRecived -= h
               );
          
          _tcpMessageRecieved
             .Subscribe(OnMessageRecived)
              .AddTo(gameObject);
         
        _appStateBroker
            .ComOutGoingStream
            .Subscribe(msg =>
            {
                Debug.Log(msg);
                _server.SendAllClientsMessage(msg);
                GameManager.StatusMsgBackup = msg;
            })
            .AddTo(gameObject);
        
     }

     private void OnMessageRecived(ORTCPEventParams e)
     {
         _appStateBroker.ComReceivingStream.OnNext(e);
     }

    public void BroadcastStartGame()
    {
        var msg = "{\"state\":20,\"description\":\"in_game\"}";
        
        ORTCPMultiServer.Instance.SendAllClientsMessage(msg);
    }

    public void BroadcastEndGame()
    {
        var msg = "{\"state\":30,\"description\":\"game_complete\"}";
        ORTCPMultiServer.Instance.SendAllClientsMessage(msg);
    }

    public static string BuildRound1Msg(int lane, string speed, bool complete1)
    {
        var msg = "{state:22" + ",station_id:" + lane + ",description:\"round1_results\",pass_speed:[[" + speed +",\""+ complete1.ToString().ToLower()+"\"]]}";
        return msg;
    }

    public static string BuildRound2Msg(int lane, string speed1 , bool complete1, string speed2, bool complete2)
    {
        var msg ="{state:25" +",station_id:" +lane +",description:\"round2_results\",pass_speed:[[" +speed1 + ",\""+complete1.ToString().ToLower()+"\"]"+",["+speed2 +",\""+complete2.ToString().ToLower()+ "\"]]}";
        return msg;
    }
    public static string BuildRound3Msg(int lane, string speed1, bool complete1,  string speed2, bool complete2 ,  string speed3, bool complete3 ,string playername, string percentage)
    {
        var msg = "{state:27" +",station_id:" +lane +",description:\"round3_results\",pass_speed:[[" +speed1 + ",\""+complete1.ToString().ToLower()+"\"]"+",["+speed2 +",\""+complete2.ToString().ToLower()+ "\"],["+speed3+",\""+complete3.ToString().ToLower()+"\"]]" +
                  ",team_average_msg:\"" +
                  playername +": " +percentage +"%\"}";
        return msg;
    }
}
