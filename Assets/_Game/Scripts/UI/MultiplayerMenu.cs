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
	// [SerializeField] GameObject pfEventProcessorPrefab;
	[SerializeField] LoginUtility loginUtility;
	[SerializeField] GameObject lobbyPannel;
	[SerializeField] InputFieldWrapper playerNameInput;
	[SerializeField] InputFieldWrapper localIPAddressField;
	[SerializeField] InputFieldWrapper localPortNumField;
	[SerializeField] GameObject localNetMultiplayerPanel;
	MainMenu mainMenu;
	ExtNetworkRoomManager networkManager;
	TransportWrapper transportWrapper;

	bool isAttemptingAuthentication;

	void Awake(){
		mainMenu = FindObjectOfType<MainMenu>(true);
		networkManager = FindObjectOfType<ExtNetworkRoomManager>(true);
		transportWrapper = networkManager.GetComponent<TransportWrapper>();
	}

	void OnEnable(){
		loginUtility.OnError += OnError;
		OnScreenMessage.SetText(string.Empty);
		playerNameInput.inputEnabled = true;
		mainMenu.gameObject.SetActive(false);
		localNetMultiplayerPanel.SetActive(false);
		Invoke(nameof(InitName), 0.1f);

		if (PlayerEntity.LocalPlayer?.HasSession ?? false){
			ViewLobbies();
		}
	}

	void OnDisable(){
		loginUtility.OnError -= OnError;
		mainMenu.gameObject.SetActive(true);
		localNetMultiplayerPanel.SetActive(false);
	}

	void InitName(){
		if (Config.Instance.HasDefaultPlayerName){
			playerNameInput.text = Config.Instance.GetDefaultPlayerName();
		}
		if (string.IsNullOrWhiteSpace(playerNameInput.text)){
			Debug.Log(PlayerInfo.ONLINE_PLAYER_NAME_KEY + ": " +PlayerPrefs.GetString(PlayerInfo.ONLINE_PLAYER_NAME_KEY));
			playerNameInput.text = PlayerPrefs.GetString(PlayerInfo.ONLINE_PLAYER_NAME_KEY);
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
		if (localNetMultiplayerPanel.activeInHierarchy){
			localNetMultiplayerPanel.SetActive(false);
		} else {
			loginUtility.authenticationMode = LoginUtility.AuthenticationMode.Local;
			if (!isAttemptingAuthentication){
				OnScreenMessage.SetText(string.Empty);
				if (!ValidateName()) return;
				localNetMultiplayerPanel.SetActive(true);
			}
		}
	}

	void AttemptPlayfabLogin(){
		OnScreenMessage.SetText("Logging in...");
		isAttemptingAuthentication = true;
		playerNameInput.inputEnabled = false;
		var playerName = !string.IsNullOrWhiteSpace(playerNameInput.text) ? playerNameInput.text : "Nameless nobody";
		if (!string.IsNullOrWhiteSpace(playerName)){
			PlayerPrefs.SetString(PlayerInfo.ONLINE_PLAYER_NAME_KEY, playerName);
		}
		loginUtility.AttemptPlayfabLogin(playerName, OnLoginResponse, ViewLobbies);
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
