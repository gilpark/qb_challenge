using System;
using System.Linq;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class TargetPresenter : MonoBehaviour
{
   
   public int Lane;
   //output text
   public Text TimerText;
   public Text Percentage;
   public Text SpeedText1,SpeedText2;
   
   //uis relates to player animation
   public RectTransform Target; //* ref from OnReadyState_AC to locate target
   public GameObject CompleteUI;
   public GameObject IncompleteUI;
   public GameObject Champion;
   public GameObject Looser;
   
   private Animator _animator;
   private CanvasGroup _canvas;
  private AppStateBroker _appStateBroker;
  
   private TeamManager _teamManager;
   private InputModule _inputModule;
   
   private Subject<float[]> _playerData;
   private BoolReactiveProperty _timWindow;
   private QBClient _currentClient {get{return _appStateBroker.ClientsList.First(c => c.Lane == Lane); }} 
   private void Awake()
   {
      _animator = transform.GetComponent<Animator>();
      _canvas = transform.GetComponent<CanvasGroup>();
      _appStateBroker = AppStateBroker.Instance;
      _teamManager = TeamManager.Instance;
      _timWindow = BindTimeWindow(Lane);
      _inputModule = InputModule.Instance;
     
      OnClientAdded(Initialize).AddTo(gameObject);
      Off();

      _playerData = BindPlayerData(Lane);
      
      OnTimeWindowClosed().AddTo(gameObject);
   }
   
   
   private void Initialize()
   {
      Observable.Timer(TimeSpan.FromMilliseconds(100))
         .Subscribe(a =>
         {
            var playSequence = _teamManager.PlayerSequences[Lane];
            playSequence
               .Where(ani => ani == AnimSequence.OnReadyExit)
               .Take(3)
               .Subscribe(_ =>
               {
                  Observable.Timer(TimeSpan.FromSeconds(1.5f))
                     .DoOnCompleted(On)
                     .Subscribe()
                     .AddTo(gameObject);
               })
               .AddTo(gameObject);
         }).AddTo(gameObject);
      
      _appStateBroker
         .CurrentRound
         .Where(round => round == Rounds.Finish)
         .Take(1)
         .Delay(TimeSpan.FromSeconds(1f))
         .Subscribe(x =>
         {
            CompleteUI.SetActive(false);
            IncompleteUI.SetActive(false);
            FadeManager.EnableCanvasGroup(_canvas,true);
            var client = _currentClient;
            var result = client.PassCompletionVal();
            var superbowl = result > 0;
            Percentage.text = string.Concat(result.ToString(), "%");
            
            Champion.SetActive(superbowl);
            Looser.SetActive(!superbowl);
            
         }).AddTo(gameObject);

      _appStateBroker
         .CurrentRound
         .Where(round => round == Rounds.Idle)
         .Skip(1)
         .Take(1)
         .Subscribe(_ =>
         {
            FadeManager.DisableCanvasGroup(_canvas,true);
            Champion.SetActive(false);
            Looser.SetActive(false);
         })
         .AddTo(gameObject);
   }

   private void Off()
   {
      FadeManager.DisableCanvasGroup(_canvas,true);
      IncompleteUI.SetActive(false);
      CompleteUI.SetActive(false);
      
   }

   private void On()
   {
      _timWindow.Value = true;
      _animator.SetTrigger("Show");
      FadeManager.EnableCanvasGroup(_canvas,true);
    
      Observable
         .Timer(TimeSpan.FromSeconds(1.5f))
         .DoOnCompleted(StartTimer)
         .Subscribe();
   }

   private IDisposable OnTimeWindowClosed()
   {
      return 
      _timWindow
         .Skip(1)
         .Where(open => !open)
         .Subscribe(_ =>
         {
            _animator.SetTrigger("Exit");
            _appStateBroker.CurrentRound      
               .Take(1)
               .Subscribe(round =>
               {
                  switch (round)
                  {
                     case Rounds.R1_Hike:
                        Observable.Timer(TimeSpan.FromMilliseconds(10))
                           .Subscribe(x =>
                           {
                              var spped1 = _currentClient.Result1;
                              var hit1 = _currentClient.Results[0].Hit;
                              SpeedText1.text = SpeedText2.text = spped1 + " mph";
                              CompleteUI.SetActive(hit1);
                              IncompleteUI.SetActive(!hit1);
                           }).AddTo(this);
                        break;
                     case Rounds.R2_Hike:
                        Observable.Timer(TimeSpan.FromMilliseconds(10))
                           .Subscribe(x =>
                           {
                              var spped2 = _currentClient.Result2;
                              var hit2= _currentClient.Results[1].Hit;
                              SpeedText1.text = SpeedText2.text = spped2+ " mph";
                              CompleteUI.SetActive(hit2);
                              IncompleteUI.SetActive(!hit2);
                           }).AddTo(this);
                        break;
                     case Rounds.R3_Hike:
                        Observable.Timer(TimeSpan.FromMilliseconds(10))
                           .Subscribe(x =>
                           {
                              var spped3 = _currentClient.Result3;
                              var hit3 = _currentClient.Results[2].Hit;
                              SpeedText1.text = SpeedText2.text = spped3 + " mph";
                              CompleteUI.SetActive(hit3);
                              IncompleteUI.SetActive(!hit3);

                           }).AddTo(this);
                        break;
                  }
               })
               .AddTo(this);	
            
         });
   }

   private IDisposable OnClientAdded(Action toInvoke)
   {
      return
         _appStateBroker
            .ClientsList
            .ObserveAdd()
            .Where(x => x.Value.Lane == Lane)
            .Subscribe(_ => toInvoke.Invoke());
   }
   
   private Subject<float[]> BindPlayerData(int lane)
   {
      switch (lane)
      {
         case 0:
            return  _inputModule.P1Data;
         case 1:
            return _inputModule.P2Data;
         case 2:
            return _inputModule.P3Data;
         default:
            return  _inputModule.P4Data;
      }
   }
   
   
   private BoolReactiveProperty BindTimeWindow(int lane)
   {
      switch (lane)
      {
         case 0:
            return  _appStateBroker.P1TimeWindow;
         case 1:
            return _appStateBroker.P2TimeWindow;
         case 2:
            return _appStateBroker.P3TimeWindow;
         default:
            return  _appStateBroker.P4TimeWindow;
      }
   }
   
   private void StartTimer()
   {
      var timer = 3f;
      Observable.EveryUpdate()
         .Select(_ => timer -= Time.deltaTime)
         .TakeWhile(time => time > -0.0001f)
         .DoOnCompleted(()=>
         {
            TimerText.text = "0.00";
            if (_timWindow.Value)
            {
               _timWindow.Value = false;
               _playerData.OnNext(new float[]{0,0,0});
            }
            Observable
               .Timer(TimeSpan.FromSeconds(GameManager.ResultDisplayDuration_External))
               .DoOnCompleted(Off)
               .Subscribe();
         })
         .Select(time => time.ToString("0.00"))
         .SubscribeToText(TimerText)
         .AddTo(gameObject);
   }
}
