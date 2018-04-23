using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Ref : MonoBehaviour
{
    public Animator Animator;
    public Subject<Unit> RefAniStream = new Subject<Unit>();
    public Camera PlayerCam;
    /// <summary>
    /// b/c of Ref need to be on/off every round so it pushes only 1 time and gets destroied.
    /// </summary>
    private void OnEnable()
    {
        Animator.GetBehaviour<RefCall_AC>()
            .OnStateExitObservable
            .Take(1)
            .Subscribe(_=> RefAniStream.OnNext(new Unit()))
            .AddTo(gameObject);
        
        Animator.GetBehaviour<RefCall_AC1>()
            .OnStateExitObservable
            .Take(1)
            .Subscribe(_=> RefAniStream.OnNext(new Unit()))
            .AddTo(gameObject);
        
        RefAniStream
            .Take(1)
            .Subscribe(x =>
            {
                PlayerCam.enabled = true;
                gameObject.SetActive(false);
            })
            .AddTo(gameObject);
    }

    public void MakeCallBasedOnResult(bool complete, int lane)
    {
        gameObject.SetActive(true);
        PlayerCam.enabled = false;
        if (complete)
        {
            Animator.SetTrigger("Complete");
            AudioManager.Instance.AudioSequenceObservable.OnNext(new Tuple<int, SFXType>(lane,SFXType.RefWhistleShort));
        }
        else
        {
            Animator.SetTrigger("Incomplete");
            AudioManager.Instance.AudioSequenceObservable.OnNext(new Tuple<int, SFXType>(lane,SFXType.RefWhistle));
        }
    }
}
