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
	public string Username;
	public string EntityId;
	public string SessionTicket;

}

public struct AuthResponseMessage : NetworkMessage {
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

public struct AddPlayerToMatchMessage : NetworkMessage {
	// public ExtNetworkRoomPlayer networkPlayer;
	public string lobbyId;
}

public struct RemovePlayerFromMatchMessage : NetworkMessage {
	// public ExtNetworkRoomPlayer networkPlayer;
	public string lobbyId;
}
public struct BeginGameMessage : NetworkMessage {
	public string lobbyId;
}