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
	[SerializeField] UIText statusText;
	MainMenu mm;
	MainMenu mainMenu {
		get {
			if (mm == null){
				mm = FindObjectOfType<MainMenu>();
			}
			return mm;
		}
	}

	bool isAttemptingAuthentication;
	// void Start(){
	// 	// Need to add the PlayfabMultiplayerEventProcessor if it doesn't exist. (Not 100% sure if it's needed on the client side.)
	// 	if (FindObjectOfType<PlayfabMultiplayerEventProcessor>() == null){
	// 		Instantiate(pfEventProcessorPrefab);
	// 	}
	// }

	void OnEnable(){
		loginUtility.OnError += OnError;
		statusText.text = string.Empty;
		playerNameInput.inputEnabled = true;
		mainMenu.gameObject.SetActive(false);

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
			DisplayMessage("Please enter your name", "red");
			return false;
		}
		return true;
	}

	public void OnClickOnline(){
		if (!isAttemptingAuthentication){
			statusText.text = string.Empty;
			if (!ValidateName()) return;
			AttemptPlayfabLogin();
		}
	}

	public void OnClickLocal(){
		if (!isAttemptingAuthentication){
			statusText.text = string.Empty;
			if (!ValidateName()) return;
			DisplayMessage("Not implemented yet", "red");
		}
	}

	void DisplayMessage(string text, string color = ""){
		statusText.text = !string.IsNullOrEmpty(color) ? $"<color={color}>{text}</color>" : text;
	}

	void AttemptPlayfabLogin(){
		DisplayMessage("Logging in...");
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
		DisplayMessage(error, "red");
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
		// // Disconnect after failed login
		// NetworkClient.Disconnect();
	}

	private void OnLoginResponse()
	{
		DisplayMessage("Logged in");
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
	}

	void ViewLobbies(){
		lobbyPannel.SetActive(true);
	}
}
