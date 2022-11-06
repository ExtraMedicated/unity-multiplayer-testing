using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityKey {

	public string Id;
	public string Type;

	public EntityKey(string id, string type){
		Id = id;
		Type = type;
	}

	// public EntityKey(PlayFab.MultiplayerModels.EntityKey entityKey) : this(entityKey.Id, entityKey.Type){}
	// public EntityKey(PlayFab.DataModels.EntityKey entityKey) : this(entityKey.Id, entityKey.Type){}

	public static implicit operator PlayFab.MultiplayerModels.EntityKey(EntityKey entityKey) => new PlayFab.MultiplayerModels.EntityKey {Id = entityKey.Id, Type = entityKey.Type};
	public static implicit operator PlayFab.DataModels.EntityKey(EntityKey entityKey) => new PlayFab.DataModels.EntityKey {Id = entityKey.Id, Type = entityKey.Type};
	public static implicit operator EntityKey(PlayFab.DataModels.EntityKey entityKey) => new EntityKey(entityKey.Id, entityKey.Type);
	public static implicit operator EntityKey(PlayFab.MultiplayerModels.EntityKey entityKey) => new EntityKey(entityKey.Id, entityKey.Type);

}
