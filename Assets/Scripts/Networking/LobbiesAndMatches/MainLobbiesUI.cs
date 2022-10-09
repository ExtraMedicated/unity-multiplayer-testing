using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class MainLobbiesUI : MonoBehaviour {
	public List<GameObject> enableUIElements;
	public Button joinButton;
	[SerializeField] TMP_InputField joinMatchIdField;
	string joinMatchId;

	void OnEnable() {
		foreach(var obj in enableUIElements){
			obj.SetActive(true);
		}
		joinMatchIdField.text = string.Empty;
		joinMatchId = string.Empty;
		joinButton.interactable = false;
		// StartCoroutine(DelayedSearch());
	}
	// IEnumerator DelayedSearch(){
	// 	// Delay refresh to allow time for changes to propagate.
	// 	yield return new WaitForSeconds(2f);
	// 	LobbyUtility.instance.FindLobbies();
	// }

	public void OnMatchIDChanged(string text){
		joinMatchId = Regex.Replace(text, @"[^\u0020-\u007E]", string.Empty).ToUpperInvariant();
		joinButton.interactable = !string.IsNullOrWhiteSpace(joinMatchId);
	}

	public void JoinMatchById(){
		if (!string.IsNullOrWhiteSpace(joinMatchId)){
			LobbyUtility.instance.JoinLobby(joinMatchId.ToUpperInvariant());
		}
	}

	public void ReturnToMenu(){
		Debug.Log($"ReturnToMenu {ExtNetworkRoomPlayer.localPlayer}");
		ExtNetworkRoomPlayer.localPlayer?.QuitGame();
	}
}
