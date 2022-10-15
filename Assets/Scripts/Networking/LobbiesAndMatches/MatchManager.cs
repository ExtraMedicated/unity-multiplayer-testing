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

public class MatchManager : NetworkBehaviour {
	public static MatchManager instance;

	// List<Match> matches = new List<Match>();
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

	void AddServerEventHandlers(){
		Debug.Log("MatchManager.AddEventHandlers");
		NetworkServer.RegisterHandler<CreateMatchRequest>(CreateMatch);
		NetworkServer.RegisterHandler<AddPlayerToMatchRequest>(AddPlayerToMatch);
		NetworkServer.RegisterHandler<RemovePlayerFromMatchMessage>(RemovePlayerFromMatch);
		NetworkServer.RegisterHandler<BeginGameMessage>(BeginGame);
		NetworkServer.RegisterHandler<FindLobbiesRequest>(GetLobbies);
		networkManager.OnPlayerRemoved += OnPlayerDisconnected;
	}

	void RemoveServerEventHandlers(){
		Debug.Log("MatchManager.RemoveEventHandlers");
		NetworkServer.UnregisterHandler<CreateMatchRequest>();
		NetworkServer.UnregisterHandler<AddPlayerToMatchRequest>();
		NetworkServer.UnregisterHandler<RemovePlayerFromMatchMessage>();
		NetworkServer.UnregisterHandler<BeginGameMessage>();
		NetworkServer.UnregisterHandler<FindLobbiesRequest>();
		networkManager.OnPlayerRemoved -= OnPlayerDisconnected;
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

	public void RemovePlayerFromMatch(ExtNetworkRoomPlayer networkPlayer, string matchId){
		Debug.Log("RemovePlayerFromMatch");
		if (networkPlayer.matchId != matchId){
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID mismatch.");
		}
		if (matches.ContainsKey(networkPlayer.matchId)){
			var match = matches[networkPlayer.matchId];
			// var temp = match.players.Select(p => p.connectionToClient);
			match.players.Remove(networkPlayer);

			if (match.players.Count > 0){
				// Make sure there is a lobby owner.
				match.lobbyOwnerNetId = match.players[0].netId;
			}

			networkPlayer.matchId = null;

			RpcPlayerLeft(networkPlayer, match);
			// matches[matchId] = match; // Triggers the update, but I think I'd rather have the RPC take care of this for greater control.
			if (match.players.Count == 0){
				Debug.Log("Removing lobby from list");
				matches.Remove(match.matchId);
			}
		} else {
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID ({networkPlayer.matchId}) not found.");
		}
	}

	private void OnPlayerDisconnected(ExtNetworkRoomPlayer roomPlayer)
	{
		if (!string.IsNullOrEmpty(roomPlayer.matchId)){
			Debug.Log($"Try to remove player from match {roomPlayer.matchId}");
			RemovePlayerFromMatch(roomPlayer, roomPlayer.matchId);
		}
	}

	bool gameSceneLoaded;
	public void BeginGame(NetworkConnectionToClient conn, BeginGameMessage msg){
		Camera.main.gameObject.SetActive(false);
		// Might need to prevent something in case two different games are started at the same time.
		// Not sure what yet, though.

		StartCoroutine(BeginGameEnumerator(msg));
		// SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive).completed += (h) => {
		// 	Debug.Log("---- Game scene loaded ----");
		// 	if (matches.ContainsKey(msg.lobbyId)){
		// 		var match = matches[msg.lobbyId];
		// 		// Set match to in-progress.
		// 		match.isInProgress = true;

		// 		// Tell each of the players in the match to call the start game function.
		// 		// I'm guessing the players then need to tell the server when they are ready?
		// 		foreach(var p in matches[msg.lobbyId].players){
		// 			SceneManager.MoveGameObjectToScene(p.gameObject, SceneManager.GetSceneByName("Game"));
		// 			var player = p.GetComponent<ExtNetworkRoomPlayer>();
		// 			player.StartGame();
		// 		}
		// 		matches[msg.lobbyId] = match;
		// 	}
		// };
	}

	IEnumerator BeginGameEnumerator(BeginGameMessage msg){
		if (!gameSceneLoaded){
			yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
			gameSceneLoaded = true;
		}
		if (matches.ContainsKey(msg.lobbyId)){
			var match = matches[msg.lobbyId];
			yield return SceneManager.LoadSceneAsync(match.level, LoadSceneMode.Additive);
			// Set match to in-progress.
			match.isInProgress = true;

			// Tell each of the players in the match to call the start game function.
			// I'm guessing the players then need to tell the server when they are ready?
			foreach(var p in matches[msg.lobbyId].players){
				// SceneManager.MoveGameObjectToScene(p.gameObject, SceneManager.GetSceneByName(match.level));
				var player = p.GetComponent<ExtNetworkRoomPlayer>();
				player.StartGame();
			}
			matches[msg.lobbyId] = match;
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


