//using UnityEngine;
//using System.Collections;
//using M1.Utilities;
//using System;
//using UniRx;
//using UnityEngine.UI;
//
//[Serializable]
//public class QB_Round1State : MonoBehaviour, IState
//{
//    public CanvasGroup uiPanel;
//    private Player_bk P1, P2, P3;
//    private LaneManager_bk _laneManagerBk;
//
//    [Header("TESTING & DEBUGGING")] 
//    public Button Hit1;
//    public Button Hit2;
//    public Button Hit3;
//    public Button Throw;
//    public Text Mph;
//    public GameObject HitText, MissText;
//    private void Awake()
//    {
//        MessageBroker.Default.Receive<TestResultParamas>()
//            .ObserveOn(Scheduler.MainThread)
//            .SubscribeOnMainThread()
//            .Subscribe(p =>
//        {
//            if (p.Hit)
//            {
//                P1.SetCatchState();
//                P1.SetThrow();
//                P1.targetUI.SetActive(false);
//                AudioManager.Instance.PlaySound(0, AudioManager.SFXType.CrowdComplete);
//            }
//            HitText.SetActive(p.Hit);
//            MissText.SetActive(!p.Hit);
//            Mph.gameObject.SetActive(true);
//            Mph.text = p.Mph.ToString();
//            
//        }).AddTo(this.gameObject);
//        
//        _laneManagerBk = LaneManager_bk.Instance;
//        P1 = _laneManagerBk.Lane1.Plyaer;
//        P2 = _laneManagerBk.Lane2.Plyaer;
//        P3 = _laneManagerBk.Lane3.Plyaer;
//        FadeManager.DisableCanvasGroup(uiPanel,true);
//        
//        Hit1.gameObject.SetActive(false);
//        Hit2.gameObject.SetActive(false);
//        Hit3.gameObject.SetActive(false);
//        Throw.gameObject.SetActive(false);
//        
//        HitText.SetActive(false);
//        MissText.SetActive(false);
//        Mph.gameObject.SetActive(false);
//    }
//
//    private void Init()
//    {
//        HitText.SetActive(false);
//        MissText.SetActive(false);
//        Mph.gameObject.SetActive(false);
//        
//        _laneManagerBk.Lane1.LaneState = LaneState.Ready;
//        _laneManagerBk.Lane2.LaneState = LaneState.Ready;
//        _laneManagerBk.Lane3.LaneState = LaneState.Ready;
//
//        AppInputManager_bk.Instance.SnapButtonStream.Take(1).Subscribe(_ =>
//        {
//            TargetManager.Instance.PrepAndDisplayTarget();
//            DMXController.PlayMainLightSequence();
//
//
//            P1.GotoRunState();
//            P2.GotoRunState();
//            P3.GotoRunState();
//            Throw.gameObject.SetActive(false);
//        }).AddTo(this.gameObject);
//        
//        Throw.gameObject.SetActive(true);
//        Throw.OnClickAsObservable().Subscribe(_ =>
//        {
//            TargetManager.Instance.PrepAndDisplayTarget();
//            DMXController.PlayMainLightSequence();
//
//            P1.GotoRunState();
//            P2.GotoRunState();
//            P3.GotoRunState();
//            Throw.gameObject.SetActive(false);
//            AppInputManager_bk.Instance.Hike();
//
//        });
//        
//        Hit1.OnClickAsObservable().Subscribe(_ =>
//        {
//
//            Debug.Log("hit 1");
//            P1.SetCatchState();
//            P1.SetThrow();
//            P1.targetUI.SetActive(false);
//
//            Hit1.gameObject.SetActive(false);
//        });
//       
//        Hit2.OnClickAsObservable().Subscribe(_ =>
//        {
//            Debug.Log("hit 2");
//            P2.SetCatchState();
//            P2.SetThrow();
//            P2.targetUI.SetActive(false);
//
//            Hit2.gameObject.SetActive(false);
//        });
//        
//        Hit3.OnClickAsObservable().Subscribe(_ =>
//        {
//            Debug.Log("hit 3");
//            P3.SetCatchState();
//            P3.SetThrow();
//            P3.targetUI.SetActive(false);
//
//            Hit3.gameObject.SetActive(false);
//        });
//        
//        P1.Go.Subscribe(b =>
//        {
//            if (b)Hit1.gameObject.SetActive(b);
//            if(!b)Hit1.gameObject.SetActive(b);
//            if (b) StartCoroutine(time1());
//
//        });
//        P2.Go.Subscribe(b =>
//        {
//            if (b)Hit2.gameObject.SetActive(b);
//            if(!b)Hit2.gameObject.SetActive(b);
//            if (b)StartCoroutine(time2());
//            
//        });
//        P3.Go.Subscribe(b =>
//        {
//            if (b)Hit3.gameObject.SetActive(b);
//            if(!b)Hit3.gameObject.SetActive(b);
//            if (b)StartCoroutine(time3());
//        });
//
//        Observable.EveryUpdate().Select(_ =>
//        {
//            if (P1.Finish.Value && P2.Finish.Value && P3.Finish.Value) return true;
//            else
//            {
//                return false;
//            }
//        }).Where(b => b).Take(1).Subscribe(_ =>
//        {
//
//            GameManager_bk.ChangeState(ServerAppState.R2);
//        });
//
//    }
//
//    IEnumerator time1()
//    {
//        Debug.Log("StartWaiting for p1");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......1");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......2");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......3");
//
//        
//
//        P1.SetThrow();
//        P1.targetUI.SetActive(false);
//
//        Debug.Log("DoneWaiting for p1");
//
//        yield return new WaitForSeconds(2f);
//
//        TeamManager.Instance.CutToRef(0);
//        TeamManager.Instance.CutToRef(1);
//        TeamManager.Instance.CutToRef(2);
//    }
//    IEnumerator time2()
//    {
//        Debug.Log("StartWaiting for p2");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......1");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......2");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......3");
//        P2.SetThrow();
//        P2.targetUI.SetActive(false);
//
//        AudioManager.Instance.PlaySound(0, AudioManager.SFXType.CrowdIncomplete);
//
//        Debug.Log("DoneWaiting for p2");
//    }
//    IEnumerator time3()
//    {
//        Debug.Log("StartWaiting for p3");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......1");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......2");
//        yield return new WaitForSeconds(1f);
//        Debug.Log(".......3");
//        P3.SetThrow();
//        P3.targetUI.SetActive(false);
//
//        AudioManager.Instance.PlaySound(0, AudioManager.SFXType.CrowdIncomplete);
//
//
//        Debug.Log("DoneWaiting for p3");
//    }
//    public void Execute()
//    {
//    }
//
//    public IEnumerator iEnter()
//    {
//        Init();
//        FadeManager.EnableCanvasGroup(uiPanel,false);
//        FadeManager.FadeIn(uiPanel, 0.5f);  //Fade this state's UI elements in
//        Debug.Log(this.name + " Enter: " + Time.time);
//        yield return new WaitForSeconds(0.5f);
//
//        if (GameManager_bk.Instance.Is_Automation_Test_Build)
//            StartCoroutine(iAutomatedTest());
//    }
//
//    public IEnumerator iExit()
//    {
//        FadeManager.FadeOut(uiPanel, 0.5f, true);
//
//
//        /////
//        P1.Done();
//        P2.Done();
//        P3.Done();
//        Debug.Log(this.name + " Exit: " + Time.time);
//        yield return new WaitForSeconds(0.5f);
//        /////
//        //P1.Reset();
//        //P2.Reset();
//        //P3.Reset();
//        ///
//        HitText.SetActive(false);
//        MissText.SetActive(false);
//        Mph.gameObject.SetActive(false);
//    }
//
//    public void ButtonDown(int _num)
//    {
//        Debug.Log(_num + " ButtonDown: " + Time.time);
//        GameManager_bk.NextState();
//    }
//
//    public void ButtonUp(int _num)
//    {
//        Debug.Log(_num + " ButtonUp: " + Time.time);
//    }
//
//    public IState GetNextState()
//    {
//        return null;
//    }
//
//    public IEnumerator iAutomatedTest()
//    {
//        yield return new WaitForSeconds(1f);
//        ButtonDown(0);
//    }
//}
