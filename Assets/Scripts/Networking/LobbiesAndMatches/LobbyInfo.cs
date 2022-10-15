using System;

[Serializable]
public class LobbyInfo {
	public string id;
	// public string name;
	public string levelName;
	public string connectionString;
	public uint currentMembers;
	public uint maxMembers;
	public bool isInProgress;
	public bool IsJoinable() => currentMembers < maxMembers && !isInProgress;
}
