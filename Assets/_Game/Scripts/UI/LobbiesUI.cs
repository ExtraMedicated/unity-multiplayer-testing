using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
// using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using PlayFab;
using System;
using PlayFab.MultiplayerModels;

public class LobbiesUI : MonoBehaviour
{
	const float MATCHES_UPDATE_INTERVAL = 6f;
	const float INITIAL_SEARCH_DELAY = 3f;
	[SerializeField] Transform lobbyListPanel;
	[SerializeField] GameObject lobbyListItemPrefab;
	[SerializeField] MatchmakingUI matchmakingUI;
	[SerializeField] MultiplayerMenu multiplayerMenu;
	[SerializeField] NetworkManager networkManager;
	[SerializeField] TransportWrapper transportWrapper;
	[SerializeField] TMP_InputField searchField;
	[SerializeField] InputFieldWrapper lobbyCodeInput;


	bool fetchingMatches;

	void OnEnable(){
		OnScreenMessage.SetText(string.Empty);
		AddEventHandlers();
		Invoke(nameof(FindLobbies), INITIAL_SEARCH_DELAY);
	}
	void OnDisable(){
		fetchingMatches = false;
		RemoveEventHandlers();
	}

	void AddEventHandlers(){
		// LobbyUtility.OnLobbyCreateAndJoinCompleted += OnLobbyCreateAndJoinCompleted;
		// LobbyUtility.OnLobbyFindLobbiesCompleted += OnLobbyFindLobbiesCompleted;
		// LobbyUtility.OnLobbyJoinCompleted += OnLobbyJoinCompleted;

	}

	void RemoveEventHandlers(){
		// LobbyUtility.OnLobbyCreateAndJoinCompleted -= OnLobbyCreateAndJoinCompleted;
		// LobbyUtility.OnLobbyFindLobbiesCompleted -= OnLobbyFindLobbiesCompleted;
		// LobbyUtility.OnLobbyJoinCompleted -= OnLobbyJoinCompleted;

	}


	public void DisplayMessage(string text, string color = ""){
		OnScreenMessage.SetText(text, color);
	}

	void SetLobbyListEnabled(bool val){
		foreach (var button in lobbyListPanel.GetComponentsInChildren<Selectable>()){
			button.interactable = val;
		}
	}

	public void OnClickedCancel(){
		DisplayMessage(string.Empty);
		// "log out" and return to menu.
		LoginUtility.Logout();
		multiplayerMenu.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}


	#region Create Lobby

	public void OnClickCreate(){
		DisplayMessage("Creating Lobby...");

		// LobbyUtility.CreateLobby("Test", 5, true); // TODO: Make a menu to configure this stuff.
	}

	// private void OnLobbyCreateAndJoinCompleted(Lobby lobby)
	// {
	// 	if (lobby != null){
	// 		DisplayMessage("Lobby was successfully created");
	// 		// Lobby was successfully created
	// 		Debug.Log(lobby.ConnectionString);
	// 		// addLobbyForm.gameObject.SetActive(false);
	// 		JoinedLobby(lobby);
	// 	}
	// 	else
	// 	{
	// 		// Error creating a lobby
	// 		DisplayMessage("Error creating a lobby", "red");
	// 	}
	// 	// addLobbyForm.SetBusy(false);
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
			fetchingMatches = LobbyUtility.FindLobbies(searchField.text, OnLobbyFindLobbiesCompleted, OnError);
			// findLobbiesButton.interactable = false;
		}
	}

	// private void OnLobbyFindLobbiesCompleted(IList<LobbySearchResult> searchResults)
	// {
	// 	fetchingMatches = false;
	// 	if (searchResults != null)
	// 	{
	// 		// Successfully found lobbies
	// 		Debug.Log($"Found {searchResults.Count} lobbies");
	// 		OnScreenMessage.SetText($"Found {searchResults.Count} lobbies");
	// 		for (int i=lobbyListPanel.childCount-1; i>=0; i--){
	// 			Destroy(lobbyListPanel.GetChild(i).gameObject);
	// 		}

	// 		// Iterate through lobby search results
	// 		foreach (LobbySearchResult result in searchResults)
	// 		{
	// 			// Examine a search result
	// 			Debug.Log(JsonConvert.SerializeObject(result));

	// 			var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
	// 			ExtDebug.LogJson("LobbyWrapper SearchData: ", result.SearchProperties);
	// 			listItem.lobby = new LobbyWrapper {
	// 				id = result.LobbyId,
	// 				lobbyOwnerId = result.OwnerEntity.Id,
	// 				currentMembers = result.CurrentMemberCount,
	// 				maxMembers = result.MaxMemberCount,
	// 				connectionString = result.ConnectionString,
	// 				searchData = result.SearchProperties as Dictionary<string,string>,
	// 				isPublic = true // TODO: How to get this?
	// 			};
	// 			listItem.UpdateUI();
	// 		}
	// 	}
	// 	else
	// 	{
	// 		// Error finding lobbies
	// 		DisplayMessage("Error finding lobbies", "red");
	// 	}
	// 	StartCoroutine(ReenableSearchButton());
	// }

	private void OnLobbyFindLobbiesCompleted(List<PlayFab.MultiplayerModels.LobbySummary> searchResults)
	{
		fetchingMatches = false;
		if (searchResults != null)
		{
			// Successfully found lobbies
			Debug.Log($"Found {searchResults.Count} lobbies");
			OnScreenMessage.SetText($"Found {searchResults.Count} lobbies");
			for (int i=lobbyListPanel.childCount-1; i>=0; i--){
				Destroy(lobbyListPanel.GetChild(i).gameObject);
			}

			// Iterate through lobby search results
			foreach (var result in searchResults)
			{
				// Examine a search result
				Debug.Log(JsonConvert.SerializeObject(result));

				var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
				listItem.lobby = new LobbyWrapper {
					id = result.LobbyId,
					lobbyOwnerId = result.Owner.Id,
					currentMembers = result.CurrentPlayers,
					maxMembers = result.MaxPlayers,
					connectionString = result.ConnectionString,
					searchData = result.SearchData,
					isPublic = result.SearchData[LobbyWrapper.LOBBY_VISIBILITY_SEARCH_KEY] == ((int)LobbyWrapper.LobbyVisibility.Visible).ToString() // Won't show up in search if it's private.
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
	public void JoinLobby(string connectionString){
		// statusMessage.text = "Joining match...";
		DisplayMessage("Joining Lobby...");
		SetLobbyListEnabled(false);
		LobbyUtility.JoinLobby(connectionString, OnLobbyJoinCompleted, OnError);

		// PlayFabMultiplayer.JoinLobby(
		// 	PlayerEntity.LocalPlayer.entityKey,
		// 	connectionString,
		// 	new Dictionary<string, string>{
		// 		{"PlayerName", PlayerEntity.LocalPlayer.name},
		// 	}
		// );
	}

	public void FindAndJoinSpecifiedLobby(){
		if (!string.IsNullOrWhiteSpace(lobbyCodeInput.text)){
			DisplayMessage($"Finding lobby {lobbyCodeInput.text.ToUpperInvariant()}...");
			LobbyUtility.FindLobbyByCode(
				lobbyCodeInput.text.ToUpperInvariant(),
				OnFindLobbyByCode,
				OnError
			);
		} else {
			OnError("You forgot to enter a lobby code.");
		}
	}

	void OnFindLobbyByCode(List<PlayFab.MultiplayerModels.LobbySummary> lobbies){
		if (lobbies.Count == 0){
			OnError("Could not find a lobby with the specified code.");
		} else {
			JoinLobby(lobbies[0].ConnectionString);
		}
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
		ExtDebug.LogJson("------------- JoinedLobby: ", lobby);

		DisplayMessage($"Connecting to {networkManager.networkAddress}:{transportWrapper.GetPort()}...");
		Debug.Log($"{PlayerEntity.LocalPlayer?.name} JoinedLobby {lobby.LobbyId}");
		// if (lobby.TryGetOwner(out PFEntityKey owner)){
			LobbyUtility.CurrentlyJoinedLobby = new BasicLobbyInfo {
				lobbyId = lobby.LobbyId,
				lobbyOwnerId = lobby.Owner.Id,
				scene = lobby.LobbyData[LobbyWrapper.LOBBY_LEVEL_KEY]
			};
		// }

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
			Debug.Log("Lobby Session ID: " + lobby.LobbyData[LobbyWrapper.LOBBY_SESSION_KEY]);
			OnScreenMessage.SetText("Requesting Server...");
			PlayFabMultiplayerAPI.RequestMultiplayerServer(
				new PlayFab.MultiplayerModels.RequestMultiplayerServerRequest {
					PreferredRegions = new List<string>() { "EastUs" }, // TODO: This should probably be based on some user setting and depending on whether I run servers in other regions.
					SessionId = lobby.LobbyData[LobbyWrapper.LOBBY_SESSION_KEY],
					// BuildId = Config.Instance.buildId,
					BuildAliasParams = new PlayFab.MultiplayerModels.BuildAliasParams {
						AliasId = Config.Instance.buildAliasId,
					}
				},
				OnRequestedServerResponse,
				e => OnError(e.GenerateErrorReport())
			);
		}
	}

	private void OnRequestedServerResponse(PlayFab.MultiplayerModels.RequestMultiplayerServerResponse response)
	{
		// TODO: Would I need to make sure the State is "Active" before attempting to connect? Am I supposed to poll for that?
		ExtDebug.LogJson("OnRequestedServerResponse: ", response);
		networkManager.networkAddress = response.IPV4Address;
		transportWrapper.SetPort((ushort)response.Ports[0].Num);

		// Starting the client triggers the Mirror authentication.
		networkManager.StartClient();

		StartCoroutine(CheckConnectionStatus(transportWrapper.GetTimeoutMS()/1000f));
	}

	IEnumerator CheckConnectionStatus(float time){
		yield return new WaitForSeconds(time);
		OnError("Failed to connect to the game server.");
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
		OnScreenMessage.SetText(error, "red");
	}

	internal void CreateAndJoinLobby(string lobbyName, string level, uint maxPlayers, bool isInvisible, bool isPublic)
	{
		LobbyUtility.CreateAndJoinLobby(
			lobbyName,
			level,
			maxPlayers,
			isInvisible,
			isPublic,
			OnLobbyCreateAndJoinCompleted,
			OnError
		);
	}
}
