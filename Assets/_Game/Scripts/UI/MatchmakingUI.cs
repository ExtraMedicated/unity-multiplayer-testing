using System;
using System.Collections;
using System.Collections.Generic;
// using PlayFab.Multiplayer;
using UnityEngine;

public class MatchmakingUI : MonoBehaviour {
	[SerializeField] UIText statusText;
	// void OnEnable(){
	// 	MatchmakingUtility.OnTicketCanceled += OnTicketCanceled;
	// 	MatchmakingUtility.OnTicketStatusChanged += OnTicketStatusChanged;
	// }

	// void OnDisable(){
	// 	MatchmakingUtility.OnTicketCanceled -= OnTicketCanceled;
	// 	MatchmakingUtility.OnTicketStatusChanged -= OnTicketStatusChanged;
	// 	LobbyArrangementString = string.Empty;
	// }

	public void StartMatchmaking(){
		// statusText.text = "Creating Ticket";
		// MatchmakingUtility.CreateMatchmakingTicket();
	}
	public void GoToLobby(){
		// MatchmakingUtility.GoToArrangedLobby(LobbyArrangementString);
	}
	public void CancelMatchmaking(){
		// MatchmakingUtility.CancelTicket();
	}
	void OnTicketCanceled(){
		// gameObject.SetActive(false);
	}

	// string LobbyArrangementString;
	// private void OnTicketStatusChanged(MatchmakingTicketStatus status)
	// {
	// 	statusText.text = status.ToString();
	// 	if (status == MatchmakingTicketStatus.Matched){
	// 		var deets = MatchmakingUtility.GetMatchDetails();
	// 		ExtDebug.LogJson("Details: ", deets);
	// 		LobbyArrangementString = deets.LobbyArrangementString;
	// 		// Why not just go right away?
	// 		MatchmakingUtility.GoToArrangedLobby(LobbyArrangementString);
	// 	}
	// }
}
