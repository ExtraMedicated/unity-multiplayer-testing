using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab.MultiplayerModels;

public class LobbyListItem : MonoBehaviour {

	public LobbyWrapper lobby;
	[SerializeField] TMP_Text playerCount;
	[SerializeField] TMP_Text lobbyName;
	[SerializeField] TMP_Text inProgress;
	[SerializeField] TMP_Text levelName;
	Selectable button;
	public void UpdateUI(){
		if (button == null){
			button = GetComponent<Selectable>();
		}
		if (lobby != null){
			ExtDebug.LogJson(lobby.searchData);
			lobbyName.text = lobby.searchData.GetValueOrDefault(LobbyWrapper.LOBBY_NAME_SEARCH_KEY);
			playerCount.text = $"{lobby.currentMembers}/{lobby.maxMembers} players";
			button.interactable = lobby.IsJoinable(); // TODO: How to tell whether the game is in progress?
			levelName.text = lobby.searchData.GetValueOrDefault(LobbyWrapper.LOBBY_LEVEL_SEARCH_KEY);
		}
		inProgress.gameObject.SetActive(lobby != null && (lobby.isInProgress || lobby._lobby?.MembershipLock == MembershipLock.Locked));
	}

	public void OnClickLobby(){
		var ui = FindObjectOfType<LobbiesUI>();
		ui.JoinLobby(lobby.connectionString);
	}

}
