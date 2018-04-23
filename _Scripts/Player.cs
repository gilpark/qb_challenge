using System;
using System.Linq;
using UniRx;
using UniRx.Diagnostics;
using Unity.Linq;
using UnityEngine;
using UnityStandardAssets.Utility;


public class Player : MonoBehaviour
{
    public bool Verbose;
    public Animator PlayerAnimator;
    public SmoothFollow CameraFollower;
    public Ref Ref;
    public GameObject FootBallPrefab;
    
    //foot ball setting
    private readonly Vector3 _ballPos = new Vector3(0.000088f,0.001375f,0.001165f);
    private readonly Quaternion _ballRot = new Quaternion(0.25569530f,-0.23448170f,0.69124320f,0.63389360f);
    private const float FootballScale = 0.011f;
    private GameObject _footBall;
    
    //player start position to reset every rounds
    private Vector3 _startPosition;
    private Quaternion _startRot;
    
    //animation related
    private RuntimeAnimatorController _playController;
    private GameObject _actor;
    
    //animation triggers
    private void SetToRun()
    {
        PlayerAnimator.SetTrigger("Run");
    }
    private void SetToHit()
    {
        PlayerAnimator.SetTrigger("Hit");
        var l = _lane;
        switch (_roundStream.Value)
        {
            case Rounds.R1_Hike:
            case Rounds.R2_Hike:
                AudioManager.Instance
                    .AudioSequenceObservable.OnNext(new UniRx.Tuple<int, SFXType>(l,SFXType.CrowdComplete));                
                break;
            case Rounds.R3_Hike:
                AudioManager.Instance
                    .AudioSequenceObservable.OnNext(new UniRx.Tuple<int, SFXType>(l,SFXType.CompleteFinal));                
                break;
        }
    }
    private void SetToMiss()
    {
        PlayerAnimator.SetTrigger("Miss");
        var l = _lane;
        AudioManager.Instance
            .AudioSequenceObservable.OnNext(new UniRx.Tuple<int, SFXType>(l,SFXType.CrowdIncomplete));
    }
    private void SetToCallRef()
    {
        PlayerAnimator.SetTrigger("RefCalled");
    }

    private void SetToMoveToIdle()
    {
        PlayerAnimator.SetTrigger("MoveToIdle");
    }
    
    //references     
    private AppStateBroker _appStateBroker;
    private RoundsReactiveProperty _roundStream;
    private int _lane;
    private QBClient thisPlayer {get{return  _appStateBroker.ClientsList.First(x => x.Lane == _lane); }}

    private void Awake()
    {
        _appStateBroker = AppStateBroker.Instance;
        //debug binding
        Observable.EveryUpdate()
            .Select(_ => GameManager.TargetObject_External)
            .DistinctUntilChanged()
            .Subscribe(_ => Verbose = (GameManager.TargetObject_External & TargetObject.Plyaer) == TargetObject.Plyaer)
            .AddTo(gameObject);
    }

    public void Initialize
        ( int lane, GameObject actor, 
        RuntimeAnimatorController idle, 
        RuntimeAnimatorController play, 
        float height,
        PlayerSquenceReactiveProperty squence)
    {
            _lane = lane;
            actor.transform.localScale = Vector3.one;
            actor.transform.localPosition = Vector3.zero;
            CameraFollower.target = actor.transform;
            PlayerAnimator = actor.GetComponent<Animator>();
            _startPosition = transform.GetChild(0).localPosition;
            _startRot = transform.GetChild(0).localRotation;
            
            actor.transform.parent.localScale =
                new Vector3(actor.transform.parent.localScale.x, height, actor.transform.parent.localScale.z);
            PlayerAnimator.runtimeAnimatorController = idle;
            _playController = play;
            _actor = actor;
            SubscribeSequence(squence);
            
            QBPlay.Instance
                .PlayerPositionNotifier
                .Do(x =>
                {
                    switch (_appStateBroker.CurrentRound.Value)
                    {
                        case Rounds.R2:
                            SetToMoveToIdle();
                            break;
                        case Rounds.R3:
                            SetToMoveToIdle();
                            break;
                    }
                })
                .Subscribe(_=>GoBackToStartPos())
                .AddTo(_actor);

            _footBall = AddFootBall(actor);
            actor.SetActive(true);
           // _footBall = FindBall(actor);
            _footBall.SetActive(false);
        
        //Current App Round Stream
        _roundStream =_appStateBroker.CurrentRound;
        _roundStream
            .DistinctUntilChanged()
            .Subscribe(round =>
            {
                switch (round)
                {
                    case Rounds.Idle_Hike:
                        PlayerAnimator.runtimeAnimatorController = _playController;
                        break;
                    case Rounds.R1_Hike:
                    case Rounds.R2_Hike:
                    case Rounds.R3_Hike:
                        SetToRun();
                        break;
                    case Rounds.Finish:
                        _appStateBroker.SnapButonObservable
                            .Take(1)
                            .DoOnCompleted(() =>
                            {
                                squence.Dispose();
                                GoBackToStartPos();
                                Destroy(_actor);
                            })
                            .Subscribe()
                            .AddTo(_actor);
                        break;
                }
            })
            .AddTo(_actor);
    }

    private void SubscribeSequence(PlayerSquenceReactiveProperty squence)
    {
        var tlane = _lane == 0
            ? TargetLane.Lane1
            : _lane == 1
                ? TargetLane.Lane2
                : _lane == 2
                    ? TargetLane.Lane3
                    : TargetLane.Lane4;
        
        squence.Debug(Verbose&& (GameManager.TargetLane_External&tlane) == tlane,"Player-{_lane}")
            .Subscribe(state =>
            {
                switch (state)
                {
                    case AnimSequence.OnReadyEnter:
                        break;
                    case AnimSequence.OnReadyExit:
                        break;
                    case AnimSequence.CatchEnter:
                        Observable 
                            .Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
                            .TakeWhile( condition =>
                            {
                                var b = false;
                                switch (_roundStream.Value)
                                {
                                    case Rounds.R1_Hike:    
                                        b = thisPlayer.R1Throw;
                                        break;
                                    case Rounds.R2_Hike:
                                        b = thisPlayer.R2Throw;
                                        break;
                                    case Rounds.R3_Hike:
                                        b = thisPlayer.R3Throw;
                                        break;
                                }
                                return b != true;
                            })
                            .DoOnCompleted(() =>
                            {
                                switch (_roundStream.Value)
                                {
                                    case Rounds.R1_Hike:
                                        if(thisPlayer.Results[0].Hit)SetToHit();
                                        else SetToMiss(); 
                                        break;
                                    case Rounds.R2_Hike:
                                        if(thisPlayer.Results[1].Hit)SetToHit();
                                        else SetToMiss();
                                        break;
                                    case Rounds.R3_Hike:
                                        if(thisPlayer.Results[2].Hit)SetToHit();
                                        else SetToMiss();
                                        break;
                                }
                            })
                            .Subscribe()
                            .AddTo(_actor);
                        break;
                    case AnimSequence.CatchExit:
                        break;
                    case AnimSequence.HitEnter:
                        _footBall.SetActive(true);
                        Observable
                            .Timer(TimeSpan.FromSeconds(1f))
                            .DoOnCompleted(() =>
                            {
                                switch (_roundStream.Value)
                                {
                                    case Rounds.R1_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[0].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream.OnNext(CommunicationModule.BuildRound1Msg(thisPlayer.Lane, thisPlayer.Result1, thisPlayer.Results[0].Hit));
                                        break;
                                    case Rounds.R2_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[1].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream.OnNext(
                                        CommunicationModule.BuildRound2Msg(thisPlayer.Lane, thisPlayer.Result1,thisPlayer.Results[0].Hit, thisPlayer.Result2,thisPlayer.Results[1].Hit));
                                        break;
                                    case Rounds.R3_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[2].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream
                                        .OnNext(CommunicationModule
                                            .BuildRound3Msg(thisPlayer.Lane, thisPlayer.Result1,thisPlayer.Results[0].Hit, thisPlayer.Result2,thisPlayer.Results[1].Hit,thisPlayer.Result3,thisPlayer.Results[2].Hit,thisPlayer.PlayerName,thisPlayer.PlayerPercentage));
                                        break;
                                }
                                Ref.RefAniStream
                                    .Take(1)
                                    .DoOnCompleted(SetToCallRef)
                                    .Subscribe()
                                    .AddTo(_actor);
                            })
                            .Subscribe()
                            .AddTo(_actor);
                        break;
                    case AnimSequence.HitExit:
                        break;
                    case AnimSequence.MissEnter:
                        Observable
                            .Timer(TimeSpan.FromSeconds(0.1f))
                            .DoOnCompleted(() =>
                            {
                                switch (_roundStream.Value)
                                {
                                    case Rounds.R1_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[0].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream.OnNext(CommunicationModule.BuildRound1Msg(thisPlayer.Lane, thisPlayer.Result1, thisPlayer.Results[0].Hit));
                                        break;
                                    case Rounds.R2_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[1].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream.OnNext(
                                            CommunicationModule.BuildRound2Msg(thisPlayer.Lane, thisPlayer.Result1,thisPlayer.Results[0].Hit, thisPlayer.Result2,thisPlayer.Results[1].Hit));
                                        break;
                                    case Rounds.R3_Hike:
                                        Ref.MakeCallBasedOnResult(thisPlayer.Results[2].Hit,_lane);
                                        _appStateBroker.ComOutGoingStream
                                            .OnNext(CommunicationModule
                                                .BuildRound3Msg(thisPlayer.Lane, thisPlayer.Result1,thisPlayer.Results[0].Hit, thisPlayer.Result2,thisPlayer.Results[1].Hit,thisPlayer.Result3,thisPlayer.Results[2].Hit,thisPlayer.PlayerName,thisPlayer.PlayerPercentage));
                                        break;
                                }
                                Ref.RefAniStream
                                    .Take(1)
                                    .DoOnCompleted(SetToCallRef)
                                    .Subscribe(_=>_footBall.SetActive(false))
                                    .AddTo(_actor);
                            })
                            .Subscribe()
                            .AddTo(_actor);
                        break;
                    case AnimSequence.MissExit:
                        break;
                    case AnimSequence.FinishEnter:
                        _roundStream
                            .Take(1)
                            .Subscribe(round =>
                            {
                                switch (round)
                                {
                                    case Rounds.R1_Hike:
                                        thisPlayer.R1Done = true;
                                        break;
                                    case Rounds.R2_Hike:
                                        thisPlayer.R2Done = true;
                                        break;
                                    case Rounds.R3_Hike:
                                        thisPlayer.R3Done = true;
                                        break;
                                }
                            })
                            .AddTo(_actor);
                        break;
                    case AnimSequence.FinishExit:
                        _footBall.SetActive(false);
                        break;
                }
            })
            .AddTo(_actor);
    }

    private void GoBackToStartPos()
    {
        _footBall.SetActive(false);
        _actor.transform.localPosition = _startPosition;
        _actor.transform.localRotation = _startRot;
    }
    
    private GameObject AddFootBall(GameObject actor)
    {
       // var ball = Instantiate(FootBallPrefab);
        var ball =
        actor
            .Descendants()
            .First(x => x.name == "mixamorig:LeftHand")
            .Add(FootBallPrefab);
        ball.transform.localScale = new Vector3(FootballScale,FootballScale,FootballScale);
        ball.transform.localPosition = _ballPos;
        ball.transform.localRotation = _ballRot;
        return ball;
    }

    private GameObject FindBall(GameObject actor)
    {
        var ball =
            actor
                .Descendants()
                .First(x => x.name == "Football(Clone)(Clone)");
        return ball;
    }
}
