using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using PlayFab.Multiplayer;
using UnityEngine;

public class LoginUtility : MonoBehaviour {

	public bool appendNameToDeviceID;
	public bool useRandomFakeDeviceId;
	static LoginUtility instance;

	public Action<string> OnError;

	public enum AuthenticationMode {
		Local,
		CustomID,
		PlayFabLogin,
		PlayFabRegister,
	}
	public AuthenticationMode authenticationMode;
	ExtNetworkRoomManager networkManager;
	void Awake(){
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		// Replace old instance with a fresh new one to prevent strange errors.
		if (instance != null && instance != this){
			Destroy(instance.gameObject);
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public bool _enabled;

	void OnEnable(){
		var networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent += OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent += AuthenticateClient;
			networkAuthenticator.AuthResponseEvent += OnAuthResponse;
			networkAuthenticator.OnClientAuthenticated.AddListener(AuthSuccess);
		}
		_enabled = true;
	}
	void OnDisable(){
		var networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent -= OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent -= AuthenticateClient;
			networkAuthenticator.AuthResponseEvent -= OnAuthResponse;
		}
		_enabled = false;
	}

	void AuthenticateClient(){
		switch (authenticationMode){
			case AuthenticationMode.CustomID:
				if (PlayFabClientAPI.IsClientLoggedIn()){
					// Tell the server that the user logged in.
					NetworkingMessages.SendAuthRequestMessage();
				} else {
					OnAuthError("Not logged in.");
				}
				break;
			case AuthenticationMode.Local:
				NetworkingMessages.SendAuthRequestMessage();
				break;
			default:
				throw new NotImplementedException($"AuthenticationMode {authenticationMode} is not implemented");
		}
	}

	private void OnAuthError(string error)
	{
		OnScreenMessage.SetText(error, "red");
		NetworkClient.Disconnect();
	}

	private void OnAuthResponse(AuthResponseMessage msg)
	{
		// TODO: Is it redundant to do this here?
		NetworkClient.connection.authenticationData = msg.playerEntity;
	}

	void AuthSuccess()
	{
		// Debug.Log("Logged in? " + (PlayFabClientAPI.IsClientLoggedIn() ? "Yes" : "No"));
		Debug.Log("Auth Success!");
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

	public void AttemptPlayfabLogin(string playerName, Action onLoginResult, Action afterLogin){
		authenticationMode = LoginUtility.AuthenticationMode.CustomID;
		Debug.Log("AuthenticateClient " + authenticationMode.ToString());
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
		PlayFabClientAPI.LoginWithCustomID(
			request,
			r => OnPlayFabLoginSuccess(r, playerName, onLoginResult, afterLogin),
			OnPlayFabError);
	}

	public static void Logout(){
		PlayFabClientAPI.ForgetAllCredentials();
		if (NetworkClient.isConnected){
			NetworkClient.connection.authenticationData = null;
		}
		PlayerEntity.LocalPlayer = null;
	}

	private void OnPlayFabError(PlayFabError e){
		ExtDebug.LogJsonError("Login error: ", e.GenerateErrorReport());
		OnError?.Invoke(e.ErrorMessage);
	}

	private void OnPlayFabLoginSuccess(LoginResult response, string playerName, Action onLoginResult, Action afterLogin)
	{
		PlayFabMultiplayer.SetEntityToken(response.AuthenticationContext); // Is this needed?
		onLoginResult.Invoke();

		var entity = new PFEntityKey(response.AuthenticationContext);
		if (PlayerEntity.LocalPlayer == null || PlayerEntity.LocalPlayer.name != playerName){
			//Set PlayerEntity.LocalPlayer
			SetPlayerData(response.SessionTicket, playerName, entity, afterLogin);
		} else {
			afterLogin.Invoke();
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
	}

	void SetPlayerData(string sessionTicket, string playerName, PFEntityKey entity, Action callback){
		// Debug.Log(JsonConvert.SerializeObject(setObjects));

		// Not sure if I'm actually using this. I'm pretty sure the playerName is coming through the UserTitleDisplayName.
		var setObjects = new List<SetObject>{
			// A free-tier customer may store up to 3 objects on each entity
			new PlayerInfo { PlayerName = playerName }.ToSetObject(),
		};
		PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {
			DisplayName = playerName
		}, OnUpdateNameSuccess, OnPlayFabError);
		PlayFabDataAPI.SetObjects(new SetObjectsRequest {
			Entity = new EntityKey(entity.Id, entity.Type),
			Objects = setObjects,
		}, (setResult) => {
			// ExtDebug.LogJson("setResult: ", setResult);
			var pInfo = setObjects[0].DataObject as PlayerInfo;
			PlayerEntity.LocalPlayer = new PlayerEntity(entity, pInfo, sessionTicket);

			// GetPlayerData(entity, (result) => {
			// 	var objs = result.Objects;
			// 	Debug.Log(JsonConvert.SerializeObject(objs));
			// });
			//DisplayMessage($"Player name is {PlayerEntity.LocalPlayer.name}");
			callback();
		}, OnPlayFabError);
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

	private void OnUpdateNameSuccess(UpdateUserTitleDisplayNameResult result)
	{
		// Debug.Log("OnUpdateNameSuccess");
		// Debug.Log(JsonConvert.SerializeObject(result));
	}

}
