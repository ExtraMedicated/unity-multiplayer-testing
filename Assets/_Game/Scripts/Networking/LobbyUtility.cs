using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.Multiplayer;
using PlayFab.MultiplayerModels;
using UnityEngine;

public class LobbyUtility : MonoBehaviour {

	static LobbyUtility instance;

	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyCreateAndJoinCompleted;
	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyDisconnected;
	public static event Action<IList<LobbySearchResult>> OnLobbyFindLobbiesCompleted;
	public static event Action<PlayFab.Multiplayer.Lobby> OnLobbyJoinCompleted;


	void Awake(){
		if (instance != null){
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	void OnEnable(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected += PlayFabMultiplayer_OnLobbyDisconnected;
		PlayFabMultiplayer.OnLobbyFindLobbiesCompleted += PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		PlayFabMultiplayer.OnLobbyJoinCompleted += PlayFabMultiplayer_OnLobbyJoinCompleted;
		PlayFabMultiplayer.OnLobbyJoinArrangedLobbyCompleted  += PlayFabMultiplayer_OnArrangedLobbyJoinCompleted;
	}
	void OnDisable(){
		PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted -= PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted;
		PlayFabMultiplayer.OnLobbyDisconnected -= PlayFabMultiplayer_OnLobbyDisconnected;
		PlayFabMultiplayer.OnLobbyFindLobbiesCompleted -= PlayFabMultiplayer_OnLobbyFindLobbiesCompleted;
		PlayFabMultiplayer.OnLobbyJoinCompleted -= PlayFabMultiplayer_OnLobbyJoinCompleted;
		PlayFabMultiplayer.OnLobbyJoinArrangedLobbyCompleted -= PlayFabMultiplayer_OnArrangedLobbyJoinCompleted;
	}


	private static void OnError(PlayFabError error, Action<string> callback)
	{
		Debug.LogError(error.ErrorMessage);
		callback.Invoke(error.ErrorMessage);
	}

	public static void FindLobbies(){
		LobbySearchConfiguration lobbySearchConfig = new LobbySearchConfiguration();
		PlayFabMultiplayer.FindLobbies(PlayerEntity.LocalPlayer.PFEntityKey, lobbySearchConfig);
	}

	public static void GetLobby(string LobbyId, Action<PlayFab.MultiplayerModels.Lobby> onSuccessCallback, Action<string> onErrorCallback){
		PlayFabMultiplayerAPI.GetLobby(
			new PlayFab.MultiplayerModels.GetLobbyRequest {LobbyId = LobbyId},
			r => OnGetLobby(r, onSuccessCallback),
			e => OnError(e, onErrorCallback)
		);
	}


	private static void OnGetLobby(GetLobbyResult lobbyResult, Action<PlayFab.MultiplayerModels.Lobby> callback)
	{
		Debug.Log("OnGetLobby");
		callback.Invoke(lobbyResult.Lobby);
	}

	public static void LeaveLobby(string lobbyId, EntityKey member, Action<string> onErrorCallback){
		PlayFabMultiplayerAPI.LeaveLobby(
			new LeaveLobbyRequest {
				LobbyId = lobbyId,
				MemberEntity = member
			},
			OnLeaveLobby,
			e => OnError(e, onErrorCallback)
		);
	}

	private static void OnLeaveLobby(LobbyEmptyResult result)
	{
		Debug.Log("OnLeaveLobby");
	}

	public static void CreateLobby(string name, uint maxPlayers, bool isPublic){
		var createConfig = new LobbyCreateConfiguration()
		{
			MaxMemberCount = maxPlayers,
			OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
			AccessPolicy = isPublic ? LobbyAccessPolicy.Public : LobbyAccessPolicy.Private
		};

		createConfig.LobbyProperties[LobbyWrapper.LOBBY_NAME_KEY] = name;
		// createConfig.LobbyProperties["Prop2"] = "Value2";

		var joinConfig = new LobbyJoinConfiguration();
		joinConfig.MemberProperties[PlayerEntity.PLAYER_NAME_KEY] = PlayerEntity.LocalPlayer.name;
		// joinConfig.MemberProperties["MemberProp2"] = "MemberValue2";

		PlayFabMultiplayer.CreateAndJoinLobby(
			PlayerEntity.LocalPlayer.PFEntityKey,
			createConfig,
			joinConfig);
	}


	private void PlayFabMultiplayer_OnLobbyCreateAndJoinCompleted(PlayFab.Multiplayer.Lobby lobby, int result)
	{
		OnLobbyCreateAndJoinCompleted.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}

	private void PlayFabMultiplayer_OnLobbyJoinCompleted(PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result)
	{
		OnLobbyJoinCompleted.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}
	private void PlayFabMultiplayer_OnArrangedLobbyJoinCompleted(PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result)
	{
		OnLobbyJoinCompleted.Invoke(LobbyError.SUCCEEDED(result) ? lobby : null);
	}

	private void PlayFabMultiplayer_OnLobbyDisconnected(PlayFab.Multiplayer.Lobby lobby)
	{
		OnLobbyDisconnected.Invoke(lobby);
	}

	private void PlayFabMultiplayer_OnLobbyFindLobbiesCompleted(
		IList<LobbySearchResult> searchResults,
		PFEntityKey newMember,
		int reason)
	{
		OnLobbyFindLobbiesCompleted.Invoke(LobbyError.SUCCEEDED(reason) ? searchResults : null);
	}
}
