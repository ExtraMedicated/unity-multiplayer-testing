using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayFab.Multiplayer;
using UnityEngine;

public class Match {
	public string matchId;
	public uint maxPlayers;
	public List<ExtNetworkRoomPlayer> players = new List<ExtNetworkRoomPlayer>();
	public uint lobbyOwnerNetId;
	public bool isPublic;
	public bool isInProgress;
}
