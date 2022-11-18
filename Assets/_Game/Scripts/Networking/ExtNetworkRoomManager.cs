using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/*
Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

// Taken from mirror examples.
public class ExtNetworkRoomManager : NetworkRoomManager {


	// public List<UnityNetworkConnection> Connections
	// {
	// 	get { return _connections; }
	// 	private set { _connections = value; }
	// }
	// private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();


	public UnityEvent<string> OnPlayerAdded;
	public UnityEvent<string> OnPlayerRemoved;

	public Action<bool> OnAllPlayersReadyStateChanged;
	public enum GameMode {
		SinglePlayer,
		Multiplayer,
	}
	public GameMode gameMode;

	public override void OnStartServer()
	{
		base.OnStartServer();
		// NetworkServer.RegisterHandler<CreateRoomPlayerMessage>(OnCreateRoomPlayer);
		// NetworkServer.RegisterHandler<CreateGamePlayerMessage>(OnCreateGamePlayer);
		NetworkServer.RegisterHandler<BeginGameMessage>(OnStartGameMessage);
	}

	/// <summary>
	/// This is called on the server when a networked scene finishes loading.
	/// </summary>
	/// <param name="sceneName">Name of the new scene.</param>
	public override void OnRoomServerSceneChanged(string sceneName)
	{
		Debug.Log("OnRoomServerSceneChanged");
		// // spawn the initial batch of powerups
		// if (sceneName == GameplayScene){
		// 	foreach (var spawner in GameObject.FindObjectsOfType<PowerupSpawner>()){
		// 		spawner.SpawnItem();
		// 	}
		// }
	}

	/// <summary>
	/// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
	/// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
	/// into the GamePlayer object as it is about to enter the Online scene.
	/// </summary>
	/// <param name="roomPlayer"></param>
	/// <param name="gamePlayer"></param>
	/// <returns>true unless some code in here decides it needs to abort the replacement</returns>
	public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
	{
		// PlayerScore playerScore = gamePlayer.GetComponent<PlayerScore>();
		// playerScore.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;

		Debug.Log($"------ OnRoomServerSceneLoadedForPlayer {SceneManager.GetActiveScene().name} {roomPlayer.name} {gamePlayer.name}");

		// SceneManager.MoveGameObjectToScene(gamePlayer, SceneManager.GetActiveScene());


		var rPlayer = roomPlayer.GetComponent<ExtNetworkRoomPlayer>();
		var gPlayer = gamePlayer.GetComponent<Player>();

		gPlayer.name = rPlayer.playerEntity.name;
		// rPlayer.gamePlayer = gPlayer; // I think this line caused an error. I forget what it was.
		gPlayer.networkRoomPlayer = rPlayer;
		return true;
	}

	private void OnStartGameMessage(NetworkConnectionToClient conn, BeginGameMessage msg)
	{
		GameplayScene = msg.scene;
		ServerChangeScene(GameplayScene);
	}

	// public override void OnRoomStopServer()
	// {
	// 	base.OnRoomStopServer();
	// }

	public override void OnRoomServerPlayersNotReady(){
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log("OnServerAddPlayer");
		base.OnServerAddPlayer(conn); // Calls OnRoomServerCreateRoomPlayer
		var roomPlayer = conn.identity.GetComponent<ExtNetworkRoomPlayer>();
		if (roomPlayer != null){
			OnPlayerAdded?.Invoke(roomPlayer.playerEntity.entityKey.Id);
		}
	}

	public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log("OnRoomServerCreateRoomPlayer");
		// Debug.Log(player.entityId, player);
		var player = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<ExtNetworkRoomPlayer>();
		player.playerEntity = conn.authenticationData as PlayerEntity;
		return player.gameObject;
	}

	void ServerRemovePlayer(string entityId){
		OnPlayerRemoved?.Invoke(entityId);
	}

	// public override void OnRoomServerConnect(NetworkConnectionToClient conn)
	// {
	// 	Debug.Log("OnRoomServerConnect " + conn.identity);
	// }

	public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
	{
		Debug.Log(gameMode + " OnRoomServerDisconnect " + conn.identity);
		// The identity might be a game player instead of a room player. Check for that.
		var p = conn.identity.GetComponent<Player>();
		if (p != null){
			Debug.Log("identity is on game player. room player is " + p.networkRoomPlayer);
			// First, switch the identity back to the room player so that it gets cleaned up.
			// NetworkServer.ReplacePlayerForConnection(conn, p.networkRoomPlayer.gameObject);
			// NetworkServer.Destroy(p.gameObject);

			// The gameMode variable might not be set on the server, but the networkRoomPlayer would be null for single player mode, so we can skip this part.
			if (p.networkRoomPlayer != null){
				ServerRemovePlayer(p.networkRoomPlayer.playerEntity.entityKey.Id);
			}
		} else {
			Debug.Log("identity is on room player");
			ServerRemovePlayer(conn.identity.GetComponent<ExtNetworkRoomPlayer>().playerEntity.entityKey.Id);
		}

	}

	public override void OnRoomClientEnter(){
		Debug.Log("OnRoomClientEnter");
	}



	/* FROM THE MIRROR EXAMPLE:
		This code below is to demonstrate how to do a Start button that only appears for the Host player
		showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
		all players are ready, but if a player cancels their ready state there's no callback to set it back to false
		Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
		Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
		is set as DontDestroyOnLoad = true.
	*/

	bool showStartButton;

	public override void OnRoomServerPlayersReady()
	{
		if (showRoomGUI){
		#if UNITY_SERVER
			// calling the base method calls ServerChangeScene as soon as all players are in Ready state.
			base.OnRoomServerPlayersReady();
		#else
			showStartButton = true;
		#endif
		}
	}

	public override void OnGUI()
	{
		base.OnGUI();

		if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
		{
			// set to false to hide it in the game scene
			showStartButton = false;

			ServerChangeScene(GameplayScene);
		}
	}
}
