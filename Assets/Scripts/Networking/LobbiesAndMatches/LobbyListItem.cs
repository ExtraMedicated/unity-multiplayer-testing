using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LobbyListItem : MonoBehaviour {

	public LobbyInfo lobby;

	[SerializeField] TMP_Text lobbyID;
	[SerializeField] TMP_Text lobbyName;
	public void UpdateUI(){
		lobbyID.text = lobby.id;
		lobbyName.text = $"{lobby.currentMembers}/{lobby.maxMembers} players"; //lobby.name;
	}

	public void OnClickLobby(){
		LobbyManager.instance.JoinLobby(lobby.connectionString);
	}

}
