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

	public Action<string, ushort> _OnStartClient;
	public Action _OnClientConnect;
	public Action _OnClientDisconnect;
	public Action<Exception> _OnClientError;
	public Action _OnStopClient;
	public enum GameMode {
		SinglePlayer,
		Multiplayer,
	}
	public GameMode gameMode;
	// public GameObject matchManagerPrefab;

	// [Header("Spawner Setup")]
	// [Tooltip("Reward Prefab for the Spawner")]
	// public GameObject rewardPrefab;

	public override void OnStartServer()
	{
		base.OnStartServer();
		// NetworkServer.RegisterHandler<CreateRoomPlayerMessage>(OnCreateRoomPlayer);
		// NetworkServer.RegisterHandler<CreateGamePlayerMessage>(OnCreateGamePlayer);
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

		SceneManager.MoveGameObjectToScene(gamePlayer, SceneManager.GetActiveScene());
		// // var sessionInfo = GameObject.FindObjectOfType<SessionInfo>();

		// Debug.Log("sessionInfo.EntityId = " + sessionInfo.EntityId);
		var player = conn.identity.GetComponent<ExtNetworkRoomPlayer>();
		player.entityId = PlayerEntity.LocalPlayer.entityKey.Id;
		// player.PlayFabId = sessionInfo.EntityId;
		// player.PlayerName = sessionInfo.PlayerName;
		OnPlayerAdded?.Invoke(player.entityId);
		return true;
	}

	public override void OnRoomStopServer()
	{
		base.OnRoomStopServer();
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log("OnServerAddPlayer");
		var authInfo = conn.authenticationData as AuthenticationInfo;
		Debug.Log("EntityId: " + authInfo?.EntityId);
		base.OnServerAddPlayer(conn);
		var roomPlayer = conn.identity.GetComponent<ExtNetworkRoomPlayer>();
		roomPlayer.entityId = authInfo.EntityId;
		if (roomPlayer != null){
			// roomPlayer.playerName = authInfo.PlayerName;
			OnPlayerAdded?.Invoke(roomPlayer.entityId);
		}
	}

	public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
	{
		Debug.Log("OnRoomServerCreateRoomPlayer");
		var player = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<ExtNetworkRoomPlayer>();
		player.entityId = (conn.authenticationData as AuthenticationInfo)?.EntityId;
		Debug.Log(player.entityId, player);
		return player.gameObject;
	}

	void ServerRemovePlayer(string entityId){
		OnPlayerRemoved?.Invoke(entityId);
	}

	public override void OnRoomServerConnect(NetworkConnectionToClient conn)
	{
		// var authInfo = conn.authenticationData as AuthenticationInfo;
		// Debug.Log("OnRoomServerConnect " + conn.identity);
		// Debug.Log("OnRoomServerConnect " + authInfo?.EntityId);
	}
	public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
	{
		Debug.Log("OnRoomServerDisconnect " + conn.identity);
		// The identity might be a game player instead of a room player. Check for that.
		var p = conn.identity.GetComponent<Player>();
		if (p != null){
			Debug.Log("identity is on game player. room player is " + p.networkRoomPlayer);
			// First, switch the identity back to the room player so that it gets cleaned up.
			NetworkServer.ReplacePlayerForConnection(conn, p.networkRoomPlayer.gameObject);
			ServerRemovePlayer(p.networkRoomPlayer.entityId);
		} else {
			Debug.Log("identity is on room player");
			ServerRemovePlayer(conn.identity.GetComponent<ExtNetworkRoomPlayer>().entityId);
		}
	}

	public override void OnRoomStartClient()
	{
		var transport = GetComponent<TransportWrapper>();
		_OnStartClient?.Invoke(networkAddress, transport.GetPort());
	}

	public override void OnRoomClientConnect()
	{
		_OnClientConnect?.Invoke();
	}

	public override void OnRoomClientDisconnect()
	{
		_OnClientDisconnect?.Invoke();
	}

	public override void OnClientError(Exception exception)
	{
		_OnClientError?.Invoke(exception);
	}

	public override void OnRoomStopClient()
	{
		_OnStopClient?.Invoke();
	}

	/*
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
		// calling the base method calls ServerChangeScene as soon as all players are in Ready state.
	#if UNITY_SERVER
		base.OnRoomServerPlayersReady();
	#else
		showStartButton = true;
	#endif
	}

	public override void OnGUI()
	{
		if (showRoomGUI && gameMode == GameMode.Multiplayer){
			base.OnGUI();

			if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
			{
				// set to false to hide it in the game scene
				showStartButton = false;

				ServerChangeScene(GameplayScene);
			}
		}
	}

	[Serializable]
	public class UnityNetworkConnection
	{
		public bool IsAuthenticated;
		public string PlayFabId;
		public string LobbyId;
		public int ConnectionId;
		public NetworkConnection Connection;
	}

	[Header("Additive Scenes - First is start scene")]

	[Scene, Tooltip("Add additive scenes here.\nFirst entry will be players' start scene")]
	public string[] additiveScenes;

	// [Header("Fade Control - See child FadeCanvas")]

	// [Tooltip("Reference to FadeInOut script on child FadeCanvas")]
	// public FadeInOut fadeInOut;

	// This is set true after server loads all subscene instances
	bool subscenesLoaded;

	// This is managed in LoadAdditive, UnloadAdditive, and checked in OnClientSceneChanged
	bool isInTransition;

	#region Scene Management

	/// <summary>
	/// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
	/// </summary>
	/// <param name="sceneName">The name of the new scene.</param>
	public override void OnServerSceneChanged(string sceneName)
	{
		Debug.Log("OnServerSceneChanged " + sceneName);
		// This fires after server fully changes scenes, e.g. offline to online
		// If server has just loaded the Container (online) scene, load the subscenes on server
		if (sceneName == GameplayScene){
			StartCoroutine(ServerLoadLevel(additiveScenes[0]));// TODO: Need a way to manage which scene(s) to load.
			// StartCoroutine(ServerLoadSubScenes());
		// } else if (sceneName == RoomScene){
		// 	Debug.Log("Spawn matchManagerPrefab");
		// 	NetworkServer.Spawn(Instantiate(matchManagerPrefab));
		}
	}

	IEnumerator ServerLoadSubScenes()
	{
		foreach (string additiveScene in additiveScenes){
			Debug.Log($"LoadSceneAsync {additiveScene}");
			yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
			{
				loadSceneMode = LoadSceneMode.Additive,
				localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
			});
		}
		subscenesLoaded = true;
	}

	IEnumerator ServerLoadLevel(string additiveScene)
	{
		Debug.Log($"LoadSceneAsync {additiveScene}");
		yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Additive,
			localPhysicsMode = LocalPhysicsMode.Physics3D // change this to .Physics2D for a 2D game
		});
		// SceneManager.SetActiveScene(SceneManager.GetSceneByPath(additiveScene));
		subscenesLoaded = true;
	}



	/// <summary>
	/// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
	/// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
	/// </summary>
	/// <param name="sceneName">Name of the scene that's about to be loaded</param>
	/// <param name="sceneOperation">Scene operation that's about to happen</param>
	/// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
	public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
	{
		Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientChangeScene {sceneName} {sceneOperation}");

		if (sceneOperation == SceneOperation.UnloadAdditive)
			StartCoroutine(UnloadAdditive(sceneName));

		if (sceneOperation == SceneOperation.LoadAdditive)
			StartCoroutine(LoadAdditive(sceneName));
	}

	IEnumerator LoadAdditive(string sceneName)
	{
		isInTransition = true;

		// This will return immediately if already faded in
		// e.g. by UnloadAdditive above or by default startup state
		// yield return fadeInOut.FadeIn();

		// host client is on server...don't load the additive scene again
		if (mode == NetworkManagerMode.ClientOnly)
		{
			// Start loading the additive subscene
			loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
				yield return null;

			// TODO: This didn't seem to work, or it didn't get called. Not sure if it even belongs here.
			if (ExtNetworkRoomPlayer.localPlayer?.gamePlayer != null){
				SceneManager.MoveGameObjectToScene(ExtNetworkRoomPlayer.localPlayer.gamePlayer.gameObject, SceneManager.GetSceneByPath(sceneName));
			}
		}

		// Reset these to false when ready to proceed
		NetworkClient.isLoadingScene = false;
		isInTransition = false;

		OnClientSceneChanged();

		// yield return fadeInOut.FadeOut();
	}

	IEnumerator UnloadAdditive(string sceneName)
	{
		isInTransition = true;

		// This will return immediately if already faded in
		// e.g. by LoadAdditive above or by default startup state
		// yield return fadeInOut.FadeIn();

		if (mode == NetworkManagerMode.ClientOnly)
		{
			yield return SceneManager.UnloadSceneAsync(sceneName);
			yield return Resources.UnloadUnusedAssets();
		}

		// Reset these to false when ready to proceed
		NetworkClient.isLoadingScene = false;
		isInTransition = false;

		OnClientSceneChanged();

		// There is no call to FadeOut here on purpose.
		// Expectation is that a LoadAdditive will follow
		// that will call FadeOut after that scene loads.
	}

	/// <summary>
	/// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
	/// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
	/// </summary>
	/// <param name="conn">The network connection that the scene change message arrived on.</param>
	public override void OnClientSceneChanged()
	{
		Debug.Log($"{System.DateTime.Now:HH:mm:ss:fff} OnClientSceneChanged {isInTransition}");

		// Only call the base method if not in a transition.
		// This will be called from DoTransition after setting doingTransition to false
		// but will also be called first by Mirror when the scene loading finishes.
		if (!isInTransition){
			base.OnClientSceneChanged();
			// SceneManager.MoveGameObjectToScene(NetworkClient.connection.identity, ????????????????????)
		}
	}

	#endregion

	#region Server System Callbacks

	/// <summary>
	/// Called on the server when a client is ready.
	/// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
	/// </summary>
	/// <param name="conn">Connection from client.</param>
	public override void OnServerReady(NetworkConnectionToClient conn)
	{
		Debug.Log($"OnServerReady {conn} {conn.identity}");

		// This fires from a Ready message client sends to server after loading the online scene
		base.OnServerReady(conn);

		// if (conn.identity == null)
		// 	StartCoroutine(AddPlayerDelayed(conn));
	}

	// // This delay is mostly for the host player that loads too fast for the
	// // server to have subscenes async loaded from OnServerSceneChanged ahead of it.
	// IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
	// {
	// 	// Wait for server to async load all subscenes for game instances
	// 	while (!subscenesLoaded)
	// 		yield return null;

	// 	// Send Scene msg to client telling it to load the first additive scene
	// 	conn.Send(new SceneMessage { sceneName = additiveScenes[0], sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

	// 	// We have Network Start Positions in first additive scene...pick one
	// 	Transform start = GetStartPosition();

	// 	// Instantiate player as child of start position - this will place it in the additive scene
	// 	// This also lets player object "inherit" pos and rot from start position transform
	// 	GameObject player = Instantiate(playerPrefab, start);
	// 	// now set parent null to get it out from under the Start Position object
	// 	player.transform.SetParent(null);

	// 	// Wait for end of frame before adding the player to ensure Scene Message goes first
	// 	yield return new WaitForEndOfFrame();

	// 	// Finally spawn the player object for this connection
	// 	if (conn.identity == null){
	// 		Debug.Log("AddPlayerForConnection " + conn.identity);
	// 		NetworkServer.AddPlayerForConnection(conn, player);
	// 	} else {
	// 		Debug.Log("ReplacePlayerForConnection " + conn.identity);
	// 		var oldPlayer = conn.identity.gameObject;
	// 		NetworkServer.ReplacePlayerForConnection(conn, player);
	// 		yield return new WaitForEndOfFrame();
	// 		Destroy(oldPlayer);
	// 	}
	// }

	// void OnCreateGamePlayer(NetworkConnectionToClient conn, CreateGamePlayerMessage msg){
	// 	Debug.Log("OnCreateGamePlayer");
	// 	// We have Network Start Positions in first additive scene...pick one
	// 	var roomPlayer = conn.identity.GetComponent<ExtNetworkRoomPlayer>();
	// 	// var scene = SceneManager.GetSceneByName(MatchManager.instance.GetMatchById(roomPlayer.matchId).level);
	// 	var scene = SceneManager.GetSceneByName(GameplayScene);
	// 	conn.Send(new SceneMessage { sceneName = scene.path, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
	// 	Transform start = GetStartPosition();
	// 	// Instantiate player as child of start position - this will place it in the additive scene
	// 	// This also lets player object "inherit" pos and rot from start position transform
	// 	var player = Instantiate(playerPrefab, start).GetComponent<Player>();
	// 	// now set parent null to get it out from under the Start Position object
	// 	player.transform.SetParent(null);
	// 	player.SetColor(msg.color);
	// 	player.gameObject.name += msg.name;
	// 	// // Wait for end of frame before adding the player to ensure Scene Message goes first
	// 	// yield return new WaitForEndOfFrame();
	// 	roomPlayer.gamePlayer = player;

	// 	// if (conn.identity == null){
	// 	// 	// TODO: Might need to be able to create a networkPlayer if this is needed.
	// 	// 	Debug.Log("AddPlayerForConnection " + conn.identity);
	// 	// 	NetworkServer.AddPlayerForConnection(conn, player.gameObject);
	// 	// } else {
	// 		player.networkRoomPlayer = roomPlayer;
	// 		Debug.Log("ReplacePlayerForConnection " + conn.identity);
	// 		// var oldPlayer = conn.identity.gameObject;
	// 		NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
	// 		SceneManager.MoveGameObjectToScene(player.gameObject, scene);
	// 		// yield return new WaitForEndOfFrame();
	// 		// Destroy(oldPlayer);
	// 	// }
	// }

	#endregion
}
