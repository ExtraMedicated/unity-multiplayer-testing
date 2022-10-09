using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour {

	public LobbyInfo lobby;
	[SerializeField] TMP_Text lobbyID;
	[SerializeField] TMP_Text lobbyName;
	Selectable button;

	// public void Start(){
	// 	button = GetComponent<Selectable>();
	// }

	public void UpdateUI(){
		if (button == null){
			button = GetComponent<Selectable>();
		}
		if (lobby != null){
			lobbyID.text = lobby.id;
			lobbyName.text = $"{lobby.currentMembers}/{lobby.maxMembers} players"; //lobby.name;
			button.interactable = lobby.IsJoinable();
		}
	}

	public void SetLobbyInfo(Match match){
		lobby = new LobbyInfo {
			id = match.matchId,
			currentMembers = (uint) match.players.Count,
			maxMembers = match.maxPlayers,
			connectionString = match.matchId,
			isInProgress = match.isInProgress,
		};
		UpdateUI();
	}

	public void OnClickLobby(){
		LobbyUtility.instance.JoinLobby(lobby.connectionString);
	}

}
