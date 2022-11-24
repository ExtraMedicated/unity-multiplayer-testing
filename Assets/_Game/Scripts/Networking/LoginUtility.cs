using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
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

	void OnEnable(){
		var networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent += OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent += AuthenticateClient;
			networkAuthenticator.AuthResponseEvent += OnAuthResponse;
			networkAuthenticator.OnClientAuthenticated.AddListener(AuthSuccess);
		}
	}
	void OnDisable(){
		var networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent -= OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent -= AuthenticateClient;
			networkAuthenticator.AuthResponseEvent -= OnAuthResponse;
		}
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
	#if PLATFORM_ANDROID
		// From: http://answers.unity.com/answers/654480/view.html
		AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
		AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject> ("getContentResolver");
		AndroidJavaClass secure = new AndroidJavaClass ("android.provider.Settings$Secure");
		return secure.CallStatic<string> ("getString", contentResolver, "android_id");
	#else
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
	#endif
	}

	public void AttemptPlayfabLogin(string playerName, Action onLoginResult, Action afterLogin){
		authenticationMode = LoginUtility.AuthenticationMode.CustomID;
		Debug.Log("AuthenticateClient " + authenticationMode.ToString());
		var infoRequestParameters = new GetPlayerCombinedInfoRequestParams {
			GetPlayerProfile = true,
			GetPlayerStatistics = true,
			GetTitleData = true,
			GetUserData = true,
			ProfileConstraints = new PlayerProfileViewConstraints {
				ShowDisplayName = true,
			}
		};
		// Log into playfab
		#if PLATFORM_ANDROID
		PlayFabClientAPI.LoginWithAndroidDeviceID(
			new LoginWithAndroidDeviceIDRequest{
				TitleId = PlayFabSettings.TitleId,
				AndroidDeviceId = appendNameToDeviceID ? GetDeviceId() + playerName : GetDeviceId(),
				CreateAccount = true,
				InfoRequestParameters = infoRequestParameters,
			},
			r => OnPlayFabLoginSuccess(r, playerName, onLoginResult, afterLogin),
			OnPlayFabError);
		#else
		PlayFabClientAPI.LoginWithCustomID(
			new LoginWithCustomIDRequest{
				TitleId = PlayFabSettings.TitleId,
				CustomId = appendNameToDeviceID ? GetDeviceId() + playerName : GetDeviceId(),
				CreateAccount = true,
				InfoRequestParameters = infoRequestParameters,
			},
			r => OnPlayFabLoginSuccess(r, playerName, onLoginResult, afterLogin),
			OnPlayFabError);
		#endif
	}

	public static void Logout(){
		PlayFabClientAPI.ForgetAllCredentials();
		PlayerEntity.AuthContext = null;
		if (NetworkClient.isConnected){
			NetworkClient.connection.authenticationData = null;
		}
		PlayerEntity.SetLocalPlayer(null);
	}

	private void OnPlayFabError(PlayFabError e){
		ExtDebug.LogJsonError("Login error: ", e.GenerateErrorReport());
		OnError?.Invoke(e.ErrorMessage);
	}

	private void OnPlayFabLoginSuccess(LoginResult response, string playerName, Action onLoginResult, Action afterLogin)
	{
		// Debug.Log("EntityToken: " + response.EntityToken.EntityToken);
		PlayerEntity.AuthContext = response.AuthenticationContext;
		PlayerEntity.EntityToken = response.EntityToken.EntityToken;
		// #if !UNITY_ANDROID
		// try {
		// 	PlayFabMultiplayer.SetEntityToken(response.AuthenticationContext); // Is this needed?
		// } catch (Exception e){
		// 	OnError?.Invoke(e.Message);
		// }
		// #endif

		Action callback = () => {
			PlayerEntity.LocalPlayer.StartSignalR();
			afterLogin.Invoke();
		};

		onLoginResult.Invoke();
		if (PlayerEntity.LocalPlayer == null || PlayerEntity.LocalPlayer.name != playerName){
			//Set PlayerEntity.LocalPlayer
			SetPlayerData(response.SessionTicket, playerName, response.EntityToken.Entity, callback);
		} else {
			callback.Invoke();
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

	void SetPlayerData(string sessionTicket, string playerName, EntityKey entity, Action callback){
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
			Entity = entity,
			Objects = setObjects,
		}, (setResult) => {
			// ExtDebug.LogJson("setResult: ", setResult);
			var pInfo = setObjects[0].DataObject as PlayerInfo;
			PlayerEntity.SetLocalPlayer(new PlayerEntity(entity, pInfo, sessionTicket));
			// GetPlayerData(entity, (result) => {
			// 	var objs = result.Objects;
			// 	Debug.Log(JsonConvert.SerializeObject(objs));
			// });
			//DisplayMessage($"Player name is {PlayerEntity.LocalPlayer.name}");
			callback();
		}, OnPlayFabError);
	}


	void GetPlayerData(EntityKey entity, Action<GetObjectsResponse> callback){
		var getRequest = new GetObjectsRequest {Entity = entity};
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
