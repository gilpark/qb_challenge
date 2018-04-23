using System;
using System.Collections.Generic;
using ETC.Platforms;
using UnityEngine;
using M1.Utilities;
using UniRx;
using UniRx.Triggers;

public class DMXController : SingletonBehaviour<DMXController>
{
    public static DMX dmx;
    public string Port = "COM3";
    public int Rows = 5;
    public int Columms = 3;
    private TeamManager _teamManager; 
    private ArrowLED[,] _leds;
    private readonly List<List<ArrowLED>> _ledLanes = new List<List<ArrowLED>>();

    private void Awake()
    {
        Port = Config.Read(CONFIG_KEYS.comport).ToUpper();
        var b = !bool.Parse(Config.Read(CONFIG_KEYS.dmxdisable));
        gameObject.SetActive(b);
       if(b)Init();
    }

    private void Init()
    { 
        _teamManager = TeamManager.Instance;
        dmx =  dmx??new DMX(Port);
        Reset();
        _leds = new ArrowLED[Columms,Rows];

        for (int x = 0; x < Columms; x++)
        {
            _ledLanes.Add(new List<ArrowLED>());
            for (int y = 0; y < Rows; y++)
            {
                var arrow = new ArrowLED((Columms * y + x) * 3);
                _ledLanes[x].Add(arrow);
                _leds[x, y] = arrow;
            }
        }
        
       _ledLanes.ForEach(list => list.Reverse());
        AppStateBroker.Instance
            .CurrentRound
            .Where(x => x == Rounds.Idle)
            .Subscribe(_=>Reset())
            .AddTo(gameObject);
        
        _teamManager
            .PlayerSequences
            .ObserveAdd()
            .Subscribe(lane =>
            {
                lane.Value
                    .Subscribe(sq =>
                    {
                        if (sq == AnimSequence.OnReadyExit)
                        {
                            AnimateLeds(lane.Key,3.5f);
                        }
                        else if (sq == AnimSequence.FinishEnter)
                        {
                            OffLast(lane.Key);
                        }
                    })
                    .AddTo(gameObject);
            }).AddTo(gameObject);
    }

    public void AnimateLeds(int lane, float duration)
    {
        var idx = 0;
        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(duration / 5))
            .TakeWhile(_ => idx < 5)
            .Subscribe(_ =>
            {
                if (idx == 4)
                {
                    _ledLanes[lane][idx].SetColor(Color.red, dmx);
                    _ledLanes[lane][idx-1].SetColor(Color.black, dmx);
                }
                else
                {
                    _ledLanes[lane][idx].SetColor(Color.white, dmx);
                    if (idx > 0)
                    {
                        _ledLanes[lane][idx-1].SetColor(Color.black, dmx);
                    }
                }
                idx++;

            })
            .AddTo(gameObject);
    }

    void OffLast(int lane)
    {
        _ledLanes[lane][4].SetColor(Color.black, dmx);
    }

    public void Reset()
    {
        if (dmx == null) return;
        dmx.Channels[1] = 0;
        dmx.Channels[2] = 0;
        dmx.Channels[3] = 0;
        dmx.Send();
    }

}