using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using PlayFab;
using PlayFab.DataModels;
using UnityEngine.SceneManagement;
using Mirror;

public class JoinedLobbyUI : MonoBehaviour {
	public LobbyWrapper lobby;

	[SerializeField] TMP_Text lobbyText;
	// [SerializeField] TMP_Text levelNameText;
	[SerializeField] Transform playerListPanel;
	[SerializeField] Button leaveLobbyBtn;
	[SerializeField] Button startGameBtn;
	public GameObject playerListItemPrefab;
	// public LobbyUI mainLobbiesUI;

	void OnEnable(){
		leaveLobbyBtn.interactable = true;
		PlayFabMultiplayer.OnLobbyMemberAdded += OnMemberAdded;
		PlayFabMultiplayer.OnLobbyMemberRemoved += OnMemberRemoved;
		LobbyUtility.OnLobbyDisconnected += OnLobbyDisconnected;
		// StartCoroutine(DelayedOnEnable());
		// MatchManager2.instance.OnUpdateMatch += UpdateMatch;
	}

	void OnDisable(){
		// if (MatchManager2.instance != null)
		// MatchManager2.instance.OnUpdateMatch -= UpdateMatch;
		PlayFabMultiplayer.OnLobbyMemberAdded -= OnMemberAdded;
		PlayFabMultiplayer.OnLobbyMemberRemoved -= OnMemberRemoved;
		LobbyUtility.OnLobbyDisconnected -= OnLobbyDisconnected;
		ClearUI();
	}

	public void LoadLobby(string lobbyId){
		LobbyUtility.GetLobby(
			lobbyId,
			l => {
				ExtDebug.LogJson(l);
				lobby = new LobbyWrapper(l);
				RefreshUI();
			},
			OnError
		);
	}

	private void OnError(string error)
	{
		Debug.LogError(error);
		// mainLobbiesUI.DisplayMessage(error, "red");
	}

	// IEnumerator DelayedOnEnable(){
	// 	yield return new WaitForEndOfFrame();
	// 	MatchManager2.instance.OnUpdateMatch += OnUpdateMatch;
	// }

	// public void UpdateMatch(Lobby lobby)
	// {
	// 	Debug.Log("OnUpdateMatch");
	// 	this.lobby = lobby;
	// 	RefreshUI();
	// }

	// public void PlayerLeft(uint playerNetId){
	// 	match.players.Remove(match.players.Find(p => p.netId == playerNetId));
	// 	RefreshPlayerList();
	// }

	private void OnMemberAdded(Lobby lobby, PFEntityKey member)
	{
		Debug.Log($"LobbyMemberAdded {member.Id}");
		// ExtDebug.LogJson(lobby);

		// Apparently I need to reload the lobby to update the list of members.
		LoadLobby(lobby.Id);

		// RefreshPlayerList();
	}

	private void OnMemberRemoved(Lobby lobby, PFEntityKey member, LobbyMemberRemovedReason reason)
	{
		Debug.Log($"LobbyMemberRemoved {member.Id} | {reason}");
		// Apparently I need to reload the lobby to update the list of members.
		LoadLobby(lobby.Id);
		// RefreshPlayerList();
	}

	void RefreshUI(){
		lobbyText.text = $"{(lobby.isPublic ? "":"Private ")}Lobby {lobby.LobbyName}";
		// levelNameText.text = $"Level: {lobby.levelName}";
		startGameBtn.gameObject.SetActive(PlayerEntity.LocalPlayer.entityKey.Id == lobby.lobbyOwnerId);
		RefreshPlayerList();
	}
	void ClearUI(){
		lobbyText.text = string.Empty;
		for (int i=playerListPanel.childCount-1; i>=0; i--){
			Destroy(playerListPanel.GetChild(i).gameObject);
		}
	}

	void RefreshPlayerList(){
		Debug.Log("RefreshPlayerList");
		for (int i=playerListPanel.childCount-1; i>=0; i--){
			Destroy(playerListPanel.GetChild(i).gameObject);
		}
		foreach (var member in lobby._lobby.Members){
			ExtDebug.LogJson("Player Entity: ", member);
			var item = Instantiate(playerListItemPrefab, playerListPanel).GetComponent<PlayerListItem>();
			item.SetPlayer(new PlayerEntity(member), lobby.lobbyOwnerId);
			// var getRequest = new GetObjectsRequest {Entity = new PlayFab.DataModels.EntityKey { Id = member.Id, Type = member.Type }};
			// //PlayFab.PlayFabClientAPI.GetPlayerProfile();
			// PlayFabDataAPI.GetObjects(getRequest,
			// 	result => {
			// 		var playerInfo = result.Objects["PlayerData"]?.DataObject as PlayerInfo;
			// 		if (playerInfo != null){
			// 			item.SetPlayer(new PlayerEntity(member, playerInfo), lobby.lobbyOwnerId);
			// 		}
			// 	},
			// 	error => {
			// 		Debug.LogError(error);
			// 		item.SetPlayer(new PlayerEntity(member, new PlayerInfo {PlayerName = "(ERROR)"}), lobby.lobbyOwnerId);
			// 	}
			// );
			// item.SetPlayer(new PlayerEntity(member), lobby.lobbyOwnerId);
		}
	}

	public void StartGame(){
		var updateData = new LobbyDataUpdate {
			MembershipLock = LobbyMembershipLock.Locked,
		};
		LobbyUtility.UpdateLobby(lobby._lobby, PlayFab.MultiplayerModels.MembershipLock.Locked);
		// NetworkClient.Send(new BeginGameMessage {matchId = match.matchId});

	}

	public void LeaveLobby(){
		StartCoroutine(TempDisableLeaveBtn());
		// NetworkClient.Send(new RemovePlayerFromMatchMessage {
		// 	networkPlayer = ExtNetworkRoomPlayer.localPlayer,
		// 	matchId = match.matchId,
		// });
		LobbyUtility.LeaveLobby(lobby.id, PlayerEntity.LocalPlayer.entityKey, OnError);
	}

	#region Disconnect
	private void OnLobbyDisconnected(Lobby lobby)
	{
		// Disconnected from lobby
		Debug.Log("Disconnected from lobby!");
		LeftLobby(lobby.Id);
	}

	IEnumerator TempDisableLeaveBtn(){
		leaveLobbyBtn.interactable = false;
		yield return new WaitForSeconds(4f);
		leaveLobbyBtn.interactable = true;
	}

	public void LeftLobby(string lobbyId){
		Debug.Log($"{PlayerEntity.LocalPlayer?.name} LeftLobby {lobbyId}");
		// DisplayMessage($"{PlayerEntity.LocalPlayer?.name} LeftLobby {lobbyId}");
		// NetworkClient.Send(new RemovePlayerFromMatchMessage {lobbyId = lobby.Id});
		// MatchManager.instance.RemovePlayerFromMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);

		// Delay to allow time for changes to propagate.
		// StartCoroutine(DelayCloseLobby());
		// mainLobbiesUI.SetActive(true);

		PlayerEntity.LocalPlayer.lobbyId = string.Empty;
		NetworkClient.Disconnect();
	}

	// IEnumerator DelayCloseLobby(){
	// 	yield return new WaitForSeconds(CLOSE_LOBBY_DELAY);
	// 	joinedLobbyUI.gameObject.SetActive(false);
	// 	joinedLobbyUI.lobby = null;
	// 	FindLobbies();
	// }
	#endregion
}
