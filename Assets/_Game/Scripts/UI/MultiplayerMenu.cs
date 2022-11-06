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
	[SerializeField] GameObject lobbyPannel;
	[SerializeField] InputFieldWrapper playerNameInput;
	[SerializeField] UIText statusText;
	[SerializeField] bool useRandomFakeDeviceId;
	[SerializeField] bool appendNameToDeviceID;
	MainMenu mm;
	MainMenu mainMenu {
		get {
			if (mm == null){
				mm = FindObjectOfType<MainMenu>();
			}
			return mm;
		}
	}
	enum AuthenticationMode {
		Local,
		CustomID,
		PlayFabLogin,
		PlayFabRegister,
	}
	AuthenticationMode authenticationMode;


	bool isAttemptingAuthentication;
	// void Start(){
	// 	// Need to add the PlayfabMultiplayerEventProcessor if it doesn't exist. (Not 100% sure if it's needed on the client side.)
	// 	if (FindObjectOfType<PlayfabMultiplayerEventProcessor>() == null){
	// 		Instantiate(pfEventProcessorPrefab);
	// 	}
	// }

	void OnEnable(){
		statusText.text = string.Empty;
		playerNameInput.inputEnabled = true;
		Invoke(nameof(InitName), 0.1f);
		mainMenu.gameObject.SetActive(false);
	}

	void InitName(){
		if (!string.IsNullOrEmpty(Config.Instance.defaultPlayerName)){
			playerNameInput.text = Config.Instance.defaultPlayerName;
		}
		if (string.IsNullOrWhiteSpace(playerNameInput.text)){
			Debug.Log(ONLINE_PLAYER_NAME_KEY + ": " +PlayerPrefs.GetString(ONLINE_PLAYER_NAME_KEY));
			playerNameInput.text = PlayerPrefs.GetString(ONLINE_PLAYER_NAME_KEY);
		}
	}

	void OnDisable(){
		mainMenu.gameObject.SetActive(true);
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

	string GetDeviceId(){
		if (useRandomFakeDeviceId){
			return Guid.NewGuid().ToString();
		}
		string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;

		// Not all platforms support this, so we use a GUID instead
		if (deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier)
		{
			// Get the value from PlayerPrefs if it exists, new GUID if it doesn't
			deviceUniqueIdentifier = PlayerPrefs.GetString("deviceUniqueIdentifier", Guid.NewGuid().ToString());

			// Store the deviceUniqueIdentifier to PlayerPrefs (in case we just made a new GUID)
			PlayerPrefs.SetString("deviceUniqueIdentifier", deviceUniqueIdentifier);
		}
		return deviceUniqueIdentifier;
	}


	void AttemptPlayfabLogin(){
		DisplayMessage("Logging in...");
		isAttemptingAuthentication = true;
		playerNameInput.inputEnabled = false;
		Debug.Log("AuthenticateClient " + authenticationMode.ToString());
		var playerName = !string.IsNullOrWhiteSpace(playerNameInput.text) ? playerNameInput.text : "Nameless nobody";
		if (!string.IsNullOrWhiteSpace(playerName)){
			Debug.Log($"set player name: {playerName}");
			PlayerPrefs.SetString(ONLINE_PLAYER_NAME_KEY, playerName);
		}
		// Log into playfab
		var request = new LoginWithCustomIDRequest{
			TitleId = PlayFabSettings.TitleId,
			CustomId = appendNameToDeviceID ? GetDeviceId() + playerName : GetDeviceId(),
			CreateAccount = true,
			InfoRequestParameters = new GetPlayerCombinedInfoRequestParams {
				GetPlayerProfile = true,
				GetPlayerStatistics = true,
				GetTitleData = true,
				GetUserData = true,
				ProfileConstraints = new PlayerProfileViewConstraints {
					ShowDisplayName = true,
				}
			}
		};
		PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnPlayFabLoginFailure);
	}

	private void OnPlayFabLoginFailure(PlayFabError error)
	{
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
		OnPlayFabError(error);
	}

	private void OnPlayFabError(PlayFabError error)
	{
		DisplayMessage(error.ErrorMessage, "red");
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
		// // Disconnect after failed login
		// NetworkClient.Disconnect();
	}

	private void OnPlayFabLoginSuccess(LoginResult response)
	{
		DisplayMessage("Logged in");
		Debug.Log(JsonConvert.SerializeObject(response));
		isAttemptingAuthentication = false;
		playerNameInput.inputEnabled = true;
		PlayFabMultiplayer.SetEntityToken(response.AuthenticationContext); // Is this needed?
		var entity = new PFEntityKey(response.AuthenticationContext);
		// var entity = new PlayFab.DataModels.EntityKey { Id = response.EntityToken.Entity.Id, Type = response.EntityToken.Entity.Type };

		var dataList = new List<SetObject>{
			// A free-tier customer may store up to 3 objects on each entity
			new PlayerInfo { PlayerName = playerNameInput.text }.ToSetObject(),
		};

		if (PlayerEntity.LocalPlayer == null || PlayerEntity.LocalPlayer.name != playerNameInput.text){
			//Set PlayerEntity.LocalPlayer
			SetPlayerData(response.SessionTicket, dataList, entity, ViewLobbies);
		} else {
			ViewLobbies();
			// GetPlayerData(entity, (result) => {
			// 	var playerInfo = result.Objects["PlayerData"]?.DataObject as PlayerInfo;
			// 	if (playerInfo != null){
			// 		if (PlayerEntity.LocalPlayer == null){
			// 			PlayerEntity.LocalPlayer = new PlayerEntity(entity, playerInfo);
			// 		} else if (playerInfo.PlayerName != PlayerEntity.LocalPlayer.name){
			// 			// Name changed. Update the player on PF.
			// 			SetPlayerData(dataList, entity, ViewLobbies);
			// 		}
			// 	}
			// 	Debug.Log(JsonConvert.SerializeObject(playerInfo));
			// });
		}

		// // Tell the server that the user logged in.
		// NetworkClient.connection.Send(new AuthRequestMessage {
		// 	username = !string.IsNullOrWhiteSpace(usernameField.text) ? usernameField.text : "Nameless nobody",
		// 	entityId = response.EntityToken.Entity.Id,
		// 	sessionTicket = response.SessionTicket,
		// });
	}

	void SetPlayerData(string sessionTicket, List<SetObject> setObjects, PFEntityKey entity, Action callback){
		Debug.Log(JsonConvert.SerializeObject(setObjects));
		PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {
			DisplayName = playerNameInput.text
		}, OnUpdateNameSuccess, OnPlayFabError);
		PlayFabDataAPI.SetObjects(new SetObjectsRequest {
			Entity = new EntityKey(entity.Id, entity.Type),
			Objects = setObjects,
		}, (setResult) => {
			Debug.Log("setResult");
			Debug.Log(JsonConvert.SerializeObject(setResult));
			var pInfo = setObjects[0].DataObject as PlayerInfo;
			PlayerEntity.LocalPlayer = new PlayerEntity(entity, pInfo, sessionTicket);

			// GetPlayerData(entity, (result) => {
			// 	var objs = result.Objects;
			// 	Debug.Log(JsonConvert.SerializeObject(objs));
			// });
			DisplayMessage($"Player name is {PlayerEntity.LocalPlayer.name}");
			callback();
		}, OnPlayFabError);
	}

	private void OnUpdateNameSuccess(UpdateUserTitleDisplayNameResult result)
	{
		Debug.Log("OnUpdateNameSuccess");
		Debug.Log(JsonConvert.SerializeObject(result));
	}

	void GetPlayerData(PFEntityKey entity, Action<GetObjectsResponse> callback){
		var getRequest = new GetObjectsRequest {Entity = new PlayFab.DataModels.EntityKey { Id = entity.Id, Type = entity.Type }};
		PlayFabDataAPI.GetObjects(getRequest,
			result => callback(result),
			// result => {
			// 	var objs = result.Objects;
			// 	Debug.Log(JsonConvert.SerializeObject(objs));
			// },
			OnPlayFabError
		);
	}

	void ViewLobbies(){
		lobbyPannel.SetActive(true);
	}

	private void PlayFabMultiplayer_OnLobbyFindLobbiesCompleted(
		IList<LobbySearchResult> searchResults,
		PFEntityKey newMember,
		int reason)
	{
		if (LobbyError.SUCCEEDED(reason))
		{
			// Successfully found lobbies
			Debug.Log("Found lobbies");

			// Iterate through lobby search results
			foreach (LobbySearchResult result in searchResults)
			{
				// Examine a search result
				Debug.Log(JsonConvert.SerializeObject(result));
			}
		}
		else
		{
			// Error finding lobbies
			Debug.Log("Error finding lobbies");
		}
	}

}
