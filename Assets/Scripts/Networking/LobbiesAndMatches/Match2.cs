using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayFab.Multiplayer;
using UnityEngine;

public class Match : NetworkMatch {
	public string lobbyId;
	public List<ExtNetworkRoomPlayer> players = new List<ExtNetworkRoomPlayer>();
}
