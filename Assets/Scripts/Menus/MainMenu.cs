using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MainMenu : MonoBehaviour {
	ExtNetworkRoomManager networkManager;
	TransportHelper transport;
	void Awake(){
		networkManager = GameObject.FindObjectOfType<ExtNetworkRoomManager>();
		transport = networkManager.GetComponent<TransportHelper>();
	}

	public void ClickedQuit(){
		Application.Quit();
	}

	public void OnClickSinglePlayer(){
		networkManager.gameMode = ExtNetworkRoomManager.GameMode.SinglePlayer;
		networkManager.authenticator = null;
		networkManager.networkAddress = "localhost";
		transport.SetPort(7777);
		networkManager.onlineScene = networkManager.GameplayScene;
		networkManager.maxConnections = 1;
		networkManager.StartHost();
	}
}
