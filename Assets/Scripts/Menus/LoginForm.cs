using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab.MultiplayerModels;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Linq;
using Mirror;
using static NewNetworkAuthenticator;
using UnityEngine.Events;
using PlayFab.Multiplayer;

public class LoginForm : MonoBehaviour {
	const string LOCAL_PLAYER_NAME_KEY = "local_multiplayer_name";
	const string ONLINE_PLAYER_NAME_KEY = "online_multiplayer_name";
	ExtNetworkRoomManager networkManager;
	TransportHelper transport;
	[SerializeField] string buildId;
	[SerializeField] TMP_InputField usernameField;
	[SerializeField] TMP_Text messageText;
	[SerializeField] TMP_InputField localIPAddressField;
	[SerializeField] TMP_InputField localPortNumField;
	[SerializeField] GameObject menuRootCanvas;

	public List<AzureRegion> regions = new List<AzureRegion>() { AzureRegion.EastUs };

	[SerializeField] bool useRandomFakeDeviceId;

	NewNetworkAuthenticator networkAuthenticator;

	enum AuthenticationMode {
		Local,
		CustomID,
		PlayFabLogin,
		PlayFabRegister,
	}
	AuthenticationMode authenticationMode;

	bool isAttemptingAuthentication;

	void Awake(){
		DontDestroyOnLoad(menuRootCanvas); // Need to keep this around long enough to finish authenticating. This gets destroyed in AuthSuccess().
		usernameField.text = PlayerPrefs.GetString(ONLINE_PLAYER_NAME_KEY, "Player");
		localIPAddressField.text = NetworkConfig.LocalIpAddress;
		localPortNumField.text = NetworkConfig.LocalPort.ToString();
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		transport = networkManager.GetComponent<TransportHelper>();
		networkManager.networkAddress = localIPAddressField.text = NetworkConfig.LocalIpAddress;
		transport.SetPort(NetworkConfig.LocalPort);

		networkManager._OnStartClient += OnStartClient;
		networkManager._OnClientConnect += OnClientConnect;
		networkManager._OnClientDisconnect += OnClientDisconnect;
		networkManager._OnClientError += OnClientError;
		// networkManager._OnStopClient += OnStopClient;
	}

	void UpdateTransport(){
		NetworkConfig.ResetOverrides();
		NetworkConfig.LocalIpAddress = localIPAddressField.text;
		if (ushort.TryParse(localPortNumField.text, out ushort p)){
			NetworkConfig.LocalPort = p;
		}
		NetworkConfig.UseLocalConfig(networkManager, transport);
	}

	void OnEnable(){
		// Don't bind event handlers until the login form is active, and remove them when it goes inactive.
		// Authenticator is unlinked from the manager when clicking single player button.

		// Make sure authenticator is set up for multiplayer.
		if (networkManager.authenticator == null) {
			networkManager.authenticator = GameObject.FindObjectOfType<NewNetworkAuthenticator>();
		}
		networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		// Make sure the room scene is being used when starting multiplayer, because the single player button changes it to another scene.
		networkManager.onlineScene = networkManager.RoomScene;
		networkManager.maxConnections = NetworkConfig.MAX_MULTIPLAYER_CONNECTIONS;
		networkManager.gameMode = ExtNetworkRoomManager.GameMode.Multiplayer;
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent += OnLoginError;
			networkAuthenticator.ClientAuthenticateEvent += AuthenticateClient;
			networkAuthenticator.AuthResponseEvent += OnAuthResponse;
			networkAuthenticator.OnClientAuthenticated.AddListener(AuthSuccess);
			// networkAuthenticator.DisplayMessage += DisplayMessage;
		}
	}

	void OnDisable(){
		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent -= OnLoginError;
			networkAuthenticator.ClientAuthenticateEvent -= AuthenticateClient;
			networkAuthenticator.AuthResponseEvent -= OnAuthResponse;
			// networkAuthenticator.DisplayMessage -= DisplayMessage;
		}
		// Reverse DontDestroyOnLoad
		SceneManager.MoveGameObjectToScene(menuRootCanvas, SceneManager.GetActiveScene());
		messageText.text = string.Empty;
	}

	void OnDestroy(){
		networkManager._OnStartClient -= OnStartClient;
		networkManager._OnClientConnect -= OnClientConnect;
		networkManager._OnClientDisconnect -= OnClientDisconnect;
		networkManager._OnClientError -= OnClientError;
		// networkManager._OnStopClient -= OnStopClient;
	}


	void OnStartClient(string networkAddress, ushort port){
		if (!isAttemptingAuthentication){
			DisplayMessage($"Connecting {networkAddress}:{port}...");
		}
	}
	void OnClientConnect(){
		DisplayMessage("Connected");
	}
	void OnClientDisconnect(){
		Debug.Log("OnClientDisconnect");
		if (isAttemptingAuthentication) {
			// DisplayMessage("Failed to connect");
			isAttemptingAuthentication = false;
		} else {
			DisplayMessage("Not connected");
		}
	}
	void OnClientError(Exception exception){
		DisplayMessage(exception.Message, "red");
	}
	// void OnStopClient(){
	// 	DisplayMessage("Client stopped");
	// }
	void DisplayMessage(string text, string color = ""){
		messageText.text = !string.IsNullOrEmpty(color) ? $"<color={color}>{text}</color>" : text;
	}

	void AuthenticateClient(){
		isAttemptingAuthentication = true;
		Debug.Log("AuthenticateClient " + authenticationMode.ToString());
		var playerName = !string.IsNullOrWhiteSpace(usernameField.text) ? usernameField.text : "Nameless nobody";
		if (!string.IsNullOrWhiteSpace(usernameField.text)){
			PlayerPrefs.SetString(ONLINE_PLAYER_NAME_KEY, usernameField.text);
		}
		switch (authenticationMode){
			case AuthenticationMode.CustomID:
				DisplayMessage("Authenticating with device ID...");
				PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest{
					TitleId = PlayFabSettings.TitleId,
					CustomId = GetDeviceId() + playerName,
					CreateAccount = true,
				}, OnPlayFabLoginSuccess, OnPlayFabLoginError);
				break;
			// case AuthenticationMode.PlayFabLogin:
			// 	DisplayMessage("Authenticating user...");
			// 	PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest {
			// 		TitleId = PlayFabSettings.TitleId,
			// 		Username = usernameField.text,
			// 		Password = passwordField.text
			// 	}, OnPlayFabLoginSuccess, OnPlayFabLoginError);
			// 	break;
			// case AuthenticationMode.PlayFabRegister:
			// 	DisplayMessage("Registering user...");
			// 	PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest {
			// 		TitleId = PlayFabSettings.TitleId,
			// 		Username = usernameField.text,
			// 		Email = emailField.text,
			// 		Password = passwordField.text
			// 	}, OnPlayFabRegisterSuccess, OnPlayFabLoginError);
			// 	break;
			case AuthenticationMode.Local:
				NetworkClient.connection.Send(new AuthRequestMessage {
					username = playerName
				});
				break;
		}
	}

	private void OnPlayFabLoginSuccess(LoginResult response)
	{
		Debug.Log(JsonUtility.ToJson(response.AuthenticationContext));
		PlayFabMultiplayer.SetEntityToken(response.AuthenticationContext); // Is this needed?

		// Tell the server that the user logged in.
		NetworkClient.connection.Send(new AuthRequestMessage {
			username = !string.IsNullOrWhiteSpace(usernameField.text) ? usernameField.text : "Nameless nobody",
			entityId = response.EntityToken.Entity.Id,
			sessionTicket = response.SessionTicket,
		});
	}
	// private void OnPlayFabRegisterSuccess(RegisterPlayFabUserResult response)
	// {
	// 	// Tell the server that the user logged in.
	// 	NetworkClient.connection.Send(new AuthRequestMessage {
	// 		Username = response.Username,
	// 		EntityId = response.EntityToken.Entity.Id,
	// 		SessionTicket = response.SessionTicket,
	// 	});
	// }

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

	public void HostLocalServer(){
		UpdateTransport();
		authenticationMode = AuthenticationMode.Local;
		DisplayMessage("Starting host...");
		networkManager.StartHost();
	}

	public void JoinLocalServer(){
		UpdateTransport();
		authenticationMode = AuthenticationMode.Local;
		DisplayMessage("Starting client...");
		networkManager.StartClient();
	}

	public void OnClickLogin(){
		if (buildId == "")
		{
			throw new Exception("A remote client build must have a buildId. Add it to the Configuration. Get this from your Multiplayer Game Manager in the PlayFab web console.");
		}
		else
		{
			LoginRemoteUser();
		}
	}

	// public void OnClickSinglePlayer(){
	// 	authenticationMode = AuthenticationMode.Local;
	// 	NetworkConfig.ResetOverrides();
	// 	NetworkConfig.UseLocalConfig(networkManager, transport);
	// 	networkManager.maxConnections = 1;
	// 	networkManager.StartHost();
	// }

	public void LoginRemoteUser(){
		NetworkConfig.UseOnlineConfig(networkManager, transport);
		authenticationMode = AuthenticationMode.CustomID;
		DisplayMessage("Starting client...");
		networkManager.StartClient();
	}

	private void OnLoginError(string error)
	{
		NetworkClient.Disconnect();
		DisplayMessage(error, "red");
	}

	private void OnPlayFabLoginError(PlayFabError error)
	{
		NetworkClient.Disconnect();
		DisplayMessage(error.ErrorMessage, "red");
		// Disconnect after failed login
	}

	private void OnAuthResponse(AuthResponseMessage msg)
	{
		// TODO: Is it redundant to do this here?
		NetworkClient.connection.authenticationData = new AuthenticationInfo {
			EntityId = msg.entityId,
			SessionTicket = msg.sessionTicket,
		};
	}

	void AuthSuccess()
	{
		// Debug.Log("Logged in? " + (PlayFabClientAPI.IsClientLoggedIn() ? "Yes" : "No"));
		// Debug.Log((NetworkClient.connection.authenticationData as AuthenticationInfo).EntityId);
		Debug.Log("Auth Success!");
		Destroy(menuRootCanvas);
	}
}
