using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.Multiplayer;
using PlayFab.MultiplayerModels;
using UnityEngine;

public class LobbyUtility : MonoBehaviour {

	const float MIN_POLLING_FREQUENCY = 6f;
	static LobbyUtility instance;
	BasicLobbyInfo currentlyJoinedLobby;

	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyCreateAndJoinCompleted;
	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyDisconnected;
	// public static event Action<IList<LobbySearchResult>> OnLobbyFindLobbiesCompleted;
	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyJoinCompleted;

	public static BasicLobbyInfo CurrentlyJoinedLobby {
		get => instance?.currentlyJoinedLobby;
		set => instance.currentlyJoinedLobby = value;
	}

	static DateTime timeOfLastSearch = DateTime.MinValue;

	void Awake(){
		// Replace old instance with a fresh new one to prevent strange errors.
		if (instance != null && instance != this){
			Destroy(instance.gameObject);
		}
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void OnEnable(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected += PlayFabMultiplayer_OnLobbyDisconnected;
		// PlayFabMultiplayer.OnLobbyFindLobbiesCompleted += PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		PlayFabMultiplayer.OnLobbyJoinCompleted += PlayFabMultiplayer_OnLobbyJoinCompleted;
		PlayFabMultiplayer.OnLobbyJoinArrangedLobbyCompleted  += PlayFabMultiplayer_OnArrangedLobbyJoinCompleted;
	}
	void OnDisable(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted -= PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected -= PlayFabMultiplayer_OnLobbyDisconnected;
		// PlayFabMultiplayer.OnLobbyFindLobbiesCompleted -= PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		PlayFabMultiplayer.OnLobbyJoinCompleted -= PlayFabMultiplayer_OnLobbyJoinCompleted;
		PlayFabMultiplayer.OnLobbyJoinArrangedLobbyCompleted -= PlayFabMultiplayer_OnArrangedLobbyJoinCompleted;
	}


	private static void OnError(PlayFabError error, Action<string> callback)
	{
		Debug.LogError(error.GenerateErrorReport());
		callback.Invoke(error.ErrorMessage);
	}

	// public static bool FindLobbies(){

	// 	if ((DateTime.Now - timeOfLastSearch).TotalSeconds > MIN_POLLING_FREQUENCY && PlayerEntity.LocalPlayer != null){
	// 		Debug.Log("------------- FindLobbies --------------");
	// 		OnScreenMessage.SetText("Finding Lobbies...");
	// 		LobbySearchConfiguration lobbySearchConfig = new LobbySearchConfiguration();
	// 		PlayFabMultiplayer.FindLobbies(PlayerEntity.LocalPlayer.entityKey, lobbySearchConfig);
	// 		timeOfLastSearch = DateTime.Now;
	// 		return true;
	// 	}
	// 	return false;
	// }

	public static bool FindLobbies(string search, Action<List<LobbySummary>> onSuccessCallback, Action<string> onErrorCallback){
		// TODO: What is there for the search field to filter on? Should I get rid of it?
		if ((DateTime.Now - timeOfLastSearch).TotalSeconds > MIN_POLLING_FREQUENCY && PlayerEntity.LocalPlayer != null){
			Debug.Log("------------- FindLobbies --------------");
			OnScreenMessage.SetText("Finding Lobbies...");
			var requestProps = new FindLobbiesRequest();
			var filters = new List<string>{
				$"{LobbyWrapper.LOBBY_VISIBILITY_SEARCH_KEY} eq {(int)LobbyWrapper.LobbyVisibility.Visible}"
			};
			if (!string.IsNullOrWhiteSpace(search)){
				filters.Add($"{LobbyWrapper.LOBBY_NAME_SEARCH_KEY} eq '{search.ToUpperInvariant()}'");
			}
			requestProps.Filter = string.Join(" and ", filters);
			PlayFabMultiplayerAPI.FindLobbies(
				requestProps,
				r => APIOnLobbyFindLobbiesCompleted(r, onSuccessCallback),
				e => Debug.LogError(e.GenerateErrorReport())//OnError(e, onErrorCallback)
			);

			timeOfLastSearch = DateTime.Now;
			return true;
		}
		return false;
	}

	public static void FindLobbyByCode(string lobbyCode, Action<List<LobbySummary>> onSuccessCallback, Action<string> onErrorCallback)
	{
		if ((DateTime.Now - timeOfLastSearch).TotalSeconds > MIN_POLLING_FREQUENCY && PlayerEntity.LocalPlayer != null){
			PlayFabMultiplayerAPI.FindLobbies(
				new FindLobbiesRequest {
					Filter = $"{LobbyWrapper.LOBBY_NAME_SEARCH_KEY} eq '{lobbyCode.ToUpperInvariant()}'",
				},
				r => APIOnLobbyFindLobbiesCompleted(r, onSuccessCallback),
				e => OnError(e, onErrorCallback)
			);
		}
	}

	private static void APIOnLobbyFindLobbiesCompleted(FindLobbiesResult result, Action<List<LobbySummary>> callback)
	{
		ExtDebug.LogJson("APIOnLobbyFindLobbiesCompleted", result);
		callback?.Invoke(result.Lobbies);
	}

	public static void GetLobby(string lobbyId, Action<PlayFab.MultiplayerModels.Lobby> onSuccessCallback, Action<string> onErrorCallback){
		PlayFabMultiplayerAPI.GetLobby(
			new PlayFab.MultiplayerModels.GetLobbyRequest {LobbyId = lobbyId},
			r => OnGetLobby(r, onSuccessCallback),
			e => OnError(e, onErrorCallback)
		);
	}


	private static void OnGetLobby(GetLobbyResult lobbyResult, Action<PlayFab.MultiplayerModels.Lobby> callback)
	{
		ExtDebug.LogJson("OnGetLobby", lobbyResult.Lobby);
		callback.Invoke(lobbyResult.Lobby);
	}

	public static void LeaveLobby(string lobbyId, EntityKey member, Action<string> onErrorCallback){
		// ExtDebug.LogJson($"LeaveLobby {lobbyId}", member);
		PlayFabMultiplayerAPI.LeaveLobby(
			new LeaveLobbyRequest {
				LobbyId = lobbyId,
				MemberEntity = member
			},
			OnLeaveLobby,
			e => OnError(e, onErrorCallback)
		);
	}

	public static void RemoveMemberFromLobby(string lobbyId, EntityKey member, Action<string> onErrorCallback){
		// ExtDebug.LogJson($"RemoveMemberFromLobby {lobbyId}", member);
		PlayFabMultiplayerAPI.RemoveMember(
			new RemoveMemberFromLobbyRequest {
				LobbyId = lobbyId,
				MemberEntity = member,
				PreventRejoin = true, // TODO: What is the proper default for this?
			},
			OnLeaveLobby,
			e => OnError(e, onErrorCallback)
		);
	}

	private static void OnLeaveLobby(LobbyEmptyResult result)
	{
		Debug.Log("OnLeaveLobby");
	}

	public static void CreateLobby(string name, string level, uint maxPlayers, bool isInvisible, bool isPublic){
		var createConfig = new LobbyCreateConfiguration()
		{
			MaxMemberCount = maxPlayers,
			OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
			AccessPolicy = isPublic ? LobbyAccessPolicy.Public : LobbyAccessPolicy.Private,
			SearchProperties = new Dictionary<string, string>{
				{LobbyWrapper.LOBBY_NAME_SEARCH_KEY, name},
				{LobbyWrapper.LOBBY_LEVEL_SEARCH_KEY, level},
				{LobbyWrapper.LOBBY_VISIBILITY_SEARCH_KEY, (isInvisible ? ((int)LobbyWrapper.LobbyVisibility.Invisible) : ((int)LobbyWrapper.LobbyVisibility.Visible)).ToString()},
			}
		};

		createConfig.LobbyProperties[LobbyWrapper.LOBBY_NAME_KEY] = name;
		createConfig.LobbyProperties[LobbyWrapper.LOBBY_LEVEL_KEY] = level;
		createConfig.LobbyProperties[LobbyWrapper.LOBBY_SESSION_KEY] = Guid.NewGuid().ToString();

		var joinConfig = new LobbyJoinConfiguration();
		joinConfig.MemberProperties[PlayerEntity.PLAYER_NAME_KEY] = PlayerEntity.LocalPlayer.name;

		OnScreenMessage.SetText($"Creating Lobby: {name}...");

		PlayFabMultiplayer.CreateAndJoinLobby(
			PlayerEntity.LocalPlayer.entityKey,
			createConfig,
			joinConfig);
	}


	private void PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted(PlayFab.Multiplayer.Lobby lobby, int result)
	{
		OnLobbyCreateAndJoinCompleted?.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}

	private void PlayFabMultiplayer_OnLobbyJoinCompleted(PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result)
	{
		OnLobbyJoinCompleted?.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}
	private void PlayFabMultiplayer_OnArrangedLobbyJoinCompleted(PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result)
	{
		OnLobbyJoinCompleted?.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}

	private void PlayFabMultiplayer_OnLobbyDisconnected(PlayFab.Multiplayer.Lobby lobby)
	{
		OnLobbyDisconnected?.Invoke(lobby);
	}

	// private void PlayFabMultiplayer_OnLobbyFindLobbiesCompleted(
	// 	IList<LobbySearchResult> searchResults,
	// 	PFEntityKey newMember,
	// 	int reason)
	// {
	// 	OnLobbyFindLobbiesCompleted?.Invoke(LobbyError.SUCCEEDED(reason) ? searchResults : null);
	// }


	public static void UpdateLobby(PlayFab.MultiplayerModels.Lobby lobby, MembershipLock membershipLock, Action callback){
		PlayFabMultiplayerAPI.UpdateLobby(
			new UpdateLobbyRequest {
				LobbyId = lobby.LobbyId,
				MembershipLock = membershipLock,
			},
			r => OnLobbyUpdated(r, callback),
			e => Debug.LogError(e.ErrorMessage)
		);
	}

	private static void OnLobbyUpdated(LobbyEmptyResult result, Action callback)
	{
		Debug.Log("OnLobbyUpdated");
		callback.Invoke();
	}
}
