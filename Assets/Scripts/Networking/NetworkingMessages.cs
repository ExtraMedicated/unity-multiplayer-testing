using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CustomGameServerMessageTypes
{
	public const short ReceiveAuthenticate = 900;
	public const short ShutdownMessage = 901;
	public const short MaintenanceMessage = 902;
}

public struct ReceiveAuthenticateMessage : NetworkMessage
{
	public string PlayFabId;
}

public struct ShutdownMessage : NetworkMessage { }


public struct MaintenanceMessage : NetworkMessage
{
	public DateTime ScheduledMaintenanceUTC;
}

public struct AuthRequestMessage : NetworkMessage {
	public string username;
	public string entityId;
	public string sessionTicket;

}

public struct AuthResponseMessage : NetworkMessage {
	public string username;
	public string sessionTicket;
	public string entityId;
}

public struct AuthErrorMessage : NetworkMessage {
	public string error;
}

public struct CreateRoomPlayerMessage : NetworkMessage {
	public string name;
	public Color color;
}
public struct CreateGamePlayerMessage : NetworkMessage {
	public string name;
	public Color color;
}

public struct CreateMatchRequest : NetworkMessage {
	public ExtNetworkRoomPlayer networkPlayer;
	// public string name;
	public uint maxPlayers;
	public bool isPublic;
}

public struct CreateMatchResponse : NetworkMessage {
	public Match match;
}
public struct AddPlayerToMatchRequest : NetworkMessage {
	// public ExtNetworkRoomPlayer networkPlayer;
	public string lobbyId;
}
// public struct AddedPlayerToMatchMessage : NetworkMessage {
// 	public ExtNetworkRoomPlayer networkPlayer;
// 	public Match match;
// }

public struct AddPlayerToMatchError : NetworkMessage {
	public string errorText;
}


public struct RemovePlayerFromMatchMessage : NetworkMessage {
	public ExtNetworkRoomPlayer networkPlayer;
	public string matchId;
}

// public struct PlayerRemovedFromMatchMessage : NetworkMessage {
// 	// public ExtNetworkRoomPlayer networkPlayer;
// 	// public string lobbyId;
// 	public uint playerNetId;
// 	public Match match;
// }

public struct FindLobbiesRequest : NetworkMessage {}

public struct FindLobbiesResponse : NetworkMessage {
	public List<Match> matches;
}



public struct BeginGameMessage : NetworkMessage {
	public string lobbyId;
}
