using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using System.Linq;

public class PlayerListItem : MonoBehaviour {
	[SerializeField] TMP_Text playerName;
	[SerializeField] TMP_Text ownerBadge;
	[SerializeField] Button kickButton;
	[SerializeField] Button readyButton;
	// public PlayerEntity Player { get; private set; }
	ExtNetworkRoomManager networkRoomManager;
	public ExtNetworkRoomPlayer networkRoomPlayer;
	UIText readyButtonText;
	public JoinedLobbyUI lobbyUI;

	// TODO: I think this should be moved to a container object.
	public bool enableKickButton;
	bool isOwner;
	bool isReady;

	void Start(){
		kickButton.onClick.AddListener(KickMe);
	}

	void OnEnable(){
		if (readyButtonText == null){
			readyButtonText = readyButton.GetComponent<UIText>();
		}
		if (networkRoomManager == null){
			networkRoomManager = FindObjectOfType<ExtNetworkRoomManager>();
		}
		#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE)
			kickButton.gameObject.SetActive(enableKickButtons);
		#else
			kickButton.gameObject.SetActive(false);
		#endif
	}

	void OnDisable(){
		if (lobbyUI.playerReadyStates.ContainsKey(networkRoomPlayer.netId)){
			lobbyUI.playerReadyStates.Remove(networkRoomPlayer.netId);
		}
	}

	void KickMe(){
		if (!isOwner && PlayerEntity.LocalPlayer.entityKey.Id == lobbyUI.lobby.lobbyOwnerId){
			Debug.Log("TODO: Add confirmation prompt.");
			LobbyUtility.RemoveMemberFromLobby(lobbyUI.lobby.id, networkRoomPlayer.playerEntity.entityKey, e => Debug.LogError(e));
		}
	}

	public void ToggleReadyStatus(){
		networkRoomPlayer.CmdChangeReadyState(!isReady);
	}

	public void SetReady(bool ready){
		isReady = ready;
		readyButtonText.SetText(
			ready ? "Ready!" : "Not Ready",
			ready ? Color.green : Color.red
		);
		if (!lobbyUI.playerReadyStates.ContainsKey(networkRoomPlayer.netId)){
			lobbyUI.playerReadyStates.Add(networkRoomPlayer.netId, isReady);
		} else {
			lobbyUI.playerReadyStates[networkRoomPlayer.netId] = isReady;
		}
		lobbyUI.CheckReady();
	}

	public void RefreshLobbyOwner(){
		isOwner = networkRoomPlayer.playerEntity.entityKey.Id == lobbyUI.lobby.lobbyOwnerId;
		LobbyUtility.CurrentlyJoinedLobby = new BasicLobbyInfo {
			lobbyId = lobbyUI.lobby.id,
			lobbyOwnerId = lobbyUI.lobby.lobbyOwnerId,
		};
		ownerBadge.gameObject.SetActive(isOwner);
		enableKickButton = !isOwner && PlayerEntity.LocalPlayer.entityKey.Id == lobbyUI.lobby.lobbyOwnerId;
		readyButton.interactable = networkRoomPlayer.playerEntity.entityKey.Id == PlayerEntity.LocalPlayer.entityKey.Id;
	}

	public void SetPlayer(ExtNetworkRoomPlayer player){
		Debug.Log(" ---------------------------- SetPlayer: " + string.Join(',', networkRoomManager.roomSlots.Select(p => p.name)));
		Debug.Log(networkRoomManager.roomSlots.Count);
		networkRoomPlayer = player;
		Debug.Log("networkRoomPlayer " + networkRoomPlayer);
		playerName.text = player.playerEntity.name;
		RefreshLobbyOwner();
		SetReady(networkRoomPlayer.readyToBegin);
	}

	public void OnPointerEnter(){
		#if !(UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE)
		kickButton.gameObject.SetActive(enableKickButton && !isOwner);
		#endif
	}

	public void OnPointerExit(){
		#if !(UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE)
		kickButton.gameObject.SetActive(false);
		#endif
	}

	// TODO: I kinda hate this. Shouldn't I be able to fire an event handler to do this?
	public void Update(){
		if (networkRoomPlayer != null && networkRoomPlayer.readyToBegin != isReady){
			SetReady(networkRoomPlayer.readyToBegin);
		}
	}
}
