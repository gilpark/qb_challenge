using System;
using System.Collections;
using System.Collections.Generic;
using AudioStream;
using M1.Utilities;
using SharpOSC;
using UniRx;
using UnityEngine;

[System.Serializable]
public class LaneSignals
{
	public AudioSource[] signals;
}


public class AudioManager : SingletonBehaviour<AudioManager>
{
	public float SFX_Volume = 0.5f;
	public int Ambient_Volume = 128;

	public List<LaneSignals> laneSignalSources = new List<LaneSignals>(4);
	public Subject<UniRx.Tuple<int, SFXType>> AudioSequenceObservable = new Subject<UniRx.Tuple<int, SFXType>>();
	
	public List<AudioClip> CompleteClips = new List<AudioClip>();
	public List<AudioClip> InCompleteClips = new List<AudioClip>();
	public List<AudioClip> FinalCompleteClips = new List<AudioClip>();
	public List<AudioClip> RefWhistleClips = new List<AudioClip>();

	public bool DEVELOPER_MODE = true;
    public bool DISABLE = false;

	private AppStateBroker _appStateBroker;
	private readonly UDPSender _UdpSender = new SharpOSC.UDPSender("127.0.0.1", 53001);
	private float refVolumeOffset, CrowdVoluemOffset;
	
	private void Awake()
	{
		gameObject.SetActive(!bool.Parse(Config.Read(CONFIG_KEYS.audiodisable)));
		_appStateBroker = AppStateBroker.Instance;
		
		if (DEVELOPER_MODE)
		{
			AudioSourceOutputDevice[] channels = 
				transform.GetComponentsInChildren<AudioSourceOutputDevice>();

			foreach (AudioSourceOutputDevice c in channels)
			{
				c.enabled = false;
			}
		}

		SFX_Volume = float.Parse(Config.Read(CONFIG_KEYS.lanevolume));
		Ambient_Volume = int.Parse(Config.Read(CONFIG_KEYS.ambientvolume));
		refVolumeOffset = float.Parse(Config.Read(CONFIG_KEYS.refvolumeoffset));
		CrowdVoluemOffset = float.Parse(Config.Read(CONFIG_KEYS.crowdvolumeoffset));
		
		var message_vol = new SharpOSC.OscMessage("/maxVol", Ambient_Volume);
		_UdpSender.Send(message_vol);
	}

	private void Start()
	{
		
		//for each lanes
		AudioSequenceObservable
			.Subscribe(sq => PlaySound(sq.Item1,sq.Item2,SFX_Volume))
			.AddTo(gameObject);
		
		
		//for Ambient Sound
		_appStateBroker
			.CurrentRound
			.Subscribe(round =>
			{
				switch (round)
				{
					case Rounds.Idle:
						var message_idle = new SharpOSC.OscMessage("/idle");
						_UdpSender.Send(message_idle);
						break;
					case Rounds.R1:
						var message_r1_lineup = new SharpOSC.OscMessage("/p1","lineup");
						_UdpSender.Send(message_r1_lineup);
						break;
					case Rounds.R1_Hike:
						var message_r1_hike = new SharpOSC.OscMessage("/p1","hike");
						_UdpSender.Send(message_r1_hike);
						break;
					case Rounds.R2:
						var message_r2_lineup = new SharpOSC.OscMessage("/p2","lineup");
						_UdpSender.Send(message_r2_lineup);
						break;
					case Rounds.R2_Hike:
						var message_r2_hike = new SharpOSC.OscMessage("/p2","hike");
						_UdpSender.Send(message_r2_hike);
						break;
					case Rounds.R3:
						var message_r3_lineup = new SharpOSC.OscMessage("/p3","lineup");
						_UdpSender.Send(message_r3_lineup);
						break;
					case Rounds.R3_Hike:
						var message_r3_hike = new SharpOSC.OscMessage("/p3","hike");
						_UdpSender.Send(message_r3_hike);
						break;
					case Rounds.Finish:
						var message_finish = new SharpOSC.OscMessage("/idle");
						_UdpSender.Send(message_finish);
						break;
				}
			})
			.AddTo(gameObject);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			PlaySound(0, SFXType.CrowdComplete);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			PlaySound(1, SFXType.CrowdIncomplete);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			PlaySound(2, SFXType.RefWhistle);
		}
	}

	private void PlaySound(int lane, SFXType fx, float volume = 1f)
	{
	    if (DISABLE) return;
		if (lane >= laneSignalSources.Count) return;
		if (laneSignalSources[lane] == null) return;

		int audience = 0;
		int reff = 1;

		switch (fx)
		{
			case SFXType.CrowdComplete:
				SetClips(lane,audience,CompleteClips.PickRandom(),volume + CrowdVoluemOffset);
				break;
			case SFXType.CrowdIncomplete:
				SetClips(lane,audience,InCompleteClips.PickRandom(),volume + CrowdVoluemOffset);
				break;
			case SFXType.RefWhistle:
				SetClips(lane,reff,RefWhistleClips[0],volume - refVolumeOffset);
				break;
			case SFXType.RefWhistleShort:
				SetClips(lane,reff,RefWhistleClips[1],volume - refVolumeOffset);
				break;
			case SFXType.CompleteFinal:
				SetClips(lane,audience,FinalCompleteClips.PickRandom(),volume);
				break;
		}
	}

	private void SetClips(int lane, int target, AudioClip clip , float voluem = 1f)
	{
		laneSignalSources[lane].signals[target].volume = voluem;
		Debug.Log(voluem);
		laneSignalSources[lane].signals[target].clip = clip;
		laneSignalSources[lane].signals[target].Pause();
		laneSignalSources[lane].signals[target].Play();
	}



//	private void SetVolumeLevels()
//	{
//	    if (DEVELOPER_MODE)
//	    {
//	        SFX_Volume = 0.5f;
//	        Ambient_Volume = 0.5f;
//	    }
//
//		foreach (LaneSignals l in laneSignalSources)
//		{
//			foreach (AudioSource s in l.signals)
//			{
//				s.volume = SFX_Volume;
//			}
//		}
//
//		foreach (AudioSource a in laneAmbientSources)
//			a.volume = Ambient_Volume;
//	}
}
