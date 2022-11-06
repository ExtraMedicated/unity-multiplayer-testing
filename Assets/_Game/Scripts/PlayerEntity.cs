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
	public string lobbyId;
	public EntityKey entityKey;
	public PFEntityKey PFEntityKey; // I think this is only needed for the local player. Probably shouldn't even have it for others.

	public PlayerEntity(PFEntityKey entity, PlayerInfo playerInfo){
		this.PFEntityKey = entity;
		entityKey = new EntityKey(entity.Id, entity.Type);
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
	// public PlayerEntity(PFEntityKey entity){
	// 	this.PFEntityKey = entity;
	// 	var getRequest = new GetObjectsRequest {Entity = new PlayFab.DataModels.EntityKey { Id = entity.Id, Type = entity.Type }};
	// 	PlayFabDataAPI.GetObjects(getRequest,
	// 		result => {
	// 			var playerInfo = result.Objects["PlayerData"]?.DataObject as PlayerInfo;
	// 			if (playerInfo != null){
	// 				name = playerInfo.PlayerName;
	// 			}
	// 		},
	// 		error => {
	// 			Debug.LogError(error);
	// 			name = "(Error)";
	// 		}
	// 	);
	// }
}

public class PlayerInfo {
	public string PlayerName;
	public SetObject ToSetObject() => new SetObject { ObjectName = "PlayerData", DataObject = this };
}
