using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;
using TMPro;
using System;
using PlayFab.ClientModels;
using PlayFab;
using PlayFab.DataModels;
using Newtonsoft.Json;

public class MultiplayerMenu : MonoBehaviour
{
	const string LOCAL_PLAYER_NAME_KEY = "local_multiplayer_name";
	const string ONLINE_PLAYER_NAME_KEY = "online_multiplayer_name";
	// [SerializeField] GameObject pfEventProcessorPrefab;
	[SerializeField] LoginUtility loginUtility;
	[SerializeField] GameObject lobbyPannel;
	[SerializeField] InputFieldWrapper playerNameInput;
	MainMenu mainMenu;
	ExtNetworkRoomManager networkManager;

	bool isAttemptingAuthentication;

	void Awake(){
		mainMenu = FindObjectOfType<MainMenu>();
	}

	void OnEnable(){
		loginUtility.OnError += OnError;
		OnScreenMessage.SetText(string.Empty);
		playerNameInput.inputEnabled = true;
		mainMenu.gameObject.SetActive(false);

		if (networkManager == null){
			networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		}

		if (PlayerEntity.LocalPlayer?.HasSession ?? false){
			ViewLobbies();
		} else {
			Invoke(nameof(InitName), 0.1f);
		}
	}

	void OnDisable(){
		loginUtility.OnError -= OnError;
		mainMenu.gameObject.SetActive(true);
	}

	void InitName(){
		if (Config.Instance.HasDefaultPlayerName){
			playerNameInput.text = Config.Instance.GetDefaultPlayerName();
		}
		if (string.IsNullOrWhiteSpace(playerNameInput.text)){
			Debug.Log(ONLINE_PLAYER_NAME_KEY + ": " +PlayerPrefs.GetString(ONLINE_PLAYER_NAME_KEY));
			playerNameInput.text = PlayerPrefs.GetString(ONLINE_PLAYER_NAME_KEY);
		}
	}

	bool ValidateName(){
		if (string.IsNullOrWhiteSpace(playerNameInput.text)){
			OnScreenMessage.SetText("Please enter your name", "red");
			return false;
		}
		return true;
	}

	public void OnClickOnline(){
		if (!isAttemptingAuthentication){
			OnScreenMessage.SetText(string.Empty);
			if (!ValidateName()) return;
			AttemptPlayfabLogin();
		}
	}

	public void OnClickLocal(){
		if (!isAttemptingAuthentication){
			OnScreenMessage.SetText(string.Empty);
			if (!ValidateName()) return;
			OnScreenMessage.SetText("Not implemented yet", "red");
		}
	}

	void AttemptPlayfabLogin(){
		OnScreenMessage.SetText("Logging in...");
		// Make sure the authenticator is set. (Although it isn't actually used until connecting to a lobby.)
		if (networkManager.authenticator == null){
			networkManager.authenticator = networkManager.GetComponent<NewNetworkAuthenticator>();
		}
		isAttemptingAuthentication = true;
		playerNameInput.inputEnabled = false;
		var playerName = !string.IsNullOrWhiteSpace(playerNameInput.text) ? playerNameInput.text : "Nameless nobody";
		if (!string.IsNullOrWhiteSpace(playerName)){
			Debug.Log($"set player name: {playerName}");
			PlayerPrefs.SetString(ONLINE_PLAYER_NAME_KEY, playerName);
		}
		loginUtility.AttemptPlayfabLogin(playerName, LoginUtility.AuthenticationMode.CustomID, OnLoginResponse, ViewLobbies);
	}

	private void OnError(string error)
	{
		OnScreenMessage.SetText(error, "red");
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
		// // Disconnect after failed login
		// NetworkClient.Disconnect();
	}

	private void OnLoginResponse()
	{
		OnScreenMessage.SetText("Logged in");
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
	}

	void ViewLobbies(){
		lobbyPannel.SetActive(true);
	}
}
