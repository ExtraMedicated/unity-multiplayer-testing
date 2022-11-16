using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

// public class CustomGameServerMessageTypes
// {
// 	public const short ReceiveAuthenticate = 900;
// 	public const short ShutdownMessage = 901;
// 	public const short MaintenanceMessage = 902;
// }

public static class NetworkingMessages
{
#if DEBUG_NETWORK_MESSAGES
	const string DEBUG_LINE = "<color=green>{0} ({1})</color>";
	static void Log(string caller, string msgType){
		Debug.Log(string.Format(DEBUG_LINE, msgType, caller));
	}
	static void SendFromClient<T>(T msg, int channelId = Channels.Reliable, [CallerMemberName] string caller = "") where T : struct, NetworkMessage {
		Log(caller, msg.GetType().Name);
		NetworkClient.Send(msg, channelId);
	}
	static void SendFromServerToAll<T>(T msg, int channelId = Channels.Reliable, [CallerMemberName] string caller = "") where T : struct, NetworkMessage {
		Log(caller, msg.GetType().Name);
		NetworkServer.SendToAll(msg, channelId);
	}
	public static void SendThroughConnection<T>(NetworkConnection conn, T msg, [CallerMemberName] string caller = "") where T : struct, NetworkMessage {
		Log(caller, msg.GetType().Name);
		conn.Send(msg);
	}
#else
	static void SendFromClient<T>(T msg, int channelId = Channels.Reliable) where T : struct, NetworkMessage {
		NetworkClient.Send(msg);
	}
	static void SendFromServerToAll<T>(T msg, int channelId = Channels.Reliable) where T : struct, NetworkMessage {
		NetworkServer.SendToAll(msg);
	}
	public static void SendThroughConnection<T>(NetworkConnection conn, T msg) where T : struct, NetworkMessage {
		conn.Send(msg);
	}
#endif

	public static void SendAuthRequestMessage(){
		SendFromClient(new AuthRequestMessage {
			playerEntity = PlayerEntity.LocalPlayer,
		});
	}

	// public static void SendChangeReadyStateMessage(string entityId, bool isReady){
	// 	SendFromClient(new ChangeReadyStateMessage {
	// 		entityId = entityId,
	// 		ready = isReady,
	// 	});
	// }

	// public static void SendChangeAllPlayersReadyStateMessage(bool isReady){
	// 	SendFromServerToAll(new ChangeAllPlayersReadyStateMessage { ready = isReady});
	// }

	public static void SendBeginGameMessage(){
		SendFromClient(new BeginGameMessage {
			scene = LobbyUtility.CurrentlyJoinedLobby.scene
		});
	}

	public static void SendMaintenanceMessage(DateTime nextScheduledMaintenanceUtc){
		SendFromServerToAll(new MaintenanceMessage { ScheduledMaintenanceUTC = nextScheduledMaintenanceUtc });
	}

	public static void SendShutdownMessage(){
		SendFromServerToAll(new ShutdownMessage());
	}

	// public static void SendStopGameMessage(){
	// 	SendFromClient(new StopGameMessage(ExtNetworkRoomPlayer.localPlayer.matchId, ExtNetworkRoomPlayer.localPlayer.netId));
	// }
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
	public PlayerEntity playerEntity;

}


public struct AuthResponseMessage : NetworkMessage {
	public PlayerEntity playerEntity;
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

// public struct AddPlayerToMatchRequest : NetworkMessage {
// 	// public ExtNetworkRoomPlayer networkPlayer;
// 	public string lobbyId;
// }
// public struct AddedPlayerToMatchMessage : NetworkMessage {
// 	public ExtNetworkRoomPlayer networkPlayer;
// 	public Match match;
// }

// public struct AddPlayerToMatchError : NetworkMessage {
// 	public string errorText;
// }


// public struct RemovePlayerFromMatchMessage : NetworkMessage {
// 	public NetworkRoomPlayer networkPlayer;
// 	public string matchId;
// }

// public struct PlayerRemovedFromMatchMessage : NetworkMessage {
// 	// public ExtNetworkRoomPlayer networkPlayer;
// 	// public string lobbyId;
// 	public uint playerNetId;
// 	public Match match;
// }

// public struct FindLobbiesRequest : NetworkMessage {}

// public struct FindLobbiesResponse : NetworkMessage {
// 	public List<Match> matches;
// }

public struct BeginGameMessage : NetworkMessage {
	public string scene;
}

