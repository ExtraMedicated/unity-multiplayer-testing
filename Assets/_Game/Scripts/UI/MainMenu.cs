using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

	ExtNetworkRoomManager networkManager;
	TransportWrapper transport;
	MultiplayerMenu mm;

	void Start(){
		mm = FindObjectOfType<MultiplayerMenu>(true);
		networkManager = GameObject.FindObjectOfType<ExtNetworkRoomManager>(true);
		transport = networkManager.GetComponent<TransportWrapper>();
		// Return to multiplayer menu if already logged in.
		if (PlayerEntity.LocalPlayer?.HasSession ?? false){
			mm?.gameObject.SetActive(true);
		}
	}

	public void OnClickSinglePlayer(){
		networkManager.gameMode = ExtNetworkRoomManager.GameMode.SinglePlayer;
		networkManager.authenticator = null;
		networkManager.networkAddress = "localhost";
		transport.SetPort(7777);
		networkManager.onlineScene = GetSinglePlayerLevel();
		networkManager.maxConnections = 1;
		networkManager.StartHost();
	}

	public void OnClickMultiplayer(){
		// Make sure the authenticator is set for multiplayer. The single player button sets it to null.
		if (networkManager.authenticator == null){
			networkManager.authenticator = networkManager.GetComponent<NewNetworkAuthenticator>();
		}

		// Make sure the room scene is being used when starting multiplayer, because the single player button changes it to another scene.
		networkManager.onlineScene = networkManager.RoomScene;
		networkManager.maxConnections = Config.MAX_MULTIPLAYER_CONNECTIONS;
		networkManager.gameMode = ExtNetworkRoomManager.GameMode.Multiplayer;
		mm.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void OnClickQuit(){
		Application.Quit();
	}

	string GetSinglePlayerLevel(){
		return "Assets/_Game/Scenes/Levels/Level1.unity";
	}
}
