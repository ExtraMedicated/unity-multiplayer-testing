using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayFab.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : NetworkBehaviour {
	public static MatchManager instance;

	private List<Match> matches = new List<Match>();
	public readonly SyncList<string> lobbyIDs = new SyncList<string>();

	ExtNetworkRoomManager networkManager;

	void Start(){
		if (instance == null){
			instance = this;
		} else {
			Destroy(gameObject);
		}
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		AddEventHandlers();
	}

	void OnDestroy(){
		RemoveEventHandlers();
	}

	void AddEventHandlers(){
		Debug.Log("MatchManager.AddEventHandlers");
		NetworkServer.RegisterHandler<AddPlayerToMatchMessage>(AddPlayerToMatch);
		NetworkServer.RegisterHandler<RemovePlayerFromMatchMessage>(RemovePlayerFromMatch);
		NetworkServer.RegisterHandler<BeginGameMessage>(BeginGame);
	}
	void RemoveEventHandlers(){
		Debug.Log("MatchManager.RemoveEventHandlers");
		NetworkServer.UnregisterHandler<AddPlayerToMatchMessage>();
		NetworkServer.UnregisterHandler<RemovePlayerFromMatchMessage>();
		NetworkServer.UnregisterHandler<BeginGameMessage>();
	}

	// public void AddPlayerToMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
	// 	CmdAddPlayerToMatch(networkPlayer, lobbyId);
	// }

	// To be called from the success callback for joining a PlayFab lobby.
	void AddPlayerToMatch(NetworkConnectionToClient conn, AddPlayerToMatchMessage msg){
		AddPlayerToMatch(conn.identity.GetComponent<ExtNetworkRoomPlayer>(), msg.lobbyId);
	}
	void AddPlayerToMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
		Debug.Log("AddPlayerToMatch");
		if (!lobbyIDs.Contains(lobbyId)){
			matches.Add(new Match{
				lobbyId = lobbyId,
				players = new List<ExtNetworkRoomPlayer>{
					networkPlayer
				}
			});
			lobbyIDs.Add(lobbyId);
		} else {
			matches[lobbyIDs.IndexOf(lobbyId)].players.Add(networkPlayer);
		}
		networkPlayer.lobbyId = lobbyId;
	}

	// To be called from the success callback for leaving a PlayFab lobby.
	public void RemovePlayerFromMatch(NetworkConnectionToClient conn, RemovePlayerFromMatchMessage msg){
		RemovePlayerFromMatch(conn.identity.GetComponent<ExtNetworkRoomPlayer>(), msg.lobbyId);
	}
	void RemovePlayerFromMatch(ExtNetworkRoomPlayer networkPlayer, string lobbyId){
		Debug.Log("RemovePlayerFromMatch");
		if (networkPlayer.lobbyId != lobbyId){
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID mismatch.");
		}
		if (lobbyIDs.Contains(networkPlayer.lobbyId)){
			var index = lobbyIDs.IndexOf(networkPlayer.lobbyId);
			matches[index].players.Remove(networkPlayer);
			if (matches[index].players.Count == 0){
				Debug.Log("Removing lobby from list");
				lobbyIDs.RemoveAt(index);
				matches.RemoveAt(index);
			}
			networkPlayer.lobbyId = null;
		} else {
			Debug.LogError($"RemovePlayerFromMatch error: Lobby ID ({networkPlayer.lobbyId}) not found.");
		}
	}

	public void BeginGame(NetworkConnectionToClient conn, BeginGameMessage msg){
		// var turnManagerObj = Instantiate<GameObject>(turnManagerPrefab);
		// NetworkServer.Spawn(turnManagerObj);
		// var turnManager = turnManagerObj.GetComponent<TurnManager>();

		// var networkMatch = turnManager.GetComponent<NetworkMatch>();
		// Debug.Log("networkMatch " + networkMatch);
		// turnManager.GetComponent<NetworkMatch>().matchId = matchId.ToGuid();


		// TODO: This is crap:
		// if (!SceneManager.GetSceneByName("Game").isLoaded){

			Camera.main.gameObject.SetActive(false);
			SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive).completed += (h) => {
				Debug.Log("---- Game scene loaded ----");
				for (int i = 0; i < matches.Count; i++)
				{
					if (matches[i].lobbyId == msg.lobbyId){
						// Tell each of the players in the match to call the start game function.
						foreach(var p in matches[i].players){
							SceneManager.MoveGameObjectToScene(p.gameObject, SceneManager.GetSceneByName("Game"));
							var player = p.GetComponent<ExtNetworkRoomPlayer>();
							// turnManager.AddPlayer(player);
							player.StartGame();
						}
						break;
					}
				}
			};
		// } else {
		// 	for (int i = 0; i < matches.Count; i++)
		// 	{
		// 		if (matches[i].lobbyId == msg.lobbyId){
		// 			// Tell each of the players in the match to call the start game function.
		// 			foreach(var p in matches[i].players){
		// 				var player = p.GetComponent<ExtNetworkRoomPlayer>();
		// 				// turnManager.AddPlayer(player);
		// 				player.StartGame();
		// 			}
		// 			break;
		// 		}
		// 	}
		// }
	}


}
