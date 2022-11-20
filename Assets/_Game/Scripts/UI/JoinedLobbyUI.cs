using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;

public class JoinedLobbyUI : MonoBehaviour {
	public LobbyWrapper lobby = new LobbyWrapper();

	[SerializeField] TMP_Text lobbyText;
	[SerializeField] TMP_Text levelNameText;
	[SerializeField] Transform playerListPanel;
	[SerializeField] Button leaveLobbyBtn;
	[SerializeField] Button startGameBtn;
	[SerializeField] GameObject simpleLobbyPanel;
	public GameObject playerListItemPrefab;

	ExtNetworkRoomManager networkManager;
	public Dictionary<uint, bool> playerReadyStates = new Dictionary<uint, bool>();
	void OnEnable(){
		leaveLobbyBtn.interactable = true;
		if (networkManager == null){
			networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		}
		var canvasGroup = GetComponent<CanvasGroup>();

		// If CurrentlyJoinedLobby is not null, that means we're using a PlayFab lobby and can use the fancy UI.
		// Otherwise, show the default Mirror UI plus the level select.
		if (LobbyUtility.CurrentlyJoinedLobby != null){
			simpleLobbyPanel.SetActive(false);
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			networkManager.showRoomGUI = false;
			PlayFabMultiplayer.OnLobbyMemberAdded += OnMemberAdded;
			PlayFabMultiplayer.OnLobbyMemberRemoved += OnMemberRemoved;
			LobbyUtility.OnLobbyDisconnected += OnLobbyDisconnected;
			networkManager.OnAllPlayersReadyStateChanged += OnChangeLobbyReady;
		} else {
			simpleLobbyPanel.SetActive(true);
			simpleLobbyPanel.GetComponentInChildren<LevelSelect>().UseSelectedLevel();
			canvasGroup.alpha = 0;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			networkManager.showRoomGUI = true;
		}
	}

	void OnDisable(){
		PlayFabMultiplayer.OnLobbyMemberAdded -= OnMemberAdded;
		PlayFabMultiplayer.OnLobbyMemberRemoved -= OnMemberRemoved;
		LobbyUtility.OnLobbyDisconnected -= OnLobbyDisconnected;
		networkManager.OnAllPlayersReadyStateChanged -= OnChangeLobbyReady;
		ClearUI();
	}

	void OnChangeLobbyReady(bool ready){
		Debug.Log("OnChangeLobbyReady " + ready);
		startGameBtn.gameObject.SetActive(ready && PlayerEntity.LocalPlayer != null && PlayerEntity.LocalPlayer.entityKey.Id == lobby.lobbyOwnerId);
	}

	public void LoadLobby(string lobbyId){
		Debug.Log($"LoadLobby {lobbyId}");
		// Need to at least have the lobby id in case there's an error here. Otherwise, the "Leave" button won't work.
		if (string.IsNullOrEmpty(lobby.id)){
			lobby.id = lobbyId;
		}
		LobbyUtility.GetLobby(
			lobbyId,
			l => {
				// ExtDebug.LogJson(l);
				lobby = new LobbyWrapper(l);
				RefreshUI();
			},
			OnError
		);
		StopAllCoroutines();
		StartCoroutine(LobbyCheck());
	}

	private void OnError(string error)
	{
		Debug.LogError(error);
		// mainLobbiesUI.DisplayMessage(error, "red");
	}

	private void OnMemberAdded(Lobby lobby, PFEntityKey member)
	{
		Debug.Log($"LobbyMemberAdded {member.Id}");
	}

	private void OnMemberRemoved(Lobby lobby, PFEntityKey member, LobbyMemberRemovedReason reason)
	{
		if (member.Id == PlayerEntity.LocalPlayer.entityKey.Id){
			LeftLobby(lobby.Id);
		} else {
			Debug.Log($"LobbyMemberRemoved {member.Id} | {reason}");
			RemovePlayerFromList(member);
			if (lobby.TryGetOwner(out var owner)){
				Debug.Log("New Lobby owner: " + owner.Id);
				RefreshLobbyOwner(owner.Id);
			}
		}
	}

	void RefreshLobbyOwner(string entityId){
		lobby.lobbyOwnerId = entityId;
		foreach (var item in playerListPanel.GetComponentsInChildren<PlayerListItem>()){
			item.RefreshLobbyOwner();
		}
	}

	void RefreshUI(){
		lobbyText.text = $"{(lobby.isPublic ? "":"Private ")}Lobby {lobby.LobbyName}";
		levelNameText.text = $"Level: {lobby.levelName}";
		startGameBtn.gameObject.SetActive(networkManager.allPlayersReady && PlayerEntity.LocalPlayer.entityKey.Id == lobby.lobbyOwnerId);
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
		foreach (var p in networkManager.roomSlots.Select(p => p as ExtNetworkRoomPlayer)){
			AddPlayerToList(p);
		}
	}
	void AddPlayerToList(ExtNetworkRoomPlayer player){
		var item = Instantiate(playerListItemPrefab, playerListPanel).GetComponent<PlayerListItem>();
		item.lobbyUI = this;
		item.SetPlayer(player);
	}

	void RemovePlayerFromList(PFEntityKey member){
		var players = playerListPanel.GetComponentsInChildren<PlayerListItem>();
		for (int i=0; i<players.Length; i++){
			if(players[i].networkRoomPlayer.playerEntity.entityKey.Id == member.Id){
				Destroy(players[i].gameObject);
				return;
			}
		}
	}

	public void StartGame(){
		LobbyUtility.UpdateLobby(lobby._lobby, PlayFab.MultiplayerModels.MembershipLock.Locked, () => NetworkingMessages.SendBeginGameMessage());
	}

	public void LeaveLobby(){
		StartCoroutine(TempDisableLeaveBtn());
		Debug.Log($"lobby.id {lobby.id} | PlayerEntity.LocalPlayer {PlayerEntity.LocalPlayer}");
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
		LobbyUtility.CurrentlyJoinedLobby = null;
		if (NetworkClient.isHostClient){
			networkManager.StopHost();
		} else {
			networkManager.StopClient();
		}
	}
	#endregion


	// TODO: I kinda hate this. Shouldn't I be able to fire an event handler to do this?
	IEnumerator LobbyCheck(){
		var delay = new WaitForSeconds(2f);
		while (true){
			yield return delay;
			if (playerListPanel.childCount != networkManager.roomSlots.Count){
				RefreshPlayerList();
			}
		}
	}

	public void CheckReady(){
		ExtDebug.LogJson($"Check Ready: ", playerReadyStates);
		startGameBtn.gameObject.SetActive(lobby.lobbyOwnerId == PlayerEntity.LocalPlayer.entityKey.Id && !playerReadyStates.ContainsValue(false));
	}
}
