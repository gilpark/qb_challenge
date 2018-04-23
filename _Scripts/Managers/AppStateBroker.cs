using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using M1.Utilities;
using SimpleJSON;
using UniRx;
using UniRx.Diagnostics;

public class AppStateBroker : SingletonBehaviour<AppStateBroker>
{
    public bool Verbose;
    public RoundsReactiveProperty CurrentRound = new RoundsReactiveProperty();
    public ReactiveCollection<QBClient> ClientsList = new ReactiveCollection<QBClient>();
    
    //plc data streams
    public Subject<UniRx.Tuple<float[],bool>> P1Result = new Subject<UniRx.Tuple<float[],bool>>();
    public Subject<UniRx.Tuple<float[],bool>> P2Result = new Subject<UniRx.Tuple<float[],bool>>();
    public Subject<UniRx.Tuple<float[],bool>> P3Result = new Subject<UniRx.Tuple<float[],bool>>();
    public Subject<UniRx.Tuple<float[],bool>> P4Result = new Subject<UniRx.Tuple<float[],bool>>();
    
    public BoolReactiveProperty P1TimeWindow = new BoolReactiveProperty();
    public BoolReactiveProperty P2TimeWindow = new BoolReactiveProperty();
    public BoolReactiveProperty P3TimeWindow = new BoolReactiveProperty();
    public BoolReactiveProperty P4TimeWindow = new BoolReactiveProperty();

    //from input module
    public Subject<Unit> SnapButonObservable = new Subject<Unit>();
    
    //from Com & PLC module
    public Subject<ORTCPEventParams> ComReceivingStream = new Subject<ORTCPEventParams>();
    public Subject<string> ComOutGoingStream = new Subject<string>(); 
    public Subject<Unit> HikeStream = new Subject<Unit>();
    
    //Triggers to jump btw states
    public Animator StateMachineAnimator;
    private readonly Action _moveToR1 = () => Instance.StateMachineAnimator.SetTrigger("MoveToR1");
    private readonly Action _moveToR2 = () => Instance.StateMachineAnimator.SetTrigger("MoveToR2"); 
    private readonly Action _moveToR3 = () => Instance.StateMachineAnimator.SetTrigger("MoveToR3"); 
    private readonly Action _moveToIdle = () => Instance.StateMachineAnimator.SetTrigger("MoveToIdle");
    private readonly Action _moveToFinish = () => Instance.CurrentRound.Value = Rounds.Finish;

    private TeamManager _teamManager;
    
    private void Awake()
    {
        
        //debug binding
        Observable.EveryUpdate()
            .Select(_ => GameManager.TargetObject_External)
            .DistinctUntilChanged()
            .Subscribe(_ => Verbose = (GameManager.TargetObject_External & TargetObject.AppStateBroker) == TargetObject.AppStateBroker)
            .AddTo(gameObject);
        
        _teamManager = TeamManager.Instance;
        AddingClient().AddTo(gameObject);
        OnTimeWindowOpen().AddTo(gameObject);
        
        SnapToStart()
            .Concat(HikeAndCheckRounds(Rounds.R1, Rounds.R1_Hike, _moveToR2))
            .Concat(HikeAndCheckRounds(Rounds.R2, Rounds.R2_Hike, _moveToR3))
            .Concat(HikeAndCheckRounds(Rounds.R3, Rounds.R3_Hike,_moveToFinish))
            .Concat(SnapToFinish())
            .Subscribe().AddTo(gameObject);
    }

    private IDisposable OnTimeWindowOpen()
    {
        P1TimeWindow.Where(open=> open).Subscribe(b =>
        {
            var client = ClientsList.First(x => x.Lane == 0);
            SetScores(client,client.Lane).AddTo(this);
        }).AddTo(this);
        P2TimeWindow.Where(open=> open).Subscribe(b =>
        {
            var client = ClientsList.First(x => x.Lane == 1);
            SetScores(client,client.Lane).AddTo(this);
        }).AddTo(this);
        P3TimeWindow.Where(open=> open).Subscribe(b =>
        {
            var client = ClientsList.First(x => x.Lane == 2);
            SetScores(client,client.Lane).AddTo(this);
        }).AddTo(this);
      return   
         P4TimeWindow.Where(open=> open).Subscribe(b =>
        {
            var client = ClientsList.First(x => x.Lane == 3);
            SetScores(client,client.Lane).AddTo(this);
        }).AddTo(this);
    }

    public void CloseTimeWindow(int lane)
    {
        switch (lane)
        {
            case 0: P1TimeWindow.Value = false; break;
            case 1: P2TimeWindow.Value = false; break;
            case 2: P3TimeWindow.Value = false; break;
            case 3: P4TimeWindow.Value = false; break;
        }
    }

    private IDisposable SetScores(QBClient client, int lane)
    {
        var playerresult = lane == 0 ? P1Result :
                           lane == 1 ? P2Result : 
                           lane == 2 ? P3Result : P4Result; 
        var timeWindow = lane == 0 ? P1TimeWindow :
                         lane == 1 ? P2TimeWindow : 
                         lane == 2 ? P3TimeWindow : P4TimeWindow;
        return 
        CurrentRound      
            .Take(1)
            .Subscribe(round =>
            {
                playerresult.Take(1)
                    .Subscribe(result =>
                    {
                        switch (round)
                        {
                            case Rounds.R1_Hike:
                                client.SetR1Score(result.Item2,result.Item1[0]);
                                if(timeWindow.Value)timeWindow.Value = false;
                                break;
                            case Rounds.R2_Hike:
                                client.SetR2Score(result.Item2,result.Item1[0]);
                                if(timeWindow.Value)timeWindow.Value = false;

                                break;
                            case Rounds.R3_Hike:
                                client.SetR3Score(result.Item2,result.Item1[0]);
                                if(timeWindow.Value)timeWindow.Value = false;
                                break;
                        }
                    }).AddTo(this);
            })
            .AddTo(this);	
    }

    
    private IDisposable AddingClient()
    {
        return
            ComReceivingStream
                .Subscribe(e =>
                {
                    var json = JSON.Parse(e.message);
                    var state = json["state"].AsInt;
                    var lane = json["station_id"].AsInt;
                    var team = json["team"].AsInt;

                    var isIntheList = false;
                    if (state != 10) return;
                    foreach (var qbClient in ClientsList)
                    {
                        isIntheList = qbClient.Lane == lane;
                    }

                    if (isIntheList)
                    {
                        Debug.Log("Player already joined");
                    }
                    else
                    {
                        var playerDataStrings = _teamManager.GetPlayerData(team);
                        var x = new QBClient(e.clientID, lane, team, playerDataStrings[0], playerDataStrings[1]);
                    
                            ClientsList.Add(x);
                            if (Verbose)
                                Debug.LogFormat("[{0}] Client Added(+) \n id - {1} : lane -{2} : team - {3}",
                                    name, x.ClientId, x.Lane, (TeamType) x.Team);
                    }
 
                });
    }

    private UniRx.IObservable<Rounds> SnapToStart()
    {
     var snap2idlehike = 
            CurrentRound
                .Where(x => x == Rounds.Idle)
                .Take(1)
                .Do(_ =>
                {
                    Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                        .TakeUntil(ClientsList.ObserveCountChanged().Where(x => x > 0))
                        .DoOnCompleted(() =>
                        {
                            SnapButonObservable
                                .Take(1)
                                .DoOnCompleted(() =>
                                {
                                    CurrentRound.Value = Rounds.Idle_Hike;
                                    ComOutGoingStream.OnNext("{\"state\":20,\"description\":\"in_game\"}");
                                })
                                .Subscribe()
                                .AddTo(this);
                        })
                        .Subscribe()
                        .AddTo(this);
                }).Debug(Verbose,"[{name}]_snap2idle");
        
        var snap2r1 =
            CurrentRound
                .Where(x => x == Rounds.Idle_Hike)
                .Take(1)
                .Do(_ =>
                {
                    Observable.Timer(TimeSpan.FromSeconds(2))
                        .DoOnCompleted(_moveToR1)
                        .Subscribe()
                        .AddTo(this);
                }).Debug(Verbose,"[{name}]_snap2r1");

        return snap2idlehike.Concat(snap2r1);
    }

    private UniRx.IObservable<Rounds> SnapToFinish()
    {
        return 
        CurrentRound
            .Where(x => x == Rounds.Finish)
            .Take(1)
            .Debug(Verbose,"[{name}]_snap2finish")
            .Do(_ =>
            {
                SnapButonObservable
                    .Take(1)
                    .DoOnCompleted(() =>
                    {
                        _moveToIdle();
                        ClientsList.Clear();
                        ComOutGoingStream.OnNext("{state:30,description:\"game_complete or reset\"}");
                           
                        SnapToStart()
                            .Concat(HikeAndCheckRounds(Rounds.R1, Rounds.R1_Hike, _moveToR2))
                            .Concat(HikeAndCheckRounds(Rounds.R2, Rounds.R2_Hike, _moveToR3))
                            .Concat(HikeAndCheckRounds(Rounds.R3, Rounds.R3_Hike, _moveToFinish))
                            .Concat(SnapToFinish())
                            .Subscribe().AddTo(gameObject);                
                        
                    })
                    .Subscribe()
                    .AddTo(this);
            });
    }

    private UniRx.IObservable<Rounds> HikeAndCheckRounds(Rounds initial, Rounds tocheck,  params Action[] toInvoke )
    {
        var initialState = initial; 
        var checkState = tocheck;
        var roundIdx = initial == Rounds.R1 ? 1 : initial== Rounds.R2 ? 2 : 3;
        
        var hike = CurrentRound.Where(x => x == initialState)
                    .Do(_ => DelayedSanp(1.5f, () => CurrentRound.Value = checkState)).Debug(Verbose,"{name}_"+initial+"_Hike");
        var check = CheckRoundsDone(checkState, roundIdx, () => toInvoke.ToList().ForEach(a => a.Invoke()))
                    .Debug(Verbose,"{name}_"+initial+"_Check");
        return hike.Take(1).Concat(check);
    }

    private void DelayedSanp(float duration, params Action[] rounds)
    {
        Observable.Timer(TimeSpan.FromSeconds(duration))
            .Select(x=>new Unit())
            .Concat(SnapButonObservable)
            .Skip(1)
            .Take(1)
            .DoOnCompleted(() => rounds.ToList().ForEach(x=>x.Invoke()))
            .Subscribe(_=>HikeStream.OnNext(new Unit()))
            .AddTo(gameObject);
    }

    private UniRx.IObservable<Rounds> CheckRoundsDone(Rounds checkState, int round,params Action[] toInvoke)
    {
        var toDispose = new Subject<Unit>();
        
            CurrentRound.Where(x => x == checkState)
            .Take(1)
            .Subscribe(_ =>
            {
                Observable
                    .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                    .TakeWhile(condition => ClientsList.All(x =>
                    {
                        var b = false;
                        switch (round)
                        {
                            case 1: b = x.R1Done; break;
                            case 2: b = x.R2Done; break;
                            case 3: b = x.R3Done; break;
                        }
                        if (Verbose)
                            Debug.LogFormat("[{0}] Client status Round{4} :\n lane = {1} team ={2} Result = {3}"
                                , name, x.Lane, x.Team, x.Result1, round);
                        return b;
                    }) != true)
                    .DoOnCompleted(() =>
                    {
//                        ClientsList.ToList().ForEach(x =>
//                        {
//                            switch (round)
//                            {
//                                case 1:
//                                    ComOutGoingStream.OnNext(CommunicationModule.BuildRound1Msg(x.Lane, x.Result1, x.Results[0].Hit));
//                                    break;
//                                case 2:
//                                    ComOutGoingStream.OnNext(
//                                        CommunicationModule.BuildRound2Msg(x.Lane, x.Result1,x.Results[0].Hit, x.Result2,x.Results[1].Hit));
//                                    break;
//                                case 3:
//                                    ComOutGoingStream
//                                        .OnNext(CommunicationModule
//                                            .BuildRound3Msg(x.Lane, x.Result1,x.Results[0].Hit, x.Result2,x.Results[1].Hit,x.Result3,x.Results[2].Hit,x.PlayerName,x.PlayerPercentage));
//                                    break;
//                            }
//                        });
                        toDispose.OnCompleted();  

                        toInvoke.ToList().ForEach(x =>x.Invoke());
                    }).Subscribe();
            });
        return toDispose.Select(_=>checkState);
    }
}
