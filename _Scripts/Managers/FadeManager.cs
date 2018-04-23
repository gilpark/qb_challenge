using System;
using UnityEngine;
using System.Collections;
using M1.Utilities;
using UniRx;

public class FadeManager : SingletonBehaviour<FadeManager>
{


    public static void EnableCanvasGroup(CanvasGroup c, bool setoOne)
    {
        Instance.enableCanvasGroup(c, setoOne);
    }

    private void enableCanvasGroup(CanvasGroup c, bool setoOne)
    {
        c.interactable = true;
        c.blocksRaycasts = true;
        if (setoOne) c.alpha = 1f;
    }

    public static void DisableCanvasGroup(CanvasGroup c, bool setoZero)
    {
        Instance.disableCanvasGroup(c, setoZero);
    }

    private void disableCanvasGroup(CanvasGroup c, bool setoZero)
    {
        c.interactable = false;
        c.blocksRaycasts = false;
        if (setoZero) c.alpha = 0f;
    }
    
    public static void FadeIn(CanvasGroup c, float duration ,Action extrafunc = null,bool interactable = true, float alpha = 1f, bool keepOn = true)
    {
        var timer = 0f;
        c.gameObject.SetActive(true);
        var o = Observable.EveryUpdate()
            .Select(_ => timer += Time.deltaTime)
            .TakeWhile(x=> x < duration)
            .Select(x=>x.FromTo(0,duration,0,alpha))
            .DoOnCompleted(() =>
            {
                c.alpha = alpha;
                c.interactable = interactable;
                c.blocksRaycasts = interactable;
                c.gameObject.SetActive(keepOn);
                if(extrafunc !=null)extrafunc.Invoke();
            })
            .Subscribe(x =>
            {
                c.alpha = x;
            });
    }
	
    public static void FadeOut(CanvasGroup c, float duration , Action extrafunc = null,bool interactable = false, float alpha = 0f, bool keepOn = false)
    {
        var timer = 0f;
        c.gameObject.SetActive(true);
        
        var o = Observable.EveryUpdate()
            .Select(_ => timer += Time.deltaTime)
            .TakeWhile(x=> x < duration)
            .Select(x=>x.FromTo(0,duration,1,alpha))
            .DoOnCompleted(() =>
            {
                c.alpha = alpha;
                c.interactable = interactable;
                c.blocksRaycasts = interactable;
                c.gameObject.SetActive(keepOn);
                if(extrafunc !=null)extrafunc.Invoke();
            })
            .Subscribe(x =>
            {
                c.alpha = x;
            });
    }
    
}
