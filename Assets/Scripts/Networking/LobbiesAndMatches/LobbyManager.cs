using System.Collections;
using System.Collections.Generic;
using Mirror;
using Newtonsoft.Json;
using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour {

	// public PFEntityKey localPlayerEntityKey;
	LobbySearchConfiguration lobbySearchConfig;

	[SerializeField] Button findLobbiesButton;
	public static LobbyManager instance;
	[SerializeField] Transform lobbyListPanel;
	[SerializeField] GameObject lobbyListItemPrefab;
	[SerializeField] AddLobbyForm addLobbyForm;
	[SerializeField] GameObject mainLobbiesUI;
	[SerializeField] JoinedLobbyUI joinedLobbyUI;

	ExtNetworkRoomManager networkRoomManager;

	// public ExtNetworkRoomPlayer localNetworkPlayer;



	// List<LobbyInfo> lobbies = new List<LobbyInfo>();
	// public readonly SyncList<string> lobbyIDs = new SyncList<string>();

	void Start () {
		if (instance == null){
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
		networkRoomManager = FindObjectOfType<ExtNetworkRoomManager>();

		// var authInfo = NetworkClient.connection?.authenticationData as AuthenticationInfo;
		// Debug.Log(NetworkClient.connection?.identity);
		// localNetworkPlayer = NetworkClient.connection?.identity.GetComponent<ExtNetworkRoomPlayer>();
		// localNetworkPlayer.playerEntityKey = new PFEntityKey(
		// 	authInfo.EntityId,
		// 	PLAYER_ENTITY_TYPE); // PlayFab user's entity key

		lobbySearchConfig = new LobbySearchConfiguration();
		joinedLobbyUI.gameObject.SetActive(false);

		AddEventHandlers();
	}

	void OnDestroy(){
		if (instance == this) {
			RemoveEventHandlers();
		}
	}

	void AddEventHandlers(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += this.PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected += this.PlayFabMultiplayer_OnLobbyDisconnected;

		PlayFabMultiplayer.OnLobbyFindLobbiesCompleted += this.PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;

		PlayFabMultiplayer.OnLobbyJoinCompleted += this.PlayFabMultiplayer_OnLobbyJoinCompleted;
	}
	void RemoveEventHandlers(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted -= this.PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected -= this.PlayFabMultiplayer_OnLobbyDisconnected;

		PlayFabMultiplayer.OnLobbyFindLobbiesCompleted -= this.PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;

		PlayFabMultiplayer.OnLobbyJoinCompleted -= this.PlayFabMultiplayer_OnLobbyJoinCompleted;
	}

	#region Create

	public void CreateLobby(string name, uint maxPlayers){
		addLobbyForm.SetBusy(true);
		var createConfig = new LobbyCreateConfiguration()
		{
			MaxMemberCount = maxPlayers,
			OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
			AccessPolicy = LobbyAccessPolicy.Public // TODO: <-- Make this configurable.
		};

		createConfig.LobbyProperties["name"] = name;
		// createConfig.LobbyProperties["Prop2"] = "Value2";

		var joinConfig = new LobbyJoinConfiguration();
		// joinConfig.MemberProperties["MemberProp1"] = "MemberValue1";
		// joinConfig.MemberProperties["MemberProp2"] = "MemberValue2";

		PlayFabMultiplayer.CreateAndJoinLobby(
			ExtNetworkRoomPlayer.localPlayer.playerEntityKey,
			createConfig,
			joinConfig);
	}

	private void PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted(Lobby lobby, int result)
	{
		if (LobbyError.SUCCEEDED(result))
		{
			// Lobby was successfully created
			Debug.Log(lobby.ConnectionString);

			addLobbyForm.gameObject.SetActive(false);

			JoinedLobby(lobby);

			// // TODO: Shouldn't need to refresh the list here if we joined the lobby, but for the sake of testing, might as well fetch results.
			// Invoke("FindLobbies", 1);
		}
		else
		{
			// Error creating a lobby
			Debug.Log("Error creating a lobby");
			addLobbyForm.SetError("Error creating lobby");
		}
		addLobbyForm.SetBusy(false);
	}
	private void PlayFabMultiplayer_OnLobbyDisconnected(Lobby lobby)
	{
		// Disconnected from lobby
		Debug.Log("Disconnected from lobby!");
		LeftLobby(lobby);
	}

	#endregion

	#region Find
	public void FindLobbies(){
		findLobbiesButton.interactable = false;
		PlayFabMultiplayer.FindLobbies(ExtNetworkRoomPlayer.localPlayer.playerEntityKey, lobbySearchConfig);
	}

	private void PlayFabMultiplayer_OnLobbyFindLobbiesCompleted(
		IList<LobbySearchResult> searchResults,
		PFEntityKey newMember,
		int reason)
	{
		if (LobbyError.SUCCEEDED(reason))
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
				listItem.lobby = new LobbyInfo {
					id = result.LobbyId,
					currentMembers = result.CurrentMemberCount,
					maxMembers = result.MaxMemberCount,
					// name = result.SearchProperties["name"], // Not sure what SearchProperties actually is, or if this will work.
					connectionString = result.ConnectionString,
				};
				listItem.UpdateUI();
			}
		}
		else
		{
			// Error finding lobbies
			Debug.Log("Error finding lobbies");
		}
		StartCoroutine(ReenableSearchButton());
	}

	IEnumerator ReenableSearchButton(){
		yield return new WaitForSeconds(5);
		findLobbiesButton.interactable = true;
	}
	#endregion

	#region Join
	public void JoinLobby(string connectionString){
		SetLobbyListEnabled(false);
		PlayFabMultiplayer.JoinLobby(
			ExtNetworkRoomPlayer.localPlayer.playerEntityKey,
			connectionString,
			null);
	}

	private void PlayFabMultiplayer_OnLobbyJoinCompleted(Lobby lobby, PFEntityKey newMember, int reason)
	{
		if (LobbyError.SUCCEEDED(reason))
		{
			// Successfully joined a lobby
			Debug.Log("Joined a lobby");
			JoinedLobby(lobby);
		}
		else
		{
			// Error joining a lobby
			Debug.Log("Error joining a lobby");
			SetLobbyListEnabled(true);
		}
	}

	void JoinedLobby(Lobby lobby){
		Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} JoinedLobby {lobby.Id}");
		// MatchManager.instance.AddPlayerToMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
		NetworkClient.Send(new AddPlayerToMatchMessage {lobbyId = lobby.Id});
		mainLobbiesUI.SetActive(false);
		joinedLobbyUI.lobby = lobby;
		joinedLobbyUI.gameObject.SetActive(true);
	}
	void LeftLobby(Lobby lobby){
		Debug.Log($"{ExtNetworkRoomPlayer.localPlayer?.name} LeftLobby {lobby.Id}");
		NetworkClient.Send(new RemovePlayerFromMatchMessage {lobbyId = lobby.Id});
		// MatchManager.instance.RemovePlayerFromMatch(ExtNetworkRoomPlayer.localPlayer, lobby.Id);
		joinedLobbyUI.gameObject.SetActive(false);
		joinedLobbyUI.lobby = null;
		mainLobbiesUI.SetActive(true);
	}
	#endregion


	void SetLobbyListEnabled(bool val){
		foreach (var button in lobbyListPanel.GetComponentsInChildren<Selectable>()){
			button.interactable = val;
		}
	}
}
