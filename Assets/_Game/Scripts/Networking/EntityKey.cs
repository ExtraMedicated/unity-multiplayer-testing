using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;

[Serializable]
public class EntityKey {

	public string Id;
	public string Type;

	public EntityKey(){}
	public EntityKey(string id, string type){
		Id = id;
		Type = type;
	}

	public static implicit operator EntityKey(PlayFab.MultiplayerModels.EntityKey entityKey) => new EntityKey(entityKey.Id, entityKey.Type);
	public static implicit operator PlayFab.MultiplayerModels.EntityKey(EntityKey entityKey) => new PlayFab.MultiplayerModels.EntityKey {Id = entityKey.Id, Type = entityKey.Type};
	public static implicit operator EntityKey(PlayFab.DataModels.EntityKey entityKey) => new EntityKey(entityKey.Id, entityKey.Type);
	public static implicit operator PlayFab.DataModels.EntityKey(EntityKey entityKey) => new PlayFab.DataModels.EntityKey {Id = entityKey.Id, Type = entityKey.Type};
	public static implicit operator EntityKey(PFEntityKey entityKey) => new EntityKey(entityKey.Id, entityKey.Type);
	public static implicit operator PFEntityKey(EntityKey entityKey) => new PFEntityKey(entityKey.Id, entityKey.Type);

}
