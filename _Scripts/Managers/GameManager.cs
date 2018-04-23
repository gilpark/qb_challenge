using System;
using System.Diagnostics;
using UnityEngine;
using M1.Utilities;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class GameManager : SingletonBehaviour<GameManager>
{
    public bool Open4thLane;
    public static int TestCount = 1;
    public GameObject DebugOutputObjects;
    public GameObject ThrowAllowed;
    public Text DubugOutput;
    [Header("OFFSET")]
    public float ResultDisplayDuration;
        
    [Header("TEST && DEBUG")]
    public bool ManualTeamSelectAllowed;
    public bool ManualInputAllowed;
    public bool DebugStateMachine;
    public bool AutoTest;
    public bool DevMode;
    
    [BitMaskAttribute(typeof(TargetLane))] 
    public TargetLane TargetLane;
    [BitMaskAttribute(typeof(TargetObject))] 
    public TargetObject TargetObject;
    private AppStateBroker _appStateBroker;
    
    public static Subject<Unit> TestSnapButton = new Subject<Unit>();

    public static string StatusMsgBackup = "{state:30}";

    private void Awake()
    {
        _appStateBroker = AppStateBroker.Instance;
        AutoTest = AutoTest_External;

#if !UNITY_EDITOR
        DevMode = false;
#endif
        //Debug msg binding
        Observable
            .EveryUpdate()
            .Select(_ => TargetLane)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                TargetLane_External = _;
            }).AddTo(gameObject);
        
        Observable
            .EveryUpdate()
            .Select(_ => TargetObject)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                TargetObject_External = _;
            }).AddTo(gameObject);
           
     //to reset
        ResetApp
            .Where(b=>b)
            .Take(1)
            .Subscribe(x =>
            {
                ResetApp.Value = false;
                AppStateBroker.Instance.ComOutGoingStream.OnNext("{state:30}");
                Observable.Timer(TimeSpan.FromSeconds(1f))
                    .Subscribe(_=>SceneManager.LoadScene(SceneManager.GetActiveScene().name))
                    .AddTo(gameObject);
            })
            .AddTo(gameObject);
    //to clibration
        CalibrationMode
            .Where(b=>b)
            .Take(1)
            .Subscribe(x =>
            {
                CalibrationMode.Value = false;
                AppStateBroker.Instance.ComOutGoingStream.OnNext("{state:30}");
                Observable.Timer(TimeSpan.FromSeconds(1f))
                    .Subscribe(_=>SceneManager.LoadScene(Open4thLane?"MainScene_4Lane_Cal":"MainScene_3Lane_Cal"))
                    .AddTo(gameObject);
            })
            .AddTo(gameObject);
        //to main game
        GameMode
            .Where(b=>b)
            .Take(1)
            .Subscribe(x =>
            {
                GameMode.Value = false;
                AppStateBroker.Instance.ComOutGoingStream.OnNext("{state:30}");
                Observable.Timer(TimeSpan.FromSeconds(1f))
                    .Subscribe(_=>
                    {
                     
                        SceneManager.LoadScene(Open4thLane ? "MainScene_4Lane" : "MainScene_3Lane");
                    })
                    .AddTo(gameObject);
            })
            .AddTo(gameObject);
        //auto test
        if (AutoTest)
        {
            DebugOutputObjects.SetActive(true);
            ManualTeamSelectAllowed = ManualTeamSelectAllowedExternal = false;
            Observable.EveryUpdate().Select(_ => TestCount).Subscribe(x =>
            {
                DubugOutput.text = x.ToString();
                ThrowAllowed.gameObject.SetActive(TestCount%2 != 0);
            }).AddTo(gameObject);
            
            _appStateBroker
                .CurrentRound
                .Subscribe(round =>
                {
                    switch (round)
                    {
                        case Rounds.Idle:
                            Observable.Timer(TimeSpan.FromSeconds(2f))
                                .Take(1)
                                .Subscribe(_ =>
                                {
                                    var len = Open4thLane ? 4 : 3;
                                    for (int i = 0; i < len; i++)
                                    {
                                        var client = new ORTCPEventParams();
                                        var team = AutoTestIndexProvider.GetTeamIndex();
                                        client.message = "{\"state\":10,\"station_id\":"+i+",\"team\":"+team+"}";
                                        _appStateBroker.ComReceivingStream.OnNext(client);    
                                    }
                          
                                    DelayedSanp();
                                }).AddTo(gameObject);
                            break;
                        case Rounds.R1:
                        case Rounds.R2:
                        case Rounds.R3:
                            DelayedSanp();
                            break;
                        case Rounds.Finish:
                            DelayedSanp();
                            TestCount++;
                            Debug.LogFormat("[{0}] Testing Count : {1}",name,TestCount);
                            break;
                    }
                })
                .AddTo(gameObject);
        }
        else
        {
            DebugOutputObjects.SetActive(false);
        }
       
    }



    private void DelayedSanp()
    {
        Observable.Timer(TimeSpan.FromSeconds(3f))
            .Subscribe(_ => _appStateBroker.SnapButonObservable.OnNext(new Unit())).AddTo(gameObject);
    }
    
    public static float ResultDisplayDuration_External
    {
        get { return Instance.ResultDisplayDuration; }
        set { Instance.ResultDisplayDuration = value; }
    }

    public static bool ManualTeamSelectAllowedExternal
    {
        get
        {
            if (!DevMode_External)
            {
                return bool.Parse(Config.Read(CONFIG_KEYS.manualteamselect));
            }
            else
            {
                return Instance.ManualTeamSelectAllowed;

            }
        }
        set { Instance.ManualTeamSelectAllowed = value; }
    }
    
    public static bool ManualInputAllowed_External
    {
        get
        {
            if (!DevMode_External)
            {
                return bool.Parse(Config.Read(CONFIG_KEYS.manualinput));
            }
            else
            {
                return Instance.ManualInputAllowed;
            }
        }
        set { Instance.ManualInputAllowed = value; }
    }
    
    public static bool AutoTest_External
    {
        get
        {
            if (!DevMode_External)
            {
               return bool.Parse(Config.Read(CONFIG_KEYS.autotest));
            }
            else
            {
                return Instance.AutoTest;
            }
        }
        set { Instance.AutoTest = value; }
    }
    
    public static bool DevMode_External
    {
        get { return Instance.DevMode; }
        set { Instance.DevMode = value; }
    }
    
    public static BoolReactiveProperty ResetApp = new BoolReactiveProperty(false);
    public static BoolReactiveProperty CalibrationMode = new BoolReactiveProperty(false);
    public static BoolReactiveProperty GameMode = new BoolReactiveProperty(false);

    public static TargetLane TargetLane_External;
    public static TargetObject TargetObject_External;

    
}


