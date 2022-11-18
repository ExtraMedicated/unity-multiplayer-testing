using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Config : MonoBehaviour {
	public const int MAX_MULTIPLAYER_CONNECTIONS = 32;
	[SerializeField] string defaultPlayerName;
	public bool addCloneNumber;
	public string buildId;
	public string buildAliasId;
	public bool forceLocalServer;
	public string localServerIP = "localhost";
	public uint localServerPort = 7777;
	public bool HasDefaultPlayerName => !string.IsNullOrWhiteSpace(defaultPlayerName?.Trim());
	public string GetDefaultPlayerName(){
		if (HasDefaultPlayerName){
			#if UNITY_EDITOR
			if (addCloneNumber){
				var dir = Path.GetDirectoryName(Application.dataPath);
				if (dir.Contains("_clone_")){
					var num = dir.Split("_clone_")[1];
					return defaultPlayerName.Trim() + num;
				}
			}
			#endif
			return defaultPlayerName.Trim();
		}
		return null;
	}

	static Config instance;
	public static Config Instance {
		get {
			if (instance == null){
				instance = FindObjectOfType<Config>();
			}
			return instance;
		}
	}

}
