using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour {
	public string defaultPlayerName;
	public string buildId;
	public bool forceLocalServer;
	public string localServerIP = "localhost";
	public uint localServerPort = 7777;

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
