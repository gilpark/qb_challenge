using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class QBIdle : SingletonBehaviour<QBIdle>
{
    public CanvasGroup UiPanel;
    
    private void Awake()
    {
        FadeManager.DisableCanvasGroup(UiPanel,true);
    }

    private void Start()
    {
        ORTCPMultiServer.Instance.DisconnectAllClients();
    }

    public void OnEnter()
    {
       // ORTCPMultiServer.Instance.DisconnectAllClients();
        FadeManager.FadeIn(UiPanel, 0.5f);
//        if (GameManager.Instance.Is_Automation_Test_Build)
//            StartCoroutine(iAutomatedTest());
    }

    public void OnExit()
    {
        FadeManager.FadeOut(UiPanel, 0.5f);
//        foreach (Player p in TeamManager.Instance.players)
//        {
//            if (!p.active) continue;
//            p.JoinGame();
//        }
    }

    public IEnumerator iAutomatedTest()
    {
        yield return new WaitForSeconds(2f);
    }
}
