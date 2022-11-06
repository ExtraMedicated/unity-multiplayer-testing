using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerListItem : MonoBehaviour {
	[SerializeField] TMP_Text playerName;
	[SerializeField] TMP_Text ownerBadge;
	[SerializeField] Button kickButton;
	PlayerEntity player;

	// TODO: I think this should be moved to a container object.
	public bool enableKickButton;
	bool isOwner;

	void Start(){
		kickButton.onClick.AddListener(KickMe);
	}

	void OnEnable(){
		#if (UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE)
			kickButton.gameObject.SetActive(enableKickButtons);
		#else
			kickButton.gameObject.SetActive(false);
		#endif
	}

	void KickMe(){
		// NetworkClient.Send(new RemovePlayerFromMatchMessage{
		// 	networkPlayer = player,
		// 	matchId = player.matchId,
		// });
	}

	public void SetPlayer(PlayerEntity player, string lobbyOwnerId){
		this.player = player;
		playerName.text = player.name;
		isOwner = player.entityKey.Id == lobbyOwnerId;
		ownerBadge.gameObject.SetActive(isOwner);
		enableKickButton = !isOwner && PlayerEntity.LocalPlayer.entityKey.Id == lobbyOwnerId;
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
