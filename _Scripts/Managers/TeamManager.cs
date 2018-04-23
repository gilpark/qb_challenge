using System;
using System.Collections;
using System.Collections.Generic;
using M1.Utilities;
using UniRx;
using UnityEngine;


public class TeamManager : SingletonBehaviour<TeamManager>
{
	
	public Rounds CurrentRound = Rounds.Idle;
	public List<Team> Teams = new List<Team>();
	public List<Player> Players = new List<Player>();
	public List<Ref> Refs = new List<Ref>();

	public ReactiveDictionary<int,PlayerSquenceReactiveProperty> PlayerSequences 
		= new ReactiveDictionary<int, PlayerSquenceReactiveProperty>();
	
	public TargetPresenter[] TargetPresenters = new TargetPresenter[4];
	
	public RuntimeAnimatorController IdleController;
	public RuntimeAnimatorController TightEndController;
	public RuntimeAnimatorController WideController;
	
	private AppStateBroker _appStateBroker;

	private void Start ()
	{
		_appStateBroker = AppStateBroker.Instance;
		
		Teams.ForEach(team =>
		{
			var key = Enum.Parse(typeof(CONFIG_KEYS), team.team.ToString().ToLower());
			var stats = Config.Read((CONFIG_KEYS)key).Split(',');
			var qbName = stats[0];
			var percentage = stats[1];
			team.playerName = qbName.Replace("_", " ");
			team.completionPercentage = percentage;

		});
		
		_appStateBroker
			.ClientsList
			.ObserveAdd()
			.Subscribe(x =>
			{
				try
				{
					var client = x.Value;
					SelectTeam(client.ClientId,client.Lane,client.Team);
				}
				catch (Exception e)
				{
					Debug.Log(e);
					throw;
				}
				
			}).AddTo(gameObject);
		
		_appStateBroker
			.CurrentRound
			.Where(round => round == Rounds.Idle)
			.Subscribe(x =>PlayerSequences.Clear())
			.AddTo(gameObject);
	}

	public void SelectTeam(int clientId, int lane, int teamIdx)
	{
		if (!PlayerSequences.ContainsKey(lane))
		{
			PlayerSequences.Add(lane,new PlayerSquenceReactiveProperty());
			GameObject actor = Instantiate(Teams[teamIdx].actor, Vector3.zero,
				Quaternion.identity, Players[lane].transform);

			RuntimeAnimatorController animatorController = Teams[teamIdx].isTightEnd ? TightEndController : WideController; 
		
			Players[lane]
				.Initialize(lane, actor, IdleController ,animatorController, Teams[teamIdx].actorHeight, PlayerSequences[lane]);
		}
		

	}

	public string[] GetPlayerData(int team)
	{
		return new[] {Teams[team].playerName, Teams[team].completionPercentage};
	}

}

