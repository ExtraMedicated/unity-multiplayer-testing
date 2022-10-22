using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

// Some parts of this come from https://youtu.be/w0Dzb4axdcw?list=PLDI3FQoanpm1X-HQI-SVkPqJEgcRwtu7M
public class MatchManager : NetworkBehaviour {
	public static MatchManager instance;
	public readonly SyncDictionary<string, Match> matches = new SyncDictionary<string, Match>();

	public Action<Match> OnAddMatch;
	public Action<string> OnRemoveMatch;
	public Action<Match> OnUpdateMatch;
	public Action<ExtNetworkRoomPlayer, Match> OnPlayerJoinedMatch;
	public Action<ExtNetworkRoomPlayer, Match> OnPlayerLeftMatch;

	ExtNetworkRoomManager networkManager;

	void Awake(){
		if (instance == null){
			instance = this;
		} else {
			Destroy(gameObject);
		}
	}

	public override void OnStartServer(){
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		AddServerEventHandlers();
	}

	public override void OnStartClient(){
		// matches is already populated with anything the server set up
		// but we can subscribe to the callback in case it is updated later on
		matches.Callback += OnMatchListChange;
		// Process initial SyncDictionary payload
		foreach (KeyValuePair<string, Match> kvp in matches)
			OnMatchListChange(SyncDictionary<string, Match>.Operation.OP_ADD, kvp.Key, kvp.Value);
	}

	void OnDestroy(){
		if (isServer){
			RemoveServerEventHandlers();
		}
	}
	void AddServerEventHandlers(){
		Debug.Log("MatchManager.AddEventHandlers");
		NetworkServer.RegisterHandler<CreateMatchRequest>(CreateMatch);
		NetworkServer.RegisterHandler<AddPlayerToMatchRequest>(AddPlayerToMatch);
		NetworkServer.RegisterHandler<RemovePlayerFromMatchMessage>(RemovePlayerFromMatch);
		NetworkServer.RegisterHandler<BeginGameMessage>(BeginGame);
		NetworkServer.RegisterHandler<StopGameMessage>(EndGame);
		NetworkServer.RegisterHandler<FindLobbiesRequest>(GetLobbies);
		networkManager.OnPlayerRemoved += OnPlayerDisconnected;
	}

	void RemoveServerEventHandlers(){
		Debug.Log("MatchManager.RemoveEventHandlers");
		NetworkServer.UnregisterHandler<CreateMatchRequest>();
		NetworkServer.UnregisterHandler<AddPlayerToMatchRequest>();
		NetworkServer.UnregisterHandler<RemovePlayerFromMatchMessage>();
		NetworkServer.UnregisterHandler<BeginGameMessage>();
		NetworkServer.UnregisterHandler<StopGameMessage>();
		NetworkServer.UnregisterHandler<FindLobbiesRequest>();
		networkManager.OnPlayerRemoved -= OnPlayerDisconnected;
	}

	void OnMatchListChange(SyncDictionary<string, Match>.Operation op, string key, Match item){
		switch (op)
		{
			case SyncIDictionary<string, Match>.Operation.OP_ADD:
				Debug.Log("// entry added: " + key);
				OnAddMatch?.Invoke(item);
				break;
			case SyncIDictionary<string, Match>.Operation.OP_SET:
				Debug.Log("// entry changed: " + key);
				OnUpdateMatch?.Invoke(item);
				break;
				// throw new NotImplementedException();
			case SyncIDictionary<string, Match>.Operation.OP_REMOVE:
				Debug.Log("// entry removed: " + key);
				OnRemoveMatch?.Invoke(key);
				break;
			case SyncIDictionary<string, Match>.Operation.OP_CLEAR:
				Debug.Log("// Dictionary was cleared: " + key);
				throw new NotImplementedException();
				// break;
		}
	}

	public Match GetMatchById(string matchId){
		return matches[matchId];
	}

	public static string GetRandomMatchID(){
		var id = string.Empty;
		for (int i=0; i<5; i++){
			int random = UnityEngine.Random.Range(0, 36);
			if (random < 26){
				id += (char) (random + 65);
			} else {
				id += (random - 26).ToString();
			}
		}
		Debug.Log($"Match ID is {id}.");
		return id;
	}


	private void CreateMatch(NetworkConnectionToClient conn, CreateMatchRequest msg)
	{
		var match = new Match {
			isPublic = msg.isPublic,
			matchId = GetRandomMatchID(),
			players = new List<ExtNetworkRoomPlayer>(),
			maxPlayers = msg.maxPlayers,
		};
		if (AddMatch(match)){
			conn.Send(new CreateMatchResponse { match = match });
			AddPlayerToMatch(msg.networkPlayer, match.matchId);
		} else {
			conn.Send(new CreateMatchResponse { match = null });
		}
	}

	bool AddMatch(Match match){
		if (!matches.ContainsKey(match.matchId)){
			// This also triggers an update on the clients through the SyncDictionary.
			matches.Add(match.matchId, match);
			return true;
		}
		return false;
	}

	void AddPlayerToMatch(NetworkConnectionToClient conn, AddPlayerToMatchRequest msg){
		AddPlayerToMatch(conn.identity.GetComponent<ExtNetworkRoomPlayer>(), msg.lobbyId);
	}
	void AddPlayerToMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
		Debug.Log($"AddPlayerToMatch {lobbyId}");
		if (matches.ContainsKey(lobbyId)){
			var match = matches[lobbyId];
			Debug.Log($"Match: {match}");
			if (match.isInProgress){
				networkPlayer.connectionToClient.Send(new AddPlayerToMatchError{errorText = UIText.MATCH_STARTED});
			} else if (match.players.Count >= match.maxPlayers){
				networkPlayer.connectionToClient.Send(new AddPlayerToMatchError{errorText = UIText.MATCH_FULL});
			} else {
				match.players.Add(networkPlayer);
				if (match.players.Count == 1){
					// Make sure there is a lobby owner.
					match.lobbyOwnerNetId = match.players[0].netId;
				}
				networkPlayer.matchId = lobbyId;
				RpcPlayerJoined(networkPlayer, match);
			}
			// foreach (var p in match.players){
			// 	p.connectionToClient.Send(new AddedPlayerToMatchMessage {match = match, networkPlayer = networkPlayer} );
			// }
		} else {
			// networkPlayer.connectionToClient.Send(new AddedPlayerToMatchMessage {match = null} );
			networkPlayer.connectionToClient.Send(new AddPlayerToMatchError{errorText = UIText.MATCH_NOT_FOUND});
		}

	}

	[ClientRpc]
	void RpcPlayerJoined(ExtNetworkRoomPlayer networkPlayer, Match match){
		OnPlayerJoinedMatch?.Invoke(networkPlayer, match);
	}
	[ClientRpc]
	void RpcPlayerLeft(ExtNetworkRoomPlayer networkPlayer, Match match){
		OnPlayerLeftMatch?.Invoke(networkPlayer, match);
	}

	public void RemovePlayerFromMatch(NetworkConnectionToClient conn, RemovePlayerFromMatchMessage msg){
		RemovePlayerFromMatch(msg.networkPlayer, msg.matchId);
	}

	public void RemovePlayerFromMatch(ExtNetworkRoomPlayer networkRoomPlayer, string matchId){
		Debug.Log("RemovePlayerFromMatch");
		if (networkRoomPlayer.matchId != matchId){
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID mismatch.");
		}
		if (matches.ContainsKey(networkRoomPlayer.matchId)){
			var match = matches[networkRoomPlayer.matchId];
			// var temp = match.players.Select(p => p.connectionToClient);
			if (networkRoomPlayer.gamePlayer != null){
				Destroy(networkRoomPlayer.gamePlayer.gameObject);
			}
			if (match.players.Remove(networkRoomPlayer)){
				Debug.Log("<color=green>Successfully removed player</color>");
			}

			if (match.players.Count > 0){
				// Make sure there is a lobby owner.
				match.lobbyOwnerNetId = match.players[0].netId;
			}

			networkRoomPlayer.matchId = null;

			RpcPlayerLeft(networkRoomPlayer, match);
			// matches[matchId] = match; //This triggers the sync, but I think I'd rather have the RPC take care of this for greater control.
			if (match.players.Count == 0){
				Debug.Log("Removing lobby from list");
				RemoveMatch(match);
			}
		} else {
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID ({networkRoomPlayer.matchId}) not found.");
		}
	}

	private void RemoveMatch(Match match){
		CleanupInProgressMatch(match, true);
	}
	private void CleanupInProgressMatch(Match match, bool isRemovingMatch){
		// If the match ends, should it unload the scene?
		// What if other matches use the same scene?
		if (match.isInProgress){
			match.isInProgress = false;
			SceneManager.UnloadSceneAsync(match.level);
		}
		if (isRemovingMatch){
			matches.Remove(match.matchId);
		} else {
			matches[match.matchId] = match;
		}
	}

	private void OnPlayerDisconnected(ExtNetworkRoomPlayer roomPlayer)
	{
		if (roomPlayer != null && !string.IsNullOrEmpty(roomPlayer.matchId)){
			Debug.Log($"Try to remove player from match {roomPlayer.matchId}");
			RemovePlayerFromMatch(roomPlayer, roomPlayer.matchId);
		}
	}

	bool gameSceneLoaded;
	void BeginGame(NetworkConnectionToClient conn, BeginGameMessage msg){
		StartCoroutine(BeginGameEnumerator(conn.identity.netId, msg));
	}

	IEnumerator BeginGameEnumerator(uint sendersNetId, BeginGameMessage msg){
		if (matches.ContainsKey(msg.matchId)){
			var match = matches[msg.matchId];
			// Prevent anyone except the lobby owner from starting the match.
			if (sendersNetId != match.lobbyOwnerNetId) {
				yield break;
			}
			if (!gameSceneLoaded){
				// Might need to prevent something in case two different games are started at the same time.
				// Not yet sure what, though.
				yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
				gameSceneLoaded = true;
			}

			yield return SceneManager.LoadSceneAsync(match.level, LoadSceneMode.Additive);
			// Set match to in-progress.
			match.isInProgress = true;

			// Tell each of the players in the match to call the start game function.
			// I'm guessing the players then need to tell the server when they are ready?
			foreach(var p in matches[msg.matchId].players){
				p.TargetBeginGame();
			}
			matches[msg.matchId] = match;
		}
	}

	void EndGame(NetworkConnectionToClient conn, StopGameMessage msg){
		Debug.Log("Got StopGameMessage " + msg.matchId);
		if (matches.ContainsKey(msg.matchId)){
			var match = matches[msg.matchId];
			// Prevent anyone except the lobby owner from ending the match.
			// TODO: Will this still work after the lobby owner disconnects? Need to test.
			if (msg.netId != match.lobbyOwnerNetId) {
				return;
			}
			// Tell each of the players in the match to call the stop game function.
			foreach(var p in matches[msg.matchId].players){
				p.TriggerStopGameFromServer();
			}
			SceneManager.UnloadSceneAsync(match.level);
			CleanupInProgressMatch(match, false);
		}
	}

	public void GetLobbies(NetworkConnectionToClient conn, FindLobbiesRequest msg){
		conn.Send(new FindLobbiesResponse{ matches = matches.Values.ToList() } );
	}
}

public static class MatchExtensions {
	public static Guid ToGuid(this string id){
		MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
		byte[] inputBytes = Encoding.Default.GetBytes(id);
		byte[] hashBytes = provider.ComputeHash(inputBytes);
		return new Guid(hashBytes);
	}
}


