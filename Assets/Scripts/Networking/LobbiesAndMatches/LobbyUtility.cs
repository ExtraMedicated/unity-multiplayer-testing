using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Newtonsoft.Json;
// using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUtility : MonoBehaviour {
	// const float MATCHES_UPDATE_INTERVAL = 6f;

	// LobbySearchConfiguration lobbySearchConfig;
	// [SerializeField] Button findLobbiesButton;
	public static LobbyUtility instance;
	[SerializeField] Transform lobbyListPanel;
	[SerializeField] GameObject lobbyListItemPrefab;
	[SerializeField] AddLobbyForm addLobbyForm;
	[SerializeField] GameObject mainLobbiesUI;
	[SerializeField] JoinedLobbyUI joinedLobbyUI;
	ExtNetworkRoomManager networkRoomManager;
	[SerializeField] MatchManager matchManager;
	[SerializeField] TMP_Text statusMessage;

	// Coroutine updateMatchesCoroutuine;
	// bool fetchingMatches;

	void Awake () {
		if (instance == null){
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
		networkRoomManager = FindObjectOfType<ExtNetworkRoomManager>();

		// lobbySearchConfig = new LobbySearchConfiguration();
		// joinedLobbyUI.gameObject.SetActive(false);
	}

	void Start () {
		joinedLobbyUI.gameObject.SetActive(false);
		AddEventHandlers();
	}

	// public override void OnStartLocalPlayer()
	// {
	// 	AddEventHandlers();
	// }

	// void OnEnable(){
	// 	if (networkRoomManager == null){
	// 		networkRoomManager = FindObjectOfType<ExtNetworkRoomManager>();
	// 	}
	// 	if (networkRoomManager.mode != NetworkManagerMode.ServerOnly){
	// 		updateMatchesCoroutuine = StartCoroutine(UpdateMatches());
	// 	}
	// }
	// void OnDisable(){
	// 	if (networkRoomManager.mode != NetworkManagerMode.ServerOnly && updateMatchesCoroutuine != null){
	// 		StopCoroutine(updateMatchesCoroutuine);
	// 	}
	// }

	void OnDestroy(){
		if (instance == this) {
			RemoveEventHandlers();
		}
	}

	void AddEventHandlers(){
		NetworkClient.RegisterHandler<CreateMatchResponse>(OnMatchCreated);
		// NetworkClient.RegisterHandler<AddedPlayerToMatchMessage>(OnPlayerAddedToMatch);
		// NetworkClient.RegisterHandler<PlayerRemovedFromMatchMessage>(OnPlayerRemovedFromMatch);
		// NetworkClient.RegisterHandler<FindLobbiesResponse>(OnGotMatches);
		NetworkClient.RegisterHandler<AddPlayerToMatchError>(OnAddPlayerToMatchError);

		matchManager.OnAddMatch += OnMatchAdded;
		matchManager.OnPlayerJoinedMatch += OnPlayerAddedToMatch;
		matchManager.OnPlayerLeftMatch += OnPlayerRemovedFromMatch;
		matchManager.OnRemoveMatch += OnMatchRemoved;
		matchManager.OnUpdateMatch += OnMatchUpdated;

		// PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += this.PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		// PlayFabMultiplayer.OnLobbyDisconnected += this.PlayFabMultiplayer_OnLobbyDisconnected;
		// PlayFabMultiplayer.OnLobbyFindLobbiesCompleted += this.PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		// PlayFabMultiplayer.OnLobbyJoinCompleted += this.PlayFabMultiplayer_OnLobbyJoinCompleted;
	}

	void RemoveEventHandlers(){
		NetworkClient.UnregisterHandler<CreateMatchResponse>();
		// NetworkClient.UnregisterHandler<AddedPlayerToMatchMessage>();
		// NetworkClient.UnregisterHandler<PlayerRemovedFromMatchMessage>();
		// NetworkClient.UnregisterHandler<FindLobbiesResponse>();
		NetworkClient.UnregisterHandler<AddPlayerToMatchError>();

		matchManager.OnAddMatch -= OnMatchAdded;
		matchManager.OnPlayerJoinedMatch -= OnPlayerAddedToMatch;
		matchManager.OnPlayerLeftMatch -= OnPlayerRemovedFromMatch;
		matchManager.OnRemoveMatch -= OnMatchRemoved;
		matchManager.OnUpdateMatch -= OnMatchUpdated;

		// PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted -= this.PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		// PlayFabMultiplayer.OnLobbyDisconnected -= this.PlayFabMultiplayer_OnLobbyDisconnected;
		// PlayFabMultiplayer.OnLobbyFindLobbiesCompleted -= this.PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		// PlayFabMultiplayer.OnLobbyJoinCompleted -= this.PlayFabMultiplayer_OnLobbyJoinCompleted;
	}

	private void OnMatchAdded(Match match)
	{
		Debug.Log("OnMatchAdded");
		if (match.isPublic){
			var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
			listItem.SetLobbyInfo(match);
		}
	}

	private void OnMatchUpdated(Match match)
	{
		Debug.Log("OnMatchUpdated");
		var li = (lobbyListPanel.GetComponentsInChildren<LobbyListItem>()).Where(li => li.lobby.id == match.matchId).FirstOrDefault();
		if (li != null && li.lobby.id == match.matchId){
			li.SetLobbyInfo(match);
		}
	}

	private void OnMatchRemoved(string matchId)
	{
		Debug.Log("OnMatchRemoved");
		var li = (lobbyListPanel.GetComponentsInChildren<LobbyListItem>()).Where(li => li.lobby.id == matchId).FirstOrDefault();
		if (li != null && li.lobby.id == matchId){
			Destroy(li.gameObject);
		}
	}

	#region Create

	public void CreateLobby(/*string name,*/ uint maxPlayers, bool isPublic){
		statusMessage.text = "";
		addLobbyForm.SetBusy(true);
		NetworkClient.Send(new CreateMatchRequest {
			networkPlayer = ExtNetworkRoomPlayer.localPlayer,
			// name = name,
			maxPlayers = maxPlayers,
			isPublic = isPublic,
		});
		// var createConfig = new LobbyCreateConfiguration()
		// {
		// 	MaxMemberCount = maxPlayers,
		// 	OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
		// 	AccessPolicy = LobbyAccessPolicy.Public // TODO: <-- Make this configurable.
		// };

		// createConfig.LobbyProperties["name"] = name;
		// // createConfig.LobbyProperties["Prop2"] = "Value2";

		// var joinConfig = new LobbyJoinConfiguration();
		// // joinConfig.MemberProperties["MemberProp1"] = "MemberValue1";
		// // joinConfig.MemberProperties["MemberProp2"] = "MemberValue2";

		// PlayFabMultiplayer.CreateAndJoinLobby(
		// 	ExtNetworkRoomPlayer.localPlayer.playerEntityKey,
		// 	createConfig,
		// 	joinConfig);
	}

	private void OnMatchCreated(CreateMatchResponse msg)
	{
		statusMessage.text = "";
		if (msg.match != null)
		{
			// Lobby was successfully created
			Debug.Log("Lobby was successfully created: " + msg.match.matchId);
			addLobbyForm.gameObject.SetActive(false);
		}
		else
		{
			// Error creating a lobby
			addLobbyForm.SetError("Error creating lobby");
		}
		addLobbyForm.SetBusy(false);
	}

	// private void PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted(Lobby lobby, int result)
	// {
	// 	if (LobbyError.SUCCEEDED(result))
	// 	{
	// 		// Lobby was successfully created
	// 		Debug.Log(lobby.ConnectionString);

	// 		addLobbyForm.gameObject.SetActive(false);

	// 		JoinedLobby(lobby);
	// 	}
	// 	else
	// 	{
	// 		// Error creating a lobby
	// 		Debug.Log("Error creating a lobby");
	// 		addLobbyForm.SetError("Error creating lobby");
	// 	}
	// 	addLobbyForm.SetBusy(false);
	// }
	// private void PlayFabMultiplayer_OnLobbyDisconnected(Lobby lobby)
	// {
	// 	// Disconnected from lobby
	// 	Debug.Log("Disconnected from lobby!");
	// 	LeftLobby(lobby);
	// }

	#endregion

	#region Find
	public void FindLobbies(){
		// fetchingMatches = true;
		// findLobbiesButton.interactable = false;
		NetworkClient.Send(new FindLobbiesRequest());
		// PlayFabMultiplayer.FindLobbies(ExtNetworkRoomPlayer.localPlayer.playerEntityKey, lobbySearchConfig);
	}

	// IEnumerator UpdateMatches(){
	// 	// Initial delay, just for the heck of it.
	// 	yield return new WaitForSeconds(1f);
	// 	var normalInterval = new WaitForSeconds(MATCHES_UPDATE_INTERVAL);
	// 	var waitInterval = new WaitForSeconds(0.3f);
	// 	while (true){
	// 		// If a previous request is somehow still pending a response, keep waiting.
	// 		while (fetchingMatches){
	// 			yield return waitInterval;
	// 		}
	// 		FindLobbies();
	// 		yield return normalInterval;
	// 	}
	// }

	// void OnGotMatches(FindLobbiesResponse msg){
	// 	fetchingMatches = false;
	// 	// Successfully found lobbies
	// 	Debug.Log($"Found {msg.matches.Count} matches");
	// 	for (int i=lobbyListPanel.childCount-1; i>=0; i--){
	// 		Destroy(lobbyListPanel.GetChild(i).gameObject);
	// 	}

	// 	// Iterate through lobby search results
	// 	foreach (var match in msg.matches)
	// 	{
	// 		var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
	// 		listItem.lobby = new LobbyInfo {
	// 			id = match.matchId,
	// 			currentMembers = (uint) match.players.Count,
	// 			maxMembers = match.maxPlayers,
	// 			connectionString = match.matchId,
	// 		};
	// 		listItem.UpdateUI();
	// 	}
	// }

	// private void PlayFabMultiplayer_OnLobbyFindLobbiesCompleted(
	// 	IList<LobbySearchResult> searchResults,
	// 	PFEntityKey newMember,
	// 	int reason)
	// {
	// 	if (LobbyError.SUCCEEDED(reason))
	// 	{
	// 		// Successfully found lobbies
	// 		Debug.Log($"Found {searchResults.Count} lobbies");
	// 		for (int i=lobbyListPanel.childCount-1; i>=0; i--){
	// 			Destroy(lobbyListPanel.GetChild(i).gameObject);
	// 		}

	// 		// Iterate through lobby search results
	// 		foreach (LobbySearchResult result in searchResults)
	// 		{
	// 			// Examine a search result
	// 			Debug.Log(JsonConvert.SerializeObject(result));

	// 			var listItem = Instantiate(lobbyListItemPrefab, lobbyListPanel).GetComponent<LobbyListItem>();
	// 			listItem.lobby = new LobbyInfo {
	// 				id = result.LobbyId,
	// 				currentMembers = result.CurrentMemberCount,
	// 				maxMembers = result.MaxMemberCount,
	// 				connectionString = result.ConnectionString,
	// 			};
	// 			listItem.UpdateUI();
	// 		}
	// 	}
	// 	else
	// 	{
	// 		// Error finding lobbies
	// 		Debug.Log("Error finding lobbies");
	// 	}
	// 	StartCoroutine(ReenableSearchButton());
	// }

	// IEnumerator ReenableSearchButton(){
	// 	yield return new WaitForSeconds(5);
	// 	findLobbiesButton.interactable = true;
	// }
	#endregion

	#region Join

	// void OnPlayerAddedToMatch(AddedPlayerToMatchMessage msg){
	void OnPlayerAddedToMatch(ExtNetworkRoomPlayer player, Match match){
		// Not sure what would cause player to be null, but apparently it happened when a second lobby was created.
		if (player == null){
			return;
		}
		Debug.Log($"OnPlayerAddedToMatch {player} {match}");
		if (match != null){
			if (ExtNetworkRoomPlayer.localPlayer.netId == player.netId){
				JoinedMatch(match);
			} else {
				if (joinedLobbyUI?.match != null && joinedLobbyUI.match.matchId == match.matchId){
					joinedLobbyUI.UpdateMatch(match);
				}
				var lobbyItem = lobbyListPanel.GetComponentsInChildren<LobbyListItem>().Where(l => l.lobby.id == match.matchId).FirstOrDefault();
				lobbyItem?.SetLobbyInfo(match);
			}
		} else {
			Debug.LogError("Failed to add player to match.");
		}
	}

	void OnAddPlayerToMatchError(AddPlayerToMatchError error){
		Debug.LogError(error.errorText);
		statusMessage.text = $"<color=red>{error.errorText}</color>";
	}


	// void OnPlayerRemovedFromMatch(PlayerRemovedFromMatchMessage msg){
	void OnPlayerRemovedFromMatch(ExtNetworkRoomPlayer player, Match match){
		Debug.Log($"{player.netId} Left Match");
		if (player.netId == ExtNetworkRoomPlayer.localPlayer.netId){
			// Local player left the lobby.
			joinedLobbyUI.gameObject.SetActive(false);
			joinedLobbyUI.match = null;
			// player.matchId = null;
			mainLobbiesUI.SetActive(true);
			statusMessage.text = "Left the match";
		}
		// Update lobby screen and lobbies list.
		if (joinedLobbyUI?.match != null && joinedLobbyUI.match.matchId == match.matchId){
			joinedLobbyUI.UpdateMatch(match);
		}
		var lobbyItem = lobbyListPanel.GetComponentsInChildren<LobbyListItem>().Where(l => l.lobby.id == match.matchId).FirstOrDefault();
		lobbyItem?.SetLobbyInfo(match);
	}
	// void LeftLobby(Lobby lobby){
	// 	Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} LeftLobby {lobby.Id}");
	// 	NetworkClient.Send(new RemovePlayerFromMatchMessage {lobbyId = lobby.Id});
	// 	// MatchManager.instance.RemovePlayerFromMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
	// 	joinedLobbyUI.gameObject.SetActive(false);
	// 	joinedLobbyUI.lobby = null;
	// 	mainLobbiesUI.SetActive(true);
	// }

	public void JoinLobby(string matchId){// string connectionString){
		statusMessage.text = "Joining match...";
		SetLobbyListEnabled(false);
		NetworkClient.Send(new AddPlayerToMatchRequest { lobbyId = matchId });
		// PlayFabMultiplayer.JoinLobby(
		// 	ExtNetworkRoomPlayer.localPlayer.playerEntityKey,
		// 	connectionString,
		// 	null);
	}

	// private void PlayFabMultiplayer_OnLobbyJoinCompleted(Lobby lobby, PFEntityKey newMember, int reason)
	// {
	// 	if (LobbyError.SUCCEEDED(reason))
	// 	{
	// 		// Successfully joined a lobby
	// 		Debug.Log("Joined a lobby");
	// 		JoinedLobby(lobby);
	// 	}
	// 	else
	// 	{
	// 		// Error joining a lobby
	// 		Debug.Log("Error joining a lobby");
	// 		SetLobbyListEnabled(true);
	// 	}
	// }

	void JoinedMatch(Match match){
		Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} Joined Match {match.matchId}");
		// ExtNetworkRoomPlayer.localPlayer.matchId = match.matchId;
		mainLobbiesUI.SetActive(false);
		joinedLobbyUI.match = match;
		joinedLobbyUI.gameObject.SetActive(true);
	}
	// void JoinedLobby(Lobby lobby){
	// 	Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} JoinedLobby {lobby.Id}");
	// 	// MatchManager.instance.AddPlayerToMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
	// 	NetworkClient.Send(new AddPlayerToMatchRequest {lobbyId = lobby.Id});
	// 	mainLobbiesUI.SetActive(false);
	// 	joinedLobbyUI.lobby = lobby;
	// 	joinedLobbyUI.gameObject.SetActive(true);
	// }
	// void LeftLobby(Lobby lobby){
	// 	Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} LeftLobby {lobby.Id}");
	// 	NetworkClient.Send(new RemovePlayerFromMatchMessage {lobbyId = lobby.Id});
	// 	// MatchManager.instance.RemovePlayerFromMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
	// 	joinedLobbyUI.gameObject.SetActive(false);
	// 	joinedLobbyUI.lobby = null;
	// 	mainLobbiesUI.SetActive(true);
	// }
	#endregion

	void SetLobbyListEnabled(bool val){
		foreach (var button in lobbyListPanel.GetComponentsInChildren<Selectable>()){
			button.interactable = val;
		}
	}
}
