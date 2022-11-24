using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.DataModels;
// using PlayFab.Multiplayer;
using PlayFab.MultiplayerModels;
using UnityEngine;

[System.Serializable]
public class PlayerEntity {
	public const string PLAYER_NAME_KEY = "PlayerName";
	public static PlayerEntity LocalPlayer { get; private set; }
	public static PlayFabAuthenticationContext AuthContext;
	public static string EntityToken;
	public SignalRConnection pubSubConnection;

	public string name;
	public EntityKey entityKey;
	public string sessionTicket;

	public bool HasSession => !string.IsNullOrEmpty(sessionTicket);

	public PlayerEntity(){}

	// This constructor is used for local/custom multiplayer (Non-PlayFab)
	public PlayerEntity(PlayerInfo playerInfo){
		this.sessionTicket = null;
		entityKey = new EntityKey {
			Id = Guid.NewGuid().ToString(),
		};
		name = playerInfo.PlayerName;
	}

	// This constructor is used for PlayFab multiplayer
	public PlayerEntity(EntityKey entity, PlayerInfo playerInfo, string sessionTicket = null){
		this.sessionTicket = sessionTicket;
		entityKey = entity;
		name = playerInfo.PlayerName;
	}

	public static void SetLocalPlayer(PlayerEntity playerEntity){
		if (playerEntity == null){
			// Also kill the SignalR connection, I guess. Might as well.
			LocalPlayer.StopSignalR();
		}
		LocalPlayer = playerEntity;
	}

	public string GetSerializedProperties(){
		return JsonConvert.SerializeObject(new Dictionary<string, string>{
			{PLAYER_NAME_KEY, name},
		});
	}

	public void StartSignalR(){
		if (pubSubConnection == null){
			pubSubConnection = new SignalRConnection();
		}
		pubSubConnection.Start();
	}
	void StopSignalR(){
		pubSubConnection?.Stop();
	}
}

[Serializable]
public class PlayerInfo {
	public const string ONLINE_PLAYER_NAME_KEY = "multiplayer_name";
	public string PlayerName;
	public SetObject ToSetObject() => new SetObject { ObjectName = "PlayerData", DataObject = this };
}
