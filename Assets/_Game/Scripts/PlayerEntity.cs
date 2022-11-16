using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.Multiplayer;
using PlayFab.MultiplayerModels;
using UnityEngine;

[System.Serializable]
public class PlayerEntity {
	public const string PLAYER_NAME_KEY = "PlayerName";
	public static PlayerEntity LocalPlayer;

	public string name;
	public EntityKey entityKey;
	public string sessionTicket;

	public bool HasSession => !string.IsNullOrEmpty(sessionTicket);

	public PlayerEntity(){}

	public PlayerEntity(EntityKey entity, PlayerInfo playerInfo, string sessionTicket = null){
		this.sessionTicket = sessionTicket;
		entityKey = entity;
		name = playerInfo.PlayerName;
	}
	public PlayerEntity(Member member){
		entityKey = member.MemberEntity;
		member.MemberData.TryGetValue(PLAYER_NAME_KEY, out name);
	}

	public string GetSerializedProperties(){
		return JsonConvert.SerializeObject(new Dictionary<string, string>{
			{PLAYER_NAME_KEY, name},
		});
	}
}

[Serializable]
public class PlayerInfo {
	public string PlayerName;
	public SetObject ToSetObject() => new SetObject { ObjectName = "PlayerData", DataObject = this };
}

