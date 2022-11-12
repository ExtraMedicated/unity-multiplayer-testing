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
	public PlayerEntity Player { get; private set; }
	ExtNetworkRoomManager networkRoomManager;
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

	void KickMe(){
		if (!isOwner && PlayerEntity.LocalPlayer.entityKey.Id == lobbyUI.lobby.lobbyOwnerId){
			Debug.Log("TODO: Add confirmation prompt.");
			LobbyUtility.RemoveMemberFromLobby(lobbyUI.lobby.id, Player.entityKey, e => Debug.LogError(e));
		}
	}

	public void ToggleReadyStatus(){
		isReady = !isReady;
		NetworkClient.localPlayer.GetComponent<ExtNetworkRoomPlayer>().CmdChangeReadyState(isReady);

		// Server gets this message and broadcasts it to all clients to update the UI.
		NetworkClient.Send(new ChangeReadyStateMessage {
			entityId = Player.entityKey.Id,
			ready = isReady,
		});
	}

	public void SetReady(bool ready){
		readyButtonText.SetText(
			ready ? "Ready!" : "Not Ready",
			ready ? Color.green : Color.red
		);
	}

	public void SetPlayer(PlayerEntity player){
		Debug.Log(" ---------------------------- SetPlayer: " + string.Join(',', networkRoomManager.roomSlots.Select(p => p.name)));
		Debug.Log(networkRoomManager.roomSlots.Count);
		this.Player = player;
		playerName.text = player.name;
		isOwner = player.entityKey.Id == lobbyUI.lobby.lobbyOwnerId;
		ownerBadge.gameObject.SetActive(isOwner);
		enableKickButton = !isOwner && PlayerEntity.LocalPlayer.entityKey.Id == lobbyUI.lobby.lobbyOwnerId;
		readyButton.interactable = player.entityKey.Id == PlayerEntity.LocalPlayer.entityKey.Id;
		SetReady(networkRoomManager.roomSlots.Find(p => (p as ExtNetworkRoomPlayer).entityId == player.entityKey.Id)?.readyToBegin ?? false);
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
}
