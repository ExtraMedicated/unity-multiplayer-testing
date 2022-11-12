using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour {

	public LobbyWrapper lobby;
	[SerializeField] TMP_Text lobbyID;
	[SerializeField] TMP_Text lobbyName;
	[SerializeField] TMP_Text inProgress;
	[SerializeField] TMP_Text levelName;
	Selectable button;

	// public void Start(){
	// 	button = GetComponent<Selectable>();
	// }

	public void UpdateUI(){
		if (button == null){
			button = GetComponent<Selectable>();
		}
		if (lobby != null){
			lobbyID.text = lobby.LobbyName;
			lobbyName.text = $"{lobby.currentMembers}/{lobby.maxMembers} players"; //lobby.name;
			button.interactable = lobby.IsJoinable();
			levelName.text = lobby.levelName;
		}
		inProgress.gameObject.SetActive(lobby != null && lobby.isInProgress);
	}

	// public void SetLobbyInfo(Match match){
	// 	lobby = new LobbyInfo {
	// 		id = match.matchId,
	// 		levelName = match.level,
	// 		currentMembers = (uint) match.players.Count,
	// 		maxMembers = match.maxPlayers,
	// 		connectionString = match.matchId,
	// 		isInProgress = match.isInProgress,
	// 	};
	// 	UpdateUI();
	// }

	public void OnClickLobby(){
		// LobbyUtility.instance.JoinLobby(lobby.connectionString);
		var ui = FindObjectOfType<LobbiesUI>();
		ui.JoinLobby(lobby.connectionString);
	}

}
