using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.MultiplayerModels;
using UnityEngine;

[Serializable]
public class LobbyWrapper
{
	public const string LOBBY_NAME_KEY = "LobbyName";
	public Lobby _lobby;
	public string id;// => _lobby.Id;
	public string LobbyName => _lobby?.LobbyData?.GetValueOrDefault(LOBBY_NAME_KEY) ?? "Untitled";
	public string levelName;
	public string connectionString;// => _lobby.ConnectionString;
	public uint currentMembers;
	public uint maxMembers;// => _lobby.MaxMemberCount;
	public bool isPublic;
	public bool isInProgress;
	public string lobbyOwnerId;
	public bool IsJoinable() => currentMembers < maxMembers && !isInProgress;
	public LobbyWrapper(){}
	public LobbyWrapper(Lobby lobby){
		_lobby = lobby;
		id = lobby.LobbyId;
		connectionString = lobby.ConnectionString;
		currentMembers = (uint)lobby.Members.Count;
		maxMembers = lobby.MaxPlayers;
		isPublic = lobby.AccessPolicy == AccessPolicy.Public;
		ExtDebug.LogJson("LobbyWrapper LobbyData: ", lobby.LobbyData);
		// isInProgress = lobby.isInProgress;
		lobbyOwnerId = lobby.Owner.Id;
	}
}

[Serializable]
public class BasicLobbyInfo {
	public string lobbyId;
	public string lobbyOwnerId;
}
