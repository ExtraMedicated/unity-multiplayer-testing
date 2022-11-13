using System.Collections;
using UnityEngine;
using PlayFab;
using System;
using System.Collections.Generic;
using PlayFab.MultiplayerAgent.Model;
using System.Linq;
using Mirror;

public class AgentListener : MonoBehaviour {
	public bool Debugging = true;
	#if ENABLE_PLAYFABSERVER_API
	private List<ConnectedPlayer> _connectedPlayers;
	ExtNetworkRoomManager networkManager;
	void Start () {
		networkManager = GameObject.FindObjectOfType<ExtNetworkRoomManager>();
		Debug.Log("----------------- AgentListener Start ----------------------");
		DontDestroyOnLoad(gameObject);
		_connectedPlayers = new List<ConnectedPlayer>();
		PlayFabMultiplayerAgentAPI.Start();
		PlayFabMultiplayerAgentAPI.IsDebugging = Debugging;
		PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
		PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
		PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
		PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

		networkManager.OnPlayerAdded.AddListener(OnPlayerAdded);
		networkManager.OnPlayerRemoved.AddListener(OnPlayerRemoved);

		// get the port that the server will listen to
		// We *have to* do it on process mode, since there might be more than one game server instances on the same VM and we want to avoid port collision
		// On container mode, we can omit the below code and set the port directly, since each game server instance will run on its own network namespace. However, below code will work as well
		// we have to do that on process
		var connInfo = PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo();
		// make sure the ListeningPortKey is the same as the one configured in your Build settings (either on LocalMultiplayerAgent or on MPS)
		const string ListeningPortKey = "game_port";
		var portInfo = connInfo.GamePortsConfiguration.Where(x=>x.Name == ListeningPortKey);
		if(portInfo.Count() > 0)
		{
			Debug.Log(string.Format("port with name {0} was found in GSDK Config Settings.", ListeningPortKey));
			Debug.Log($"ServerListeningPort: {portInfo.Single().ServerListeningPort}");
			Debug.Log($"ClientConnectionPort: {portInfo.Single().ClientConnectionPort}");
			var transport = GameObject.FindObjectOfType<TransportWrapper>();
			if (transport != null){
				transport.SetPort((ushort)portInfo.Single().ServerListeningPort);
			}
			Debug.Log("--- AgentListener Starting Server ---");
			networkManager.StartServer();
		}
		else
		{
			string msg = string.Format("Cannot find port with name {0} in GSDK Config Settings. Please make sure the LocalMultiplayerAgent is running and that the MultiplayerSettings.json file includes correct name as a GamePort Name.", ListeningPortKey);
			throw new Exception(msg);
		}

		StartCoroutine(ReadyForPlayers());
	}

	IEnumerator ReadyForPlayers()
	{
		yield return new WaitForSeconds(.5f);
		Debug.Log("ReadyForPlayers");
		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
	}

	private void OnServerActive()
	{
		Debug.Log("----- AgentListener OnServerActive");
	}

	private void OnPlayerRemoved(string playfabId)
	{
		Debug.Log("OnPlayerRemoved " + playfabId);
		if (!string.IsNullOrEmpty(playfabId)){
			ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
			_connectedPlayers.Remove(player);
			PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
			CheckPlayerCountToShutdown();
		}
	}

	// private void OnApplicationQuit()
	// {
	// 	NetworkServer.Shutdown();
	// }

	private void CheckPlayerCountToShutdown()
	{
		if (_connectedPlayers.Count <= 0)
		{
			Debug.Log("SHUT DOWN DUE TO NO PLAYERS");
			OnShutdown();
		}
	}

	private void OnPlayerAdded(string playfabId)
	{
		Debug.Log("OnPlayerAdded: " + playfabId);
		_connectedPlayers.Add(new ConnectedPlayer(playfabId));
		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
	}

	private void OnAgentError(string error)
	{
		Debug.Log(error);
	}

	private void OnShutdown()
	{
		Debug.Log("Server is shutting down");
		NetworkingMessages.SendShutdownMessage();
		networkManager.StopServer();
		StartCoroutine(Shutdown());
	}

	IEnumerator Shutdown()
	{
		yield return new WaitForSeconds(5f);
		Application.Quit();
	}

	private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
	{
		Debug.Log(string.Format("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString()));

		NetworkingMessages.SendMaintenanceMessage(NextScheduledMaintenanceUtc.GetValueOrDefault());
	}
	#endif
}


// using System.Collections;
// using UnityEngine;
// using PlayFab;
// using System;
// using System.Collections.Generic;
// using PlayFab.MultiplayerAgent.Model;
// using System.Linq;
// using Mirror;

// public class AgentListener : MonoBehaviour {
// 	public bool Debugging = true;
// 	#if ENABLE_PLAYFABSERVER_API
// 	private List<ConnectedPlayer> _connectedPlayers;
// 	NetworkManager networkManager;
// 	void Start () {
// 		networkManager = GameObject.FindObjectOfType<NetworkManager>();
// 		Debug.Log("----------------- AgentListener Start ----------------------");
// 		DontDestroyOnLoad(gameObject);
// 		_connectedPlayers = new List<ConnectedPlayer>();
// 		PlayFabMultiplayerAgentAPI.Start();
// 		PlayFabMultiplayerAgentAPI.IsDebugging = Debugging;
// 		PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
// 		PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
// 		PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
// 		PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

// 		networkManager.OnPlayerAdded.AddListener(OnPlayerAdded);
// 		networkManager.OnPlayerRemoved.AddListener(OnPlayerRemoved);

// 		// get the port that the server will listen to
// 		// We *have to* do it on process mode, since there might be more than one game server instances on the same VM and we want to avoid port collision
// 		// On container mode, we can omit the below code and set the port directly, since each game server instance will run on its own network namespace. However, below code will work as well
// 		// we have to do that on process
// 		var connInfo = PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo();
// 		// make sure the ListeningPortKey is the same as the one configured in your Build settings (either on LocalMultiplayerAgent or on MPS)
// 		const string ListeningPortKey = "game_port";
// 		var portInfo = connInfo.GamePortsConfiguration.Where(x=>x.Name == ListeningPortKey);
// 		if(portInfo.Count() > 0)
// 		{
// 			Debug.Log(string.Format("port with name {0} was found in GSDK Config Settings.", ListeningPortKey));
// 			Debug.Log($"ServerListeningPort: {portInfo.Single().ServerListeningPort}");
// 			Debug.Log($"ClientConnectionPort: {portInfo.Single().ClientConnectionPort}");
// 			var transport = GameObject.FindObjectOfType<TransportHelper>();
// 			if (transport != null){
// 				transport.SetPort((ushort)portInfo.Single().ServerListeningPort);
// 			}
// 			Debug.Log("--- AgentListener Starting Server ---");
// 			networkManager.StartServer();
// 		}
// 		else
// 		{
// 			string msg = string.Format("Cannot find port with name {0} in GSDK Config Settings. Please make sure the LocalMultiplayerAgent is running and that the MultiplayerSettings.json file includes correct name as a GamePort Name.", ListeningPortKey);
// 			throw new Exception(msg);
// 		}

// 		StartCoroutine(ReadyForPlayers());
// 	}

// 	IEnumerator ReadyForPlayers()
// 	{
// 		yield return new WaitForSeconds(.5f);
// 		Debug.Log("ReadyForPlayers");
// 		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
// 	}

// 	private void OnServerActive()
// 	{
// 		Debug.Log("----- AgentListener OnServerActive");
// 	}

// 	private void OnPlayerRemoved(string playfabId)
// 	{
// 		Debug.Log("OnPlayerRemoved " + playfabId);
// 		if (!string.IsNullOrEmpty(playfabId)){
// 			ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
// 			_connectedPlayers.Remove(player);
// 			PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
// 			CheckPlayerCountToShutdown();
// 		}
// 	}

// 	// private void OnApplicationQuit()
// 	// {
// 	// 	NetworkServer.Shutdown();
// 	// }

// 	private void CheckPlayerCountToShutdown()
// 	{
// 		if (_connectedPlayers.Count <= 0)
// 		{
// 			Debug.Log("SHUT DOWN DUE TO NO PLAYERS");
// 			OnShutdown();
// 		}
// 	}

// 	private void OnPlayerAdded(string playfabId)
// 	{
// 		Debug.Log("OnPlayerAdded: " + playfabId);
// 		_connectedPlayers.Add(new ConnectedPlayer(playfabId));
// 		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
// 	}

// 	private void OnAgentError(string error)
// 	{
// 		Debug.Log(error);
// 	}

// 	private void OnShutdown()
// 	{
// 		Debug.Log("Server is shutting down");
// 		foreach(var conn in networkManager.Connections)
// 		{
// 			conn.Connection.Send<ShutdownMessage>(new ShutdownMessage());
// 		}
// 		NetworkServer.Shutdown();
// 		StartCoroutine(Shutdown());
// 	}

// 	IEnumerator Shutdown()
// 	{
// 		yield return new WaitForSeconds(5f);
// 		Application.Quit();
// 	}

// 	private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
// 	{
// 		Debug.Log(string.Format("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString()));
// 		foreach (var conn in networkManager.Connections)
// 		{
// 			conn.Connection.Send<MaintenanceMessage>(new MaintenanceMessage() {
// 				ScheduledMaintenanceUTC = (DateTime)NextScheduledMaintenanceUtc
// 			});
// 		}
// 	}
// 	#endif
// }
