using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Multiplayer;
using Mirror;

// From: https://www.youtube.com/watch?v=qkoZ7d6eQ8k&list=PLDI3FQoanpm1X-HQI-SVkPqJEgcRwtu7M&index=3
public class AutostartServer : MonoBehaviour
{
	NetworkManager networkManager;
	// TransportHelper transport;
	[SerializeField] GameObject agentListener;
	[SerializeField] GameObject pfEventProcessorPrefab;

	public bool forceServerMode;
	void Awake() {
		networkManager = GameObject.FindObjectOfType<NetworkManager>();
		// transport = networkManager.GetComponent<TransportHelper>();
	}
	void Start(){
		// Need to add the PlayfabMultiplayerEventProcessor if it doesn't exist. (Not 100% sure if it's needed on the client side.)
		if (FindObjectOfType<PlayfabMultiplayerEventProcessor>() == null){
			Instantiate(pfEventProcessorPrefab);
		}
		// transport.SetClientAddress(NetworkConfig.OnlineIpAddress);
		// transport.SetPort(NetworkConfig.OnlinePort);

		// Telepathy.Log.Info = (msg) => {}; // Disable logging info from telepathy transport.
		if (Application.isBatchMode || forceServerMode){
			Debug.Log("==========================================\n Server Build \n==========================================");
			// networkManager.gameMode = NetworkManagerWrapper.GameMode.Multiplayer;
			#if ENABLE_PLAYFABSERVER_API
			// if (usePlayFab){
				// Set offline scene to null so that it doesn't try to restart the server.
				// networkManager.offlineScene = null;

				// Shouldn't need to worry about setting the port for this. The agent listener should get that from PlayFabMultiplayerAgentAPI.
				agentListener.SetActive(true); // I think this is all that this script really needs to do (aside from adding PlayfabMultiplayerEventProcessor).
				// Also, don't start the server here. The agentListener handles that as well.

			// } else {
			// 	networkManager.StartServer();
			// }
			#else
			networkManager.StartServer();
			#endif
		}
	}
}
