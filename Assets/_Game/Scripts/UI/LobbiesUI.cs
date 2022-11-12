using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using PlayFab;

public class LobbiesUI : MonoBehaviour
{
	const float MATCHES_UPDATE_INTERVAL = 6f;
	const float INITIAL_SEARCH_DELAY = 1.5f;
	[SerializeField] Transform lobbyListPanel;
	[SerializeField] GameObject lobbyListItemPrefab;
	// [SerializeField] JoinedLobbyUI joinedLobbyUI;
	[SerializeField] UIText statusMessage;
	[SerializeField] MatchmakingUI matchmakingUI;
	[SerializeField] MultiplayerMenu multiplayerMenu;

	[SerializeField] NetworkManager networkManager;
	[SerializeField] TransportWrapper transportWrapper;

	NewNetworkAuthenticator networkAuthenticator;

	bool fetchingMatches;

	void OnEnable(){
		statusMessage.text = string.Empty;
		// Make sure authenticator is set up for multiplayer.
		if (networkManager.authenticator == null) {
			networkManager.authenticator = GameObject.FindObjectOfType<NewNetworkAuthenticator>();
		}
		networkAuthenticator = networkManager.authenticator as NewNetworkAuthenticator;
		AddEventHandlers();
		Invoke(nameof(FindLobbies), INITIAL_SEARCH_DELAY);
	}
	void OnDisable(){
		fetchingMatches = false;
		RemoveEventHandlers();
	}

	void AddEventHandlers(){
		LobbyUtility.OnLobbyCreateAndJoinCompleted += OnLobbyCreateAndJoinCompleted;
		LobbyUtility.OnLobbyFindLobbiesCompleted += OnLobbyFindLobbiesCompleted;
		LobbyUtility.OnLobbyJoinCompleted += OnLobbyJoinCompleted;

		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent += OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent += AuthenticateClient;
			networkAuthenticator.AuthResponseEvent += OnAuthResponse;
			networkAuthenticator.OnClientAuthenticated.AddListener(AuthSuccess);
		}

	}

	void RemoveEventHandlers(){
		LobbyUtility.OnLobbyCreateAndJoinCompleted -= OnLobbyCreateAndJoinCompleted;
		LobbyUtility.OnLobbyFindLobbiesCompleted -= OnLobbyFindLobbiesCompleted;
		LobbyUtility.OnLobbyJoinCompleted -= OnLobbyJoinCompleted;

		if (networkAuthenticator != null){
			networkAuthenticator.AuthErrorEvent -= OnAuthError;
			networkAuthenticator.ClientAuthenticateEvent -= AuthenticateClient;
			networkAuthenticator.AuthResponseEvent -= OnAuthResponse;
		}
	}

	void AuthenticateClient(){
		// isAttemptingAuthentication = true;
		// Debug.Log("AuthenticateClient " + authenticationMode.ToString());
		var playerName = PlayerEntity.LocalPlayer.name;
		// Tell the server that the user logged in.
		NetworkClient.connection.Send(new AuthRequestMessage {
			username = PlayerEntity.LocalPlayer.name,
			entityId = PlayerEntity.LocalPlayer.entityKey.Id,
			sessionTicket = PlayerEntity.LocalPlayer.sessionTicket,
		});
	}

	private void OnAuthError(string error)
	{
		NetworkClient.Disconnect();
		DisplayMessage(error, "red");
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
		// Destroy(menuRootCanvas);
	}

	public void DisplayMessage(string text, string color = ""){
		statusMessage.SetText(text, color);
	}

	void SetLobbyListEnabled(bool val){
		foreach (var button in lobbyListPanel.GetComponentsInChildren<Selectable>()){
			button.interactable = val;
		}
	}

	public void OnClickedCancel(){
		DisplayMessage(string.Empty);
		// "log out" and return to menu.
		PlayFabClientAPI.ForgetAllCredentials();
		PlayerEntity.LocalPlayer = null;
		multiplayerMenu.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}


	#region Create Lobby

	public void OnClickCreate(){
		DisplayMessage("Creating Lobby...");

		LobbyUtility.CreateLobby("Test", 5, true);
	}

	// private void OnMatchCreated(CreateMatchResponse msg)
	// {
	// 	statusMessage.text = "";
	// 	if (msg.match != null)
	// 	{
	// 		// Lobby was successfully created
	// 		Debug.Log("Lobby was successfully created: " + msg.match.matchId);
	// 		addLobbyForm.gameObject.SetActive(false);
	// 	}
	// 	else
	// 	{
	// 		// Error creating a lobby
	// 		addLobbyForm.SetError("Error creating lobby");
	// 	}
	// 	addLobbyForm.SetBusy(false);
	// }

	private void OnLobbyCreateAndJoinCompleted(Lobby lobby)
	{
		if (lobby != null){
			DisplayMessage("Lobby was successfully created");
			// Lobby was successfully created
			Debug.Log(lobby.ConnectionString);
			// addLobbyForm.gameObject.SetActive(false);
			JoinedLobby(lobby);
		}
		else
		{
			// Error creating a lobby
			DisplayMessage("Error creating a lobby", "red");
		}
		// addLobbyForm.SetBusy(false);
	}

	#endregion

	#region Find Lobbies
	public void FindLobbies(){
		if (!fetchingMatches){
			fetchingMatches = LobbyUtility.FindLobbies();
			// findLobbiesButton.interactable = false;
		}
	}

	IEnumerator UpdateMatches(){
		// Initial delay, just for the heck of it.
		yield return new WaitForSeconds(1f);
		var normalInterval = new WaitForSeconds(MATCHES_UPDATE_INTERVAL);
		var waitInterval = new WaitForSeconds(0.3f);
		while (true){
			// If a previous request is somehow still pending a response, keep waiting.
			while (fetchingMatches){
				yield return waitInterval;
			}
			FindLobbies();
			yield return normalInterval;
		}
	}

	private void OnLobbyFindLobbiesCompleted(IList<LobbySearchResult> searchResults)
	{
		fetchingMatches = false;
		if (searchResults != null)
		{
			// Successfully found lobbies
			Debug.Log($"Found {searchResults.Count} lobbies");
			for (int i=lobbyListPanel.childCount-1; i>=0; i--){
				Destroy(lobbyListPanel.GetChild(i).gameObject);
			}

			// Iterate through lobby search results
			foreach (LobbySearchResult result in searchResults)
			{
				// Examine a search result
				Debug.Log(JsonConvert.SerializeObject(result));

				var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
				listItem.lobby = new LobbyWrapper {
					id = result.LobbyId,
					lobbyOwnerId = result.OwnerEntity.Id,
					currentMembers = result.CurrentMemberCount,
					maxMembers = result.MaxMemberCount,
					connectionString = result.ConnectionString,
					isPublic = true // TODO: How to get this?
				};
				listItem.UpdateUI();
			}
		}
		else
		{
			// Error finding lobbies
			DisplayMessage("Error finding lobbies", "red");
		}
		StartCoroutine(ReenableSearchButton());
	}

	IEnumerator ReenableSearchButton(){
		yield return new WaitForSeconds(5);
		// findLobbiesButton.interactable = true;
	}
	#endregion

	#region Join Lobbies
	public void JoinLobby( string connectionString){
		// statusMessage.text = "Joining match...";
		DisplayMessage("Joining Lobby...");
		SetLobbyListEnabled(false);
		// NetworkClient.Send(new AddPlayerToMatchRequest { lobbyId = matchId });
		PlayFabMultiplayer.JoinLobby(
			PlayerEntity.LocalPlayer.PFEntityKey,
			connectionString,
			new Dictionary<string, string>{
				{"PlayerName", PlayerEntity.LocalPlayer.name},
			});
	}

	private void OnLobbyJoinCompleted(Lobby lobby)
	{
		if (lobby != null)
		{
			// Successfully joined a lobby
			Debug.Log("Joined a lobby");
			JoinedLobby(lobby);
		}
		else
		{
			// Error joining a lobby
			DisplayMessage("Error joining a lobby", "red");
			SetLobbyListEnabled(true);
		}
	}

	void JoinedLobby(Lobby lobby){
		if (matchmakingUI.gameObject.activeInHierarchy){
			matchmakingUI.gameObject.SetActive(false);
		}
		DisplayMessage("Connecting...");
		Debug.Log($"{PlayerEntity.LocalPlayer?.name} JoinedLobby {lobby.Id}");
		PlayerEntity.LocalPlayer.lobbyId = lobby.Id;

		if (Config.Instance.forceLocalServer){
			OnRequestedServerResponse(new PlayFab.MultiplayerModels.RequestMultiplayerServerResponse {
				IPV4Address = Config.Instance.localServerIP,
				Ports = new List<PlayFab.MultiplayerModels.Port> {
					new PlayFab.MultiplayerModels.Port {
						Num = (int)Config.Instance.localServerPort,
					}
				}
			});
		} else {
			PlayFabMultiplayerAPI.RequestMultiplayerServer(
				new PlayFab.MultiplayerModels.RequestMultiplayerServerRequest {
					PreferredRegions = new List<string>() { "EastUs" }, // TODO: This should probably be based on some user setting and depending on whether I run servers in other regions.
					SessionId = System.Guid.NewGuid().ToString(),
					BuildId = Config.Instance.buildId,
				},
				OnRequestedServerResponse,
				e => OnError(e.ErrorMessage)
			);
		}

		// MatchManager.instance.AddPlayerToMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
		// NetworkClient.Send(new AddPlayerToMatchRequest {lobbyId = lobby.Id});
		// mainLobbiesUI.SetActive(false);
		// joinedLobbyUI.LoadLobby(lobby.Id);
		// joinedLobbyUI.gameObject.SetActive(true);
	}

	private void OnRequestedServerResponse(PlayFab.MultiplayerModels.RequestMultiplayerServerResponse response)
	{
		ExtDebug.LogJson("OnRequestedServerResponse: ", response);
		// var networkManager = FindObjectOfType<NetworkManager>();
		// var transport = networkManager.GetComponent<TransportWrapper>();
		// transport.SetServerBindAddress(response.IPV4Address, FishNet.Transporting.IPAddressType.IPv4);
		networkManager.networkAddress = response.IPV4Address;
		transportWrapper.SetPort((ushort)response.Ports[0].Num);
		networkManager.StartClient();
	}

	#endregion

	#region Matchmaking

	public void StartMatchmaking(){
		matchmakingUI.gameObject.SetActive(true);
		matchmakingUI.StartMatchmaking();
	}

	#endregion


	private void OnError(string error)
	{
		Debug.LogError(error);
		// mainLobbiesUI.DisplayMessage(error, "red");
	}

}
