using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class JoinedLobbyUI : MonoBehaviour {

	public Lobby lobby;

	[SerializeField] TMP_Text lobbyText;
	[SerializeField] Transform playerListPanel;
	[SerializeField] Button leaveLobbyBtn;
	public GameObject playerListItemPrefab;

	void OnEnable(){
		leaveLobbyBtn.interactable = true;
		if (lobby != null){
			lobbyText.text = $"Lobby {lobby.Id}";
			RefreshPlayerList();
		}
		PlayFabMultiplayer.OnLobbyMemberAdded += OnMemberAdded;
		PlayFabMultiplayer.OnLobbyMemberRemoved += OnMemberRemoved;
	}

	void OnDisable(){
		PlayFabMultiplayer.OnLobbyMemberAdded -= OnMemberAdded;
		PlayFabMultiplayer.OnLobbyMemberRemoved -= OnMemberRemoved;
	}

	private void OnMemberAdded(Lobby lobby, PFEntityKey member)
	{
		Debug.Log($"LobbyMemberAdded {member.Id}");
		RefreshPlayerList();
	}

	private void OnMemberRemoved(Lobby lobby, PFEntityKey member, LobbyMemberRemovedReason reason)
	{
		RefreshPlayerList();
		Debug.Log($"LobbyMemberRemoved {member.Id} | {reason}");
	}

	void RefreshPlayerList(){
		for (int i=playerListPanel.childCount-1; i>=0; i--){
			Destroy(playerListPanel.GetChild(i).gameObject);
		}
		foreach (var player in lobby.GetMembers()){
			var item = Instantiate(playerListItemPrefab, playerListPanel);
			item.GetComponentInChildren<TMP_Text>().text = player.Id;
		}
	}

	public void StartGame(){
		NetworkClient.Send(new BeginGameMessage {lobbyId = lobby.Id});
	}

	public void LeaveLobby(){
		StartCoroutine(TempDisableLeaveBtn());
		lobby.Leave(ExtNetworkRoomPlayer.localPlayer.playerEntityKey);
	}

	IEnumerator TempDisableLeaveBtn(){
		leaveLobbyBtn.interactable = false;
		yield return new WaitForSeconds(4f);
		leaveLobbyBtn.interactable = true;
	}
}
