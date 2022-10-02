using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayFabCheck : MonoBehaviour {
	public List<GameObject> playfabOnlyObjects;
	ExtNetworkRoomManager networkRoomManager;

	void Awake(){
		networkRoomManager = FindObjectOfType<ExtNetworkRoomManager>();
	}

	void Start(){
		var authInfo = NetworkClient.connection?.authenticationData as AuthenticationInfo;
		// If user is not logged in through PlayFab, show the default lobby UI.
		// Debug.Log($"----------- Entity ID: {authInfo?.EntityId}, Session: {authInfo?.SessionTicket}");
		if (authInfo != null && !string.IsNullOrEmpty(authInfo.SessionTicket)){
			Debug.Log("Using PlayFab");
			networkRoomManager.showRoomGUI = false;
			playfabOnlyObjects.ForEach(obj => obj.SetActive(true));
		} else {
			Debug.Log("Not Using PlayFab");
			networkRoomManager.showRoomGUI = true;
			playfabOnlyObjects.ForEach(obj => obj.SetActive(false));
		}
	}

}
