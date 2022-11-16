using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.Multiplayer;
using PlayFab.Multiplayer.InteropWrapper;
using PlayFab.MultiplayerModels;
using UnityEngine;

public class MatchmakingUtility : MonoBehaviour
{
	public const string QUEUE_NAME = "DefaultQueue";

	static MatchmakingUtility instance;
	MatchmakingTicket _ticket;

	public static MatchmakingMatchDetails GetMatchDetails() => instance._ticket.GetMatchDetails();

	public static event Action<MatchmakingTicket> OnTicketCreated;
	public static event Action<MatchmakingTicketStatus> OnTicketStatusChanged;
	public static event Action OnTicketCanceled;

	void Awake(){
		if (instance != null){
			Destroy(gameObject);
			return;
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void OnEnable(){
		PlayFabMultiplayer.OnMatchmakingTicketCompleted += PF_OnMatchmakingTicketCompleted;
		PlayFabMultiplayer.OnMatchmakingTicketStatusChanged += PF_OnMatchmakingTicketStatusChanged;
	}

	void OnDisable(){
		PlayFabMultiplayer.OnMatchmakingTicketCompleted -= PF_OnMatchmakingTicketCompleted;
		PlayFabMultiplayer.OnMatchmakingTicketStatusChanged -= PF_OnMatchmakingTicketStatusChanged;
	}

	void OnDestroy(){
		// Should it cancel the ticket here?
		// CancelTicket();
	}

	private static void PF_OnTicketCanceled(CancelMatchmakingTicketResult result)
	{
		// ExtDebug.LogJson("OnTicketCanceled", result);
		OnTicketCanceled?.Invoke();
	}

	private void PF_OnMatchmakingTicketCompleted(MatchmakingTicket ticket, int result)
	{
		if (LobbyError.SUCCEEDED(result)){
			_ticket = ticket;
			Debug.Log("OnMatchmakingTicketCompleted");
			// ExtDebug.LogJson(ticket);
			OnTicketCreated?.Invoke(ticket);
		} else {
			Debug.Log("Failed to create matchmaking ticket.");
		}
	}

	public static void CreateMatchmakingTicket(){
		Debug.Log("CreateMatchmakingTicket");
		var matchUser = new MatchUser(PlayerEntity.LocalPlayer.entityKey, PlayerEntity.LocalPlayer.GetSerializedProperties());
		PlayFabMultiplayer.CreateMatchmakingTicket(matchUser, QUEUE_NAME, 120);
	}

	private void PF_OnMatchmakingTicketStatusChanged(MatchmakingTicket ticket)
	{
		Debug.Log("PF_OnMatchmakingTicketStatusChanged");
		// ExtDebug.LogJson(ticket);
		_ticket = ticket;
		OnTicketStatusChanged?.Invoke(ticket.Status);
		switch (ticket.Status){
			case MatchmakingTicketStatus.Creating: Debug.Log("--------Creating---------"); break;
			case MatchmakingTicketStatus.Joining: Debug.Log("--------Joining---------"); break;
			case MatchmakingTicketStatus.WaitingForPlayers: Debug.Log("--------WaitingForPlayers---------"); break;
			case MatchmakingTicketStatus.WaitingForMatch: Debug.Log("--------WaitingForMatch---------"); break;
			case MatchmakingTicketStatus.Matched: Debug.Log("--------Matched---------"); break;
			case MatchmakingTicketStatus.Canceled: Debug.Log("--------Canceled---------"); break;
			case MatchmakingTicketStatus.Failed: Debug.Log("--------Failed---------"); break;
		}
	}

	private static void OnMatchmakingError(PlayFabError error)
	{
		Debug.LogError(error.ErrorMessage);
	}

	public static void CancelTicket(){
		// TODO: Do these statuses make sense here?
		var checkStatuses = new List<MatchmakingTicketStatus> {
			MatchmakingTicketStatus.Creating,
			MatchmakingTicketStatus.Joining,
			MatchmakingTicketStatus.WaitingForPlayers,
			MatchmakingTicketStatus.WaitingForMatch,
			MatchmakingTicketStatus.Matched,
		};
		if (instance._ticket != null && checkStatuses.Contains(instance._ticket.Status)){
			PlayFabMultiplayerAPI.CancelMatchmakingTicket(
				new CancelMatchmakingTicketRequest
				{
					QueueName = QUEUE_NAME,
					TicketId = instance._ticket.TicketId,
				},
				PF_OnTicketCanceled,
				OnMatchmakingError);
		}
	}

	public static void GetMatch(){
		if (instance._ticket != null && instance._ticket.Status == MatchmakingTicketStatus.Matched){
			PlayFabMultiplayerAPI.GetMatch(
				new GetMatchRequest
				{
					MatchId = instance._ticket.TicketId,
					QueueName = QUEUE_NAME,
				},
				OnGetMatch,
				OnMatchmakingError);
		}
	}

	private static void OnGetMatch(GetMatchResult result)
	{
		// ExtDebug.LogJson("OnGetMatch :", result);
	}

	public static void GoToArrangedLobby(string LobbyArrangementString){
		Debug.Log("GoToArrangedLobby: " + LobbyArrangementString);
		PlayFabMultiplayer.JoinArrangedLobby(
			PlayerEntity.LocalPlayer.entityKey,
			LobbyArrangementString,
			new LobbyArrangedJoinConfiguration {
				AccessPolicy = LobbyAccessPolicy.Public, // TODO: Should this be configurable?
				MaxMemberCount = 5,
				OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
				MemberProperties = new Dictionary<string, string> {
					{PlayerEntity.PLAYER_NAME_KEY, PlayerEntity.LocalPlayer.name}
				}
			}
		);
	}
}
