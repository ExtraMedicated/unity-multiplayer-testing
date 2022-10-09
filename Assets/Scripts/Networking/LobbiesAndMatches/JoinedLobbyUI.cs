using System.Collections;
using System.Collections.Generic;
// using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using System;

public class JoinedLobbyUI : MonoBehaviour {

	// public Lobby lobby;
	public Match match;

	[SerializeField] TMP_Text lobbyText;
	[SerializeField] Transform playerListPanel;
	[SerializeField] Button leaveLobbyBtn;
	[SerializeField] Button startGameBtn;
	public GameObject playerListItemPrefab;

	void OnEnable(){
		leaveLobbyBtn.interactable = true;
		if (match != null){
			RefreshUI();
		}
		// PlayFabMultiplayer.OnLobbyMemberAdded += OnMemberAdded;
		// PlayFabMultiplayer.OnLobbyMemberRemoved += OnMemberRemoved;
		// StartCoroutine(DelayedOnEnable());
		// MatchManager2.instance.OnUpdateMatch += UpdateMatch;
	}

	// IEnumerator DelayedOnEnable(){
	// 	yield return new WaitForEndOfFrame();
	// 	MatchManager2.instance.OnUpdateMatch += OnUpdateMatch;
	// }

	void OnDisable(){
		// if (MatchManager2.instance != null)
		// MatchManager2.instance.OnUpdateMatch -= UpdateMatch;
		// PlayFabMultiplayer.OnLobbyMemberAdded -= OnMemberAdded;
		// PlayFabMultiplayer.OnLobbyMemberRemoved -= OnMemberRemoved;
	}

	public void UpdateMatch(Match match)
	{
		Debug.Log("OnUpdateMatch");
		this.match = match;
		RefreshUI();
	}

	// public void PlayerLeft(uint playerNetId){
	// 	match.players.Remove(match.players.Find(p => p.netId == playerNetId));
	// 	RefreshPlayerList();
	// }

	// private void OnMemberAdded(Lobby lobby, PFEntityKey member)
	// {
	// 	Debug.Log($"LobbyMemberAdded {member.Id}");
	// 	RefreshPlayerList();
	// }

	// private void OnMemberRemoved(Lobby lobby, PFEntityKey member, LobbyMemberRemovedReason reason)
	// {
	// 	RefreshPlayerList();
	// 	Debug.Log($"LobbyMemberRemoved {member.Id} | {reason}");
	// }

	void RefreshUI(){
		lobbyText.text = $"{(match.isPublic ? "":"Private ")}Lobby {match.matchId}";
		startGameBtn.gameObject.SetActive(ExtNetworkRoomPlayer.localPlayer.netId == match.lobbyOwnerNetId);
		RefreshPlayerList();
	}

	void RefreshPlayerList(){
		Debug.Log("RefreshPlayerList");
		for (int i=playerListPanel.childCount-1; i>=0; i--){
			Destroy(playerListPanel.GetChild(i).gameObject);
		}
		foreach (var player in match.players){
			var item = Instantiate(playerListItemPrefab, playerListPanel).GetComponent<PlayerListItem>();
			item.SetPlayer(player, match.lobbyOwnerNetId);
		}
	}

	public void StartGame(){
		// var updateData = new LobbyDataUpdate {
		// 	MembershipLock = LobbyMembershipLock.Locked,
		// };
		// lobby.PostUpdate(
		// 	ExtNetworkRoomPlayer.localPlayer.playerEntityKey,
		// 	updateData
		// );
		NetworkClient.Send(new BeginGameMessage {lobbyId = match.matchId});
	}

	public void LeaveLobby(){
		StartCoroutine(TempDisableLeaveBtn());
		NetworkClient.Send(new RemovePlayerFromMatchMessage {
			networkPlayer = ExtNetworkRoomPlayer.localPlayer,
			matchId = match.matchId,
		});
		// lobby.Leave(ExtNetworkRoomPlayer.localPlayer.playerEntityKey);
	}

	IEnumerator TempDisableLeaveBtn(){
		leaveLobbyBtn.interactable = false;
		yield return new WaitForSeconds(4f);
		leaveLobbyBtn.interactable = true;
	}
}
