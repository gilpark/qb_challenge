using System;
using System.Collections.Generic;
using System.Linq;
using ETC.Platforms;
using S7.Net;
using UniRx;
using UnityEngine;
using Debug = UnityEngine.Debug;

/*========= PLC & DATA  =========*/
#region PLC and Result Data

 public class RoundResult
{
    public Vector2 Coordinate; 
    public float Speed;
    
    public bool Hit = false;
    public bool Miss = false;
    public bool InComplete = false;
    
    public RoundResult()
    {
        Coordinate = Vector2.zero;
        Speed = 0f;
    }

    public void UpdateScore(bool hit,float speed)
    {
        Speed = speed;
        Hit = hit;
        Miss = !hit;
        InComplete = speed == 0 || !hit;
    }

}

public static class PLCdata
{
    //general
    private const string FeetReal = "DB9.DBD62";
    private const string IncompleteInt = "DB9.DBW66";
    private const string HikeInt = "DB9.DBW60";
   
    public static ErrorCode SetHike(Plc plc)
    {
        return plc.Write(HikeInt, 1);  
    }

    public static int GetHike(Plc plc)
    {
        return   ((ushort) plc.Read(HikeInt)).ConvertToShort();
    }

    public static ErrorCode SetDistance(Plc plc, double distance)
    {
        return 
            plc.Write(FeetReal, distance.ConvertToUInt());
    }

    public static double GetDistance(Plc plc)
    {
        return ((uint)plc.Read(FeetReal)).ConvertToDouble(); 
    }

    public static ErrorCode SetTTT(Plc plc, float duration)
    {
        return
            plc.Write(IncompleteInt, (duration * 1000));
    }

    public static int GetTTT(Plc plc)
    {
        return
            ((ushort) plc.Read(IncompleteInt)).ConvertToShort();
    }
    public static class P1
    {
        //P1
        private const string Mph = "DB9.DBD4";
        private const string Ttt = "DB9.DBD0";
        private const string X = "DB9.DBW8";
        private const string Y = "DB9.DBW10";

        public static float[] GetData(Plc plc)
        {
            return new[]
            {
                (float)(((uint)plc.Read(Mph)).ConvertToDouble()), //mph
                (float) (((ushort) plc.Read(X)).ConvertToShort()), //x
                (float) (((ushort) plc.Read(Y)).ConvertToShort()) //y
            };
        }
    }

    public static class P2
    {
        //P2
        private const string Mph = "DB9.DBD16";
        private const string Ttt = "DB9.DBD12";
        private const string X = "DB9.DBW20";
        private const string Y = "DB9.DBW22";
        
        public static float[] GetData(Plc plc)
        {
            return new[]
            {
                (float)(((uint)plc.Read(Mph)).ConvertToDouble()), //mph
                (float) (((ushort) plc.Read(X)).ConvertToShort()), //x
                (float) (((ushort) plc.Read(Y)).ConvertToShort()) //y
            };
        }
    }
    public static class P3
    {
        //p3    
        private const string Mph = "DB9.DBD28";
        private const string Ttt = "DB9.DBD24";
        private const string X = "DB9.DBW32";
        private const string Y = "DB9.DBW34";
        
        
        public static float[] GetData(Plc plc)
        {
            return new[]
            {
                (float)(((uint)plc.Read(Mph)).ConvertToDouble()), //mph
                (float) (((ushort) plc.Read(X)).ConvertToShort()), //x
                (float) (((ushort) plc.Read(Y)).ConvertToShort()) //y
            };
        }
    }
    public static class P4
    {
        //p4 
        private const string Mph = "DB9.DBD40";
        private const string Ttt = "DB9.DBD36";
        private const string X = "DB9.DBW44";
        private const string Y = "DB9.DBW46";
        public static float[] GetData(Plc plc)
        {
            return new[]
            {
                (float)(((uint)plc.Read(Mph)).ConvertToDouble()), //mph
                (float) (((ushort) plc.Read(X)).ConvertToShort()), //x
                (float) (((ushort) plc.Read(Y)).ConvertToShort()) //y
            };
        }
    }
}

public class QBClient
{
    public int ClientId;
    public int Lane;
    public int Team;
    public bool R1Throw, R2Throw, R3Throw; 
    public bool R1Done, R2Done, R3Done;
    public string Result1, Result2, Result3;
    public string PlayerName, PlayerPercentage;
    
    public RoundResult[] Results = new RoundResult[3];
    
    public QBClient(int clientID, int laneID, int team, string playeNname, string playerPercentage )
    {
        ClientId = clientID;
        Lane = laneID;
        Team = team;
        PlayerName = playeNname;
        PlayerPercentage = playerPercentage;
        
        for (int i = 0; i < Results.Length; i++)
        {
            Results[i] = new RoundResult();
        }
    }

    public void UpdateTeam(int team)
    {
        Team = team;
    }

    public void SetR1Score(bool hit, float speed)
    {
        Results[0].UpdateScore(hit, speed);
        var _speed = Results[0].Speed.ToString("0.##");
        Result1 = _speed;
        R1Throw = true;
    }

    public void SetR2Score(bool hit,  float speed)
    {
        Results[1].UpdateScore(hit,  speed);
        var _speed = Results[1].Speed.ToString("0.##");
        Result2 = _speed;
        R2Throw = true;
    }
    public void SetR3Score(bool hit, float speed)
    {
        Results[2].UpdateScore(hit, speed);
        var _speed = Results[2].Speed.ToString("0.##");
        Result3 = _speed;
        R3Throw = true;
    }

    public int PassCompletionVal()
    {
        var r1 = Results[0].Hit ? 33 : 0;
        var r2 = Results[1].Hit ? 33 : 0;
        var r3 = Results[2].Hit ? 33 : 0;
        return r1 + r2 + r3 == 99 ? 100 : r1 + r2 + r3;
    }
}

#endregion

/*========= Animation  =========*/
#region Animation related
/// <summary>
/// AnimIndexProvider:
/// Rent and store indicies to avoid play same animation across players
/// </summary>
public static class AnimIndexProvider
{
    private static Queue<int> _idleAniIndex;
    private static Queue<float> _catchAniIndex;
    private static Queue<float> _hitAniIndex;
    private static Queue<float> _missAniIndex;
    private static Vector3[] _targetPositions = 
    {
        new Vector3(1f, -328f, 0f),
        new Vector3(28f, 188f, 0f),
        new Vector3(-45f, 1f, 0f),
        new Vector3(-63f, 264f, 0f),
       // new Vector3(1f, -328f, 0f)
    };
    
    public static Vector3 GetTargetPos(float catchBlendVal)
    {
        var pos = Vector3.zero;

        if (catchBlendVal == 0f)
            pos = _targetPositions[0];
        else if (catchBlendVal == 0.3333f)
        {
            pos = _targetPositions[1];
        }
        else if (catchBlendVal == 0.66666f)
        {
            pos = _targetPositions[2];
        }
        else if (catchBlendVal == 1f)
        {
            pos = _targetPositions[3];
        }
        return pos;
    }
    
    public static int GetIdleAniIndex()
    {
        var idx = _idleAniIndex.Dequeue();
        _idleAniIndex.Enqueue(idx);
        return idx;
    }

    public static float GetCatchBlendVal()
    {
        var val = _catchAniIndex.Dequeue();
        _catchAniIndex.Enqueue(val);
        return val;
    }
    
    public static float GetHitBlendVal()
    {
        var val = _hitAniIndex.Dequeue();
        _hitAniIndex.Enqueue(val);
        return val;
    }
    
    public static float GetMissBlendVal()
    {
        var val = _missAniIndex.Dequeue();
        _missAniIndex.Enqueue(val);
        return val;
    }
    
    static AnimIndexProvider()
    {

        _idleAniIndex = new Queue<int>(5);
        for (int i = 0; i < 5; i++) _idleAniIndex.Enqueue(i);
        
        _catchAniIndex = _hitAniIndex = _missAniIndex = new Queue<float>(4);
        
        List<float> blendValues = new List<float>{0f, 0.3333f, 0.66666f, 1f, 0.66666f, 0.3333f, 1f};
        
        blendValues.ForEach(v =>
        {
            _catchAniIndex.Enqueue(v);
            _hitAniIndex.Enqueue(v);
            _missAniIndex.Enqueue(v);
        });
    }
    
}

public static class PlayerSequenceIndx
{
    public static int GetIndex(string parentname)
    {
        var idx = -1;
        switch (parentname)
        {
            case "Player1": idx = 0; break;
            case "Player2": idx = 1; break;
            case "Player3": idx = 2; break;
            case "Player4": idx = 3; break;
        }
        return idx;
    }
}
#endregion

/*========= Others  =========*/
#region others


public static class EnumerableExtension
{
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }
}

public static class AutoTestIndexProvider
{
    private static Queue<int> _teamIndex;
    private static Queue<Vector2> _testTargetVal;
    private static Vector2[] _testTargetVec 
        = 
    {
        new Vector2(348,711),
        new Vector2(283,1287),
        new Vector2(306,983),
        new Vector2(367,1189)
    };

    
    static AutoTestIndexProvider()
    {

        _teamIndex = new Queue<int>(32);
        for (int i = 0; i < 32; i++) _teamIndex.Enqueue(i);
        
        _testTargetVal = new Queue<Vector2>(4);
        
        _testTargetVec.ToList().ForEach(v =>
        {
            _testTargetVal.Enqueue(v);
        });
    }
    
    public static int GetTeamIndex()
    {
        var val = _teamIndex.Dequeue();
        _teamIndex.Enqueue(val);
        return val;
    }
    
    public static Vector2 GetTestTargetVal()
    {
        var val = _testTargetVal.Dequeue();
        _testTargetVal.Enqueue(val);
        return val;
    }
}

[Serializable]
public class JoinButton
{
    public int Lane;
    public UnityEngine.UI.Slider Slider;
    public UnityEngine.UI.Button Button;
    public UnityEngine.UI.Text Text;
}

[System.Serializable]
public class Team
{
    public TeamType team;
    public GameObject actor;

    public float actorHeight = 0.45f;
    public bool isTightEnd = false;

    public string playerName = "Tyrod Taylor";
    public string completionPercentage = "62.30";
}


public class ArrowLED
{
    private int r, g, b;

    public ArrowLED(int startAddress)
    {
        r = startAddress + 1;
        g = startAddress + 2;
        b = startAddress + 3;
    }

    public void SetColor(Color c, DMX dmx, params Action[] toInvoke)
    {
        if (dmx != null)
        {
            var rval = c.r.FromTo(0, 1, 0, 255);
            var gval = c.g.FromTo(0, 1, 0, 255);
            var bval = c.b.FromTo(0, 1, 0, 255);

            dmx.Channels[r] = (byte)rval;
            dmx.Channels[g] = (byte)gval;
            dmx.Channels[b] = (byte)bval;
            try
            {
                dmx.Send();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
          
        }
       
    }

    public void Print()
    {
        Debug.LogFormat("LED r: {0} g:{1} b:{2}",r,g,b);
    }

}

public static class DataPath
{
    public static string PREBUILD_CONFIG = Application.dataPath + "/Config/";
    public static string POSTBUILD_CONFIG = Application.dataPath + "/../Config/";
}
[Serializable]
public class LaneConfig
{
    [SerializeField]
    public float PosX,PosY;
    [SerializeField]
    public float ScaleX,ScaleY;
    [SerializeField]
    public float Radius;
}
#endregion

/*========= Rx  =========*/
#region Rx Utilities

/// <summary>
/// RoundsReactiveProperty:
/// Keeps tacking of current rounds and notifyes subscribers
/// </summary>
[System.Serializable] 
public class RoundsReactiveProperty : ReactiveProperty<Rounds>
{
    public RoundsReactiveProperty(){}
    public RoundsReactiveProperty(Rounds initialValue) : base(initialValue){}
}
/// <summary>
/// PlayerSquenceReactiveProperty:
/// Keeps tacking of player animation sequences
/// </summary>
[System.Serializable] 
public class PlayerSquenceReactiveProperty : ReactiveProperty<AnimSequence>
{
    public PlayerSquenceReactiveProperty(){}
    public PlayerSquenceReactiveProperty(AnimSequence initialValue) : base(initialValue){}
}

public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public BitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}

#endregion

/*========= Enums =========*/
#region Enums

public enum Rounds { Idle,Idle_Hike,R1,R1_Hike,R2,R2_Hike,R3,R3_Hike,Finish }

public enum HitMiss { Hit,Miss,InComplete }

public enum AnimSequence
{
    OnReadyEnter,OnReadyExit,
    CatchEnter, CatchExit,
    HitEnter, HitExit,
    MissEnter, MissExit,
    FinishEnter, FinishExit
}

public enum SFXType
{
    CrowdComplete, 
    CrowdIncomplete, 
    CompleteFinal, 
    RefWhistle, 
    RefWhistleShort,
//    GeneralCrowd,
//    ExcitedCrowd,
//    StartClapping,
//    EndClapping
}


public enum TeamType
{
    __ = -1,
    Arizona_Cardinals = 0,
    Atlanta_Falcons = 1,
    Baltimore_Ravens = 2,
    Buffalo_Bills = 3,
    Carolina_Panthers = 4,
    Chicago_Bears = 5,
    Cincinnati_Bengals = 6,
    Cleveland_Browns = 7,
    Dallas_Cowboys = 8,
    Denver_Broncos = 9,
    Detroit_Lions = 10,
    Green_Bay_Packers = 11,
    Houston_Texans = 12,
    Indianapolis_Colts = 13,
    Jacksonville_Jaguars = 14,
    Kansas_City_Chiefs = 15,
    Los_Angeles_Chargers = 16,
    Los_Angeles_Rams = 17,
    Miami_Dolphins = 18,
    Minnesota_Vikings = 19,
    New_England_Patriots = 20,
    New_Orleans_Saints = 21,
    New_York_Giants = 22,
    New_York_Jets = 23,
    Oakland_Raiders = 24,
    Philadelphia_Eagles = 25,
    Pittsburgh_Steelers = 26,
    San_Francisco_49ers = 27,
    Seattle_Seahawks = 28,
    Tampa_Bay_Buccaneers = 29,
    Tennessee_Titans = 30,
    Washington_Redskins = 31
}

public enum TargetLane
{
    Lane1          = (1<<0),
    Lane2          = (1<<1),
    Lane3          = (1<<2),
    Lane4          = (1<<3),
}

public enum TargetObject
{
    InputModule      = (1<<0),
    PLCModule        = (1<<1),
    Plyaer           = (1<<2),
    AppStateBroker   = (1<<3),
    TCP              = (1<<4),
    
}

#endregion