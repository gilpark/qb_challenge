using System;
using System.Collections;
using System.Collections.Generic;
using S7.Net;
using UnityEngine;
using M1.Utilities;
using UniRx;
//using UnityEditor.MemoryProfiler;



public class GateManager : SingletonBehaviour<GateManager>
{
    private static Plc _plc;
    private bool evaluated2 = false;
    private bool evaluated3 = false;

    private bool[] lanesEvaluated = new bool[] {false, false, false};

    private static string Feet_real = "DB9.DBD62";
    private static string Incomplete_int = "DB9.DBW66";
    private static string Hike_int = "DB9.DBW60";
    private static string MPH1_Real = "DB9.DBD4";
    private static string MPH2_Real = "DB9.DBD16";
    private static string MPH3_Real = "DB9.DBD28";
    private static string MPH4_Real = "DB9.DBD40";
    private static string TTT1_real = "DB9.DBD0";
    private static string TTT2_real = "DB9.DBD12";
    private static string TTT3_real = "DB9.DBD24";
    private static string TTT4_real = "DB9.DBD36";
    private static string X1_int = "DB9.DBW8";
    private static string X2_int = "DB9.DBW20";
    private static string X3_int = "DB9.DBW32";
    private static string X4_int = "DB9.DBW44";
    private static string Y1_int = "DB9.DBW10";
    private static string Y2_int = "DB9.DBW22";
    private static string Y3_int = "DB9.DBW34";
    private static string Y4_int = "DB9.DBW46";
   
    double ttt1 {get { return ((uint)_plc.Read(TTT1_real)).ConvertToDouble();}} 
    double ttt2 {get { return ((uint)_plc.Read(TTT2_real)).ConvertToDouble();}} 
    double ttt3 {get { return ((uint)_plc.Read(TTT3_real)).ConvertToDouble();}} 

    double mph1 {get { return ((uint)_plc.Read(MPH1_Real)).ConvertToDouble();}} 
    double mph2 {get { return ((uint)_plc.Read(MPH2_Real)).ConvertToDouble();}} 
    double mph3 {get { return ((uint)_plc.Read(MPH3_Real)).ConvertToDouble();}} 

    short x1 {get { return ((ushort)_plc.Read(X1_int)).ConvertToShort();}} 
    short x2 {get { return ((ushort)_plc.Read(X2_int)).ConvertToShort();}} 
    short x3 {get { return ((ushort)_plc.Read(X3_int)).ConvertToShort();}} 
    short y1 {get { return ((ushort)_plc.Read(Y1_int)).ConvertToShort();}} 
    short y2 {get { return ((ushort)_plc.Read(Y2_int)).ConvertToShort();}} 
    short y3 {get { return ((ushort)_plc.Read(Y3_int)).ConvertToShort();}} 
    short done {get { return ((ushort)_plc.Read("DB9.DBW60")).ConvertToShort();}}

    ErrorCode write_Hike {get {return _plc.Write("DB9.DBW60", 1);}}

//    public  IObservable<PlcData> SensorDatObservable;
//    private  Subject<PlcData> _sensorSubject = new Subject<PlcData>();
    private bool _hike = false;
    public BoolReactiveProperty SendHike2Plc = new BoolReactiveProperty(false);
    int test = 0;

    private void Awake()
    {
      //  SensorDatObservable = _sensorSubject;
        SendHike2Plc.Subscribe(b =>
        {
            if (b)
            {
                Debug.Log("Hike!!");
                Debug.Log(_plc.Write("DB9.DBW60", 1));
                _hike = true;
                //evaluated2 = false;

                lanesEvaluated[0] = false;
                lanesEvaluated[1] = false;
                lanesEvaluated[2] = false;



                SendHike2Plc.Value = false;
            }
        }).AddTo(this.gameObject);
        Observable.Start(() =>
        {
           // var data = null;

            while (_hike)
            {
//                data.ttt1 = ttt1;
//                data.ttt2 = ttt2;
//                data.ttt3 = ttt3;
//                data.mph1 = mph1;
//                data.mph2 = mph2;
//                data.mph3 = mph3;
//                data.x1 = x1;
//                data.x2 = x2;
//                data.x3 = x3;
//                data.y1 = y1;
//                data.y2 = y2;
//                data.y3 = y3;
//
//
//                MainThreadDispatcher.Post(a => CheckIfHitAndHandle(0, data.mph1), null);
//                MainThreadDispatcher.Post(a => CheckIfHitAndHandle(1, data.mph2), null);
//                MainThreadDispatcher.Post(a => CheckIfHitAndHandle(2, data.mph3), null);


                if (done == 0)
                {
                    //Debug.Log("MPH______" + data.mph1);

                    Observable.Timer(TimeSpan.FromTicks(15)).Subscribe(_ =>
                    {
                   
//                         _hike = false;
//                        //Debug.Log("THE MPH " + data.mph2);
//                        data.L1_Completed = data.mph1 == 0 ? false : true;
//                        data.L2_Completed = data.mph2 == 0 ? false : true;
//                        data.L3_Completed = data.mph3 == 0 ? false : true;
//
//                        _sensorSubject.OnNext(data);
                    
                    });

                }


            }
        }).RepeatUntilDestroy(this.gameObject).Subscribe();
//
//        _sensorSubject.Subscribe(data =>
//        {
//  
//        });
//        
//        SensorDatObservable.Buffer(2).Subscribe(d =>
//        {
//            var data = d[1];
//            int _x2 = data.x2;
//            int _y2 = data.y2;
//
//            var __x2 = (_x2 !=0 )?_x2 / 4: 0;
//            var __y2 = (_y2 !=0 )?_y2 / 4: 0;
//
////            Debug.LogFormat("Lane2 : Complete {4} ! ttt-{0} speed-{1} X-Y : {2}-{3}", data.ttt1, data.mph1,__x1,
////                __y1, data.L2_Completed);
////            Debug.LogFormat("Lane2 : Complete {4} ! ttt-{0} speed-{1} X-Y : {2}-{3}", data.ttt2, data.mph2,__x2,
////                __y2, data.L2_Completed);
////            Debug.LogFormat("Lane3 : Complete {4} ! ttt-{0} speed-{1} X-Y : {2}-{3}", data.ttt3, data.mph3, data.x3,
////                data.y3, data.L3_Completed);
//            
//            
//            //Lane ONE
//            if (data.L1_Completed && !lanesEvaluated[0])
//            {
//                HandleResult(0, data.x1, data.y1, data.mph1);
//                lanesEvaluated[0] = true;
//
//            }
//            else if (data.ttt1 >= 3 && !lanesEvaluated[0])
//            {
//                OnPassIncomplete(0, data.mph1);
//                lanesEvaluated[0] = true;
//            }
//
//            //Lane TWO
//            if (data.L2_Completed && !lanesEvaluated[1])
//            {
//                HandleResult(1, data.x2, data.y2, data.mph2);
//                lanesEvaluated[1] = true;
//
//            }
//            else if (data.ttt2 >= 3 && !lanesEvaluated[1])
//            {
//                OnPassIncomplete(1, data.mph2);
//                lanesEvaluated[1] = true;
//            }
//
////            //Lane THREE
//            if (data.L3_Completed && !lanesEvaluated[2])
//            {
//                
//                Debug.Log("plot third point - x " + data.x3/4 + " y " + data.y3/4);
//                HandleResult(2, data.x3, data.y3, data.mph3);
//                lanesEvaluated[2] = true;
//
//            }
//            else if (data.ttt3 >= 3 && !lanesEvaluated[2])
//            {
//                OnPassIncomplete(2, data.mph3);
//                lanesEvaluated[2] = true;
//            }
//        });
        
        Open();
    }

    private void OnDisable()
    {
        Close();
    }

    public bool Open()
    {
        _plc = new Plc(cpu:CpuType.S71200, ip: "10.1.10.6", rack: 0, slot: 1);
        ErrorCode errCode = _plc.Open();
        Debug.Log("PLC connected -- "+errCode);
        return errCode == ErrorCode.NoError;
    }

    public void Close()
    {
        _plc.Close();
    }

    public bool Hike()
    {
        var b = write_Hike == ErrorCode.NoError;
       if (b) _hike = true;
        return b;
    }

    public void DoneReading()
    {
        _hike = false;
    }

    public void HandleResult(int lane, short x, short y, double mph)
    {
        if (HitTarget(lane, x, y))
        {
            //Complete
            OnPassComplete(lane, mph);
            //evaluated2 = true;
        }
        if (!HitTarget(lane, x, y))
        {
            OnPassIncomplete(lane, mph);
            //evaluated2 = true;

        }
    }

    public bool HitTarget(int lane, short x, short y)
    {
     //   return TargetManager.Instance.plotters[lane].PlotPointAndReturnResult(x, y);
        return true;
    }

    private int result_1, result_2, result_3;
    
    public void OnPassComplete(int lane, double speed)
    {
      //  if (!TeamManager.Instance.players[lane].active) return;

        // if(!Completed)return;
        Debug.Log("OnPassComplete - - - - ");
//        var rounds = InputModule.Instance._currentClientState;
//        var updateStatus1 = new AppStateParams();
//        //Debug.Log("LANE NUMBER IS - " + lane);
//        updateStatus1.UpdateResult(rounds, lane, true, speed);
//        MainThreadDispatcher.Post(_ => InputModule.Instance.UpdateAppStatus(updateStatus1), null);
//        
        //Debug.LogFormat("------000000000-----------{0} : {1}",rounds,speed );
        
        //result_1 = (rounds == ClientState.State.R1) ? (int) speed : 0;

//        if (rounds == ClientState.State.R1)
//        {
//            TeamManager.Instance.players[lane].r1 = (int) speed;
//        }
//        if (rounds == ClientState.State.R2)
//        {
//            TeamManager.Instance.players[lane].r2 = (int) speed;
//        }
//        if (rounds == ClientState.State.R3)
//        {
//            TeamManager.Instance.players[lane].r3 = (int) speed;
//        }
//        result_2 = (rounds == ClientState.State.R2) ? (int) speed : 0;
//        result_3 = (rounds == ClientState.State.R3) ? (int) speed : 0;

        float resultSpeed = (true) ? (float) speed : 0f;

        //Debug.Log("speeeeedd 0---" + speed);
        
        //UnityEngine.Debug.Log("is null?" + TeamManager.Instance);
        //TeamManager.Instance.players[lane].results.AddMPHResult((int)speed);
        //DoOnMainThread.ExecuteOnMainThread.
        //Enqueue(() => TeamManager.Instance.players[laneIdx].SetResultMPHText(resultSpeed.ToString()));

        //TeamManager.Instance.players[lane].SetResultMPHText(((float)speed).ToString("0.00"));//resultSpeed.ToString());


//        CommunicationModule.Instance.
//            SendRoundResults(TeamManager.Instance.players[lane].clientID,
//                (int)TeamManager.Instance.currentRound, lane,
//                TeamManager.Instance.players[lane].results,TeamManager.Instance.players[lane].r1,TeamManager.Instance.players[lane].r2, TeamManager.Instance.players[lane].r3);

        //if (TeamManager.Instance.players[0].active)
        //{
        //    CommunicationModule.Instance.
        //        SendRoundResults(TeamManager.Instance.players[0].clientID,
        //            (int) TeamManager.Instance.currentRound, 0,
        //            TeamManager.Instance.players[0].results, 0, 0, 0);
        //}

        //if (TeamManager.Instance.players[2].active)
        //{
        //    CommunicationModule.Instance.
        //        SendRoundResults(TeamManager.Instance.players[2].clientID,
        //            (int) TeamManager.Instance.currentRound, 2,
        //            TeamManager.Instance.players[2].results, 0, 0, 0);
        //}

       // TeamManager.Instance.players[lane].HitTarget();

    }

    public void OnPassIncomplete(int lane, double speed)
    {
      //  if (!TeamManager.Instance.players[lane].active) return;
        //if(Completed)return;
        
        Debug.Log("OnPassIncomplete - - - -");


//        var updateStatus1 = new AppStateParams();
//        var rounds = InputModule.Instance._currentClientState;
//
//        updateStatus1.UpdateResult(rounds, lane, false, 0);
//        MainThreadDispatcher.Post(_ => InputModule.Instance.UpdateAppStatus(updateStatus1), null);
//        //Debug.LogFormat("------XXXXXXXXX-----------{0} : {1}",InputModule.Instance._currentClientState,speed );
//        
//        if (rounds == ClientState.State.R1)
//        {
//            TeamManager.Instance.players[lane].r1 = 0;
//        }
//        if (rounds == ClientState.State.R2)
//        {
//            TeamManager.Instance.players[lane].r2 =  0;
//        }
//        if (rounds == ClientState.State.R3)
//        {
//            TeamManager.Instance.players[lane].r3 =  0;
//        }
        float resultSpeed = 0f;
        
        //UnityEngine.Debug.Log("is null?" + TeamManager.Instance);
        //TeamManager.Instance.players[lane].results.AddMPHResult((int)resultSpeed);
        //DoOnMainThread.ExecuteOnMainThread.
        //Enqueue(() => TeamManager.Instance.players[laneIdx].SetResultMPHText(resultSpeed.ToString()));

        //TeamManager.Instance.players[lane].SetResultMPHText(((int)resultSpeed).ToString("0.00"));//resultSpeed.ToString());

    

//        CommunicationModule.
//            SendRoundResults(TeamManager.Instance.players[lane].clientID,
//                (int)TeamManager.Instance.currentRound, lane,
//                TeamManager.Instance.players[lane].results,TeamManager.Instance.players[lane].r1,TeamManager.Instance.players[lane].r2, TeamManager.Instance.players[lane].r3);


        //if (TeamManager.Instance.players[0].active)
        //{
        //    CommunicationModule.Instance.
        //        SendRoundResults(TeamManager.Instance.players[0].clientID,
        //            (int) TeamManager.Instance.currentRound, 0,
        //            TeamManager.Instance.players[0].results, 0, 0, 0);
        //}

        //if (TeamManager.Instance.players[2].active)
        //{
        //    CommunicationModule.Instance.
        //        SendRoundResults(TeamManager.Instance.players[2].clientID,
        //            (int) TeamManager.Instance.currentRound, 2,
        //            TeamManager.Instance.players[2].results, 0, 0, 0);
        //}

      //  TeamManager.Instance.players[lane].MissedTarget();


    }

    public void CheckIfHitAndHandle(int lane, double speed)
    {
        //if (!TeamManager.Instance.players[lane].active) return;
        //Debug.Log("~~~~~~~CheckIfHitAndHandle() " + speed);
        if (speed != 0)
        {
            Debug.Log("~~~~~~~IT HIT HIT TIHAOIUSDHFOHASDF");
        //    TeamManager.Instance.players[lane].targetUI.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}

