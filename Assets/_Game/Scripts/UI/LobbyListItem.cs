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
	public void UpdateUI(){
		if (button == null){
			button = GetComponent<Selectable>();
		}
		if (lobby != null){
			lobbyID.text = lobby.LobbyName;
			lobbyName.text = $"{lobby.currentMembers}/{lobby.maxMembers} players";
			button.interactable = lobby.IsJoinable();
			levelName.text = lobby.levelName;
		}
		inProgress.gameObject.SetActive(lobby != null && lobby.isInProgress);
	}

	public void OnClickLobby(){
		var ui = FindObjectOfType<LobbiesUI>();
		ui.JoinLobby(lobby.connectionString);
	}

}
