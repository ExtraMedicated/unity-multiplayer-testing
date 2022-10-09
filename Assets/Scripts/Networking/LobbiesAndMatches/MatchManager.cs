// using System.Collections;
// using System.Collections.Generic;
// using Mirror;
// using PlayFab.Multiplayer;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MatchManager : NetworkBehaviour {
// 	public static MatchManager instance;

// 	public readonly SyncList<Match> matches = new SyncList<Match>();
// 	public readonly SyncList<string> lobbyIDs = new SyncList<string>();

// 	ExtNetworkRoomManager networkManager;

// 	void Start(){
// 		if (instance == null){
// 			instance = this;
// 		} else {
// 			Destroy(gameObject);
// 		}
// 		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
// 		AddEventHandlers();
// 	}

// 	void OnDestroy(){
// 		RemoveEventHandlers();
// 	}

// 	void AddEventHandlers(){
// 		Debug.Log("MatchManager.AddEventHandlers");
// 		NetworkServer.RegisterHandler<AddPlayerToMatchRequest>(AddPlayerToMatch);
// 		NetworkServer.RegisterHandler<RemovePlayerFromMatchMessage>(RemovePlayerFromMatch);
// 		NetworkServer.RegisterHandler<BeginGameMessage>(BeginGame);
// 	}
// 	void RemoveEventHandlers(){
// 		Debug.Log("MatchManager.RemoveEventHandlers");
// 		NetworkServer.UnregisterHandler<AddPlayerToMatchRequest>();
// 		NetworkServer.UnregisterHandler<RemovePlayerFromMatchMessage>();
// 		NetworkServer.UnregisterHandler<BeginGameMessage>();
// 	}

// 	// To be called from the success callback for joining a PlayFab lobby.
// 	void AddPlayerToMatch(NetworkConnectionToClient conn, AddPlayerToMatchRequest msg){
// 		AddPlayerToMatch(conn.identity.GetComponent<ExtNetworkRoomPlayer>(), msg.lobbyId);
// 	}
// 	void AddPlayerToMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
// 		Debug.Log("AddPlayerToMatch");
// 		if (!lobbyIDs.Contains(lobbyId)){
// 			matches.Add(new Match{
// 				matchId = lobbyId,
// 				players = new List<ExtNetworkRoomPlayer>{
// 					networkPlayer
// 				}
// 			});
// 			lobbyIDs.Add(lobbyId);
// 		} else {
// 			matches[lobbyIDs.IndexOf(lobbyId)].players.Add(networkPlayer);
// 		}
// 		networkPlayer.matchId = lobbyId;
// 	}

// 	// To be called from the success callback for leaving a PlayFab lobby.
// 	public void RemovePlayerFromMatch(NetworkConnectionToClient conn, RemovePlayerFromMatchMessage msg){
// 		RemovePlayerFromMatch(conn.identity.GetComponent<ExtNetworkRoomPlayer>(), msg.matchId);
// 	}
// 	void RemovePlayerFromMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
// 		Debug.Log("RemovePlayerFromMatch");
// 		if (networkPlayer.matchId != lobbyId){
// 			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID mismatch.");
// 		}
// 		if (lobbyIDs.Contains(networkPlayer.matchId)){
// 			var index = lobbyIDs.IndexOf(networkPlayer.matchId);
// 			matches[index].players.Remove(networkPlayer);
// 			if (matches[index].players.Count == 0){
// 				Debug.Log("Removing lobby from list");
// 				lobbyIDs.RemoveAt(index);
// 				matches.RemoveAt(index);
// 			}
// 			networkPlayer.matchId = null;
// 		} else {
// 			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID ({networkPlayer.matchId}) not found.");
// 		}
// 	}

// 	public void BeginGame(NetworkConnectionToClient conn, BeginGameMessage msg){
// 		Camera.main.gameObject.SetActive(false);
// 		SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive).completed += (h) => {
// 			Debug.Log("---- Game scene loaded ----");
// 			for (int i = 0; i < matches.Count; i++)
// 			{
// 				if (matches[i].matchId == msg.lobbyId){
// 					// Tell each of the players in the match to call the start game function.
// 					foreach(var p in matches[i].players){
// 						SceneManager.MoveGameObjectToScene(p.gameObject, SceneManager.GetSceneByName("Game"));
// 						var player = p.GetComponent<ExtNetworkRoomPlayer>();
// 						// turnManager.AddPlayer(player);
// 						player.StartGame();
// 					}
// 					break;
// 				}
// 			}
// 		};
// 	}


// }
