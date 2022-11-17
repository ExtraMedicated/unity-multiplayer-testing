using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.MultiplayerModels;
using UnityEngine;

[Serializable]
public class LobbyWrapper
{
	public const string LOBBY_NAME_SEARCH_KEY = "string_key1";
	public const string LOBBY_LEVEL_SEARCH_KEY = "string_key2";
	public const string LOBBY_VISIBILITY_SEARCH_KEY = "number_key1";
	public enum LobbyVisibility {
		Visible = 0,
		Invisible = 1
	}

	public const string LOBBY_NAME_KEY = "LobbyName";
	public const string LOBBY_LEVEL_KEY = "LevelScene";
	public const string LOBBY_SESSION_KEY = "SessionID";

	public Lobby _lobby;
	public string id;
	public string LobbyName => _lobby?.LobbyData?.GetValueOrDefault(LOBBY_NAME_KEY) ?? "Untitled";
	public string levelName => _lobby?.LobbyData?.GetValueOrDefault(LOBBY_LEVEL_KEY) ?? "_Game/Scenes/Levels/Level1.unity";
	public string connectionString;
	public uint currentMembers;
	public uint maxMembers;
	public bool isPublic;
	public bool isInProgress;
	public string lobbyOwnerId;
	public Dictionary<string, string> searchData = new Dictionary<string, string>();
	public bool IsJoinable() => currentMembers < maxMembers && !(isInProgress || _lobby?.MembershipLock == MembershipLock.Locked);
	public LobbyWrapper(){}
	public LobbyWrapper(Lobby lobby){
		_lobby = lobby;
		id = lobby.LobbyId;
		connectionString = lobby.ConnectionString;
		currentMembers = (uint)lobby.Members.Count;
		maxMembers = lobby.MaxPlayers;
		isPublic = lobby.AccessPolicy == AccessPolicy.Public;
		ExtDebug.LogJson("LobbyWrapper LobbyData: ", lobby.LobbyData);
		ExtDebug.LogJson("LobbyWrapper SearchData: ", lobby.SearchData);
		// isInProgress = lobby.isInProgress;
		lobbyOwnerId = lobby.Owner.Id;
		searchData = lobby.SearchData;
	}
}

[Serializable]
public class BasicLobbyInfo {
	public string lobbyId;
	public string lobbyOwnerId;
	public string scene;
}
