using System;
using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using S7.Net;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;

public class PLCModule : SingletonBehaviour<PLCModule>
{

    public bool Verbose;
    public float TTTDuration = 10f;
    public double DistanceFeet = 8.29f;
    public string PLCAddress = "172.30.201.232";
    private AppStateBroker _appStateBroker;
    private InputModule _inputModule;
    private Plc plc;

    private void Awake()
    {
        
        _appStateBroker = AppStateBroker.Instance;
        _appStateBroker.HikeStream.Subscribe(_=>Hike()).AddTo(gameObject);
        _inputModule = InputModule.Instance;
        
        var _PLCAddres = GameManager.DevMode_External? PLCAddress : Config.Read(CONFIG_KEYS.plcip).ToString();
        plc = new Plc(cpu: CpuType.S71200, ip: _PLCAddres, rack: 0, slot: 1);
        ErrorCode errCode = plc.Open();
        if (Verbose)Debug.LogFormat("[{0}] initial status [{2}] => {1}",name,errCode, _PLCAddres);
        
        var _DistanceFeet = GameManager.DevMode_External? DistanceFeet : float.Parse(Config.Read(CONFIG_KEYS.distance));
        ErrorCode feet = PLCdata.SetDistance(plc, _DistanceFeet);
        if (Verbose)Debug.LogFormat("[{0}] Setting feet to {2} => {1} feet",name,feet,_DistanceFeet);

        ErrorCode duration = PLCdata.SetTTT(plc, TTTDuration);
        if (Verbose)Debug.LogFormat("[{0}] Setting TTT to {2} => {1} secconds",name,duration,TTTDuration);
        plc.Close();
        
        Debug.Log("////////////////////////////////////");
        Debug.LogFormat("[{0}] initial status [{2}] => {1}",name,errCode, _PLCAddres);
        Debug.Log("////////////////////////////////////");
        //debug binding
        Observable.EveryUpdate()
            .Select(_ => GameManager.TargetObject_External)
            .DistinctUntilChanged()
            .Subscribe(_ => Verbose = (GameManager.TargetObject_External & TargetObject.PLCModule) == TargetObject.PLCModule)
            .AddTo(gameObject);
    }

    public ErrorCode PLCComStatus()
    {
        var code = plc.Open();
        plc.Close();
        return code;
    }

    public bool SensorStatus()
    {
        var code = plc.Open();
        var blocked = (bool)plc.Read("M7.0");
        plc.Close();
        return blocked;
    }
    
    private UniRx.IObservable<double> HikeStream()
    {
        return Observable.Create<double>(obs =>
        {
            if(Verbose)
            MainThreadDispatcher.Post(_ =>
            {
                Debug.LogFormat("[{0}] Start reading data.", name);
            }, null);
            
            if (plc.Open() == ErrorCode.NoError)
            {
                if (PLCdata.SetHike(plc) == ErrorCode.NoError)
                {
                    var ttt = 0;
                    while (ttt < TTTDuration)
                    {
                        ttt = (int) ((uint) plc.Read("DB9.DBD48")).ConvertToDouble();
                        obs.OnNext(ttt);
                    }
                    obs.OnCompleted();
                }
                else
                {
                    obs.OnError(new Exception("Unable to send msg to plc"));
                }
            }
            else
            {
                obs.OnError(new Exception("Unable to send msg to plc"));
            }
            return Disposable.Create(() =>
            {
                plc.Close();
                if (Verbose)
                    MainThreadDispatcher.Post(_ => Debug.LogFormat("[{0}] Hike Stream disposed.", name), null);
            });
        }).SubscribeOn(Scheduler.ThreadPool);
    }

    /// <summary>
    /// when HikeStream gets subscribed
    /// each stream reads plc data till it has x pos or timed out
    /// </summary>

    public void TestHike()
    {
        Hike();
    }

    private void Hike()
    {
        var hikeStream = HikeStream().Publish().RefCount();
        hikeStream
            .TakeWhile(condition =>PLCdata.P1.GetData(plc)[1] == 0)
            .Debug(Verbose && (GameManager.TargetLane_External&TargetLane.Lane1)==TargetLane.Lane1,"PLC-READ-L1")
            .DoOnCompleted(()=>
            {
                _inputModule.P1Data.OnNext(PLCdata.P1.GetData(plc));
            })
            .Subscribe().AddTo(gameObject);
        
        
        hikeStream
            .TakeWhile(condition =>PLCdata.P2.GetData(plc)[1] == 0)
            .Debug(Verbose&& (GameManager.TargetLane_External&TargetLane.Lane2)==TargetLane.Lane2,"PLC-READ-L2")
            .DoOnCompleted(()=>
            {
                _inputModule.P2Data.OnNext(PLCdata.P2.GetData(plc));
            })
            .Subscribe().AddTo(gameObject);
        
        hikeStream
            .TakeWhile(condition =>PLCdata.P3.GetData(plc)[1] == 0)
            .Debug(Verbose&& (GameManager.TargetLane_External&TargetLane.Lane3)==TargetLane.Lane3,"PLC-READ-L3")
            .DoOnCompleted(()=>
            {
                _inputModule.P3Data.OnNext(PLCdata.P3.GetData(plc));
            })
            .Subscribe().AddTo(gameObject);
        
        hikeStream
            .TakeWhile(condition =>PLCdata.P4.GetData(plc)[1] == 0)
            .Debug(Verbose&& (GameManager.TargetLane_External&TargetLane.Lane4)==TargetLane.Lane4,"PLC-READ-L4")
            .DoOnCompleted(()=>
            {
                _inputModule.P4Data.OnNext(PLCdata.P4.GetData(plc));
            })
            .Subscribe().AddTo(gameObject);
    }

}
