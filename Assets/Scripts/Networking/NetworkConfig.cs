using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Mirror;

public class NetworkConfig : MonoBehaviour {

	public const int MAX_MULTIPLAYER_CONNECTIONS = 20;
	public NetConfigVals[] configs;

	public int localConfigIndex;
	public int onlineConfigIndex;
	public bool loginWithPlayFab = true;

	static NetworkConfig instance;
	public static NetworkConfig Instance {
		get {
			if (instance == null){
				instance = FindObjectOfType<NetworkConfig>();
			}
			return instance;
		}
	}

	string overrideIpAddress;
	ushort? overridePort;


	public static string LocalIpAddress {
		get => !string.IsNullOrWhiteSpace(Instance.overrideIpAddress) ? Instance.overrideIpAddress : Instance.configs[Instance.localConfigIndex].IpAddress;
		set {
			// Debug.Log("Set IP: " + value);
			Instance.overrideIpAddress = value;
		}
	}
	public static ushort LocalPort {
		get => Instance.overridePort.GetValueOrDefault(Instance.configs[Instance.localConfigIndex].Port);
		set {
			// Debug.Log("Set port: " + value);
			Instance.overridePort = value;
		}
	}
	public static string OnlineIpAddress => Instance.configs[Instance.onlineConfigIndex].IpAddress;
	public static ushort OnlinePort => Instance.configs[Instance.onlineConfigIndex].Port;

	public static void UseOnlineConfig(NetworkManager networkManager, TransportHelper transport){

		networkManager.networkAddress = OnlineIpAddress;
		transport.SetPort(OnlinePort);
	}
	public static void UseLocalConfig(NetworkManager networkManager, TransportHelper transport){
		networkManager.networkAddress = LocalIpAddress;
		transport.SetPort(LocalPort);
	}

	public static void ResetOverrides(){
		Instance.overrideIpAddress = null;
		Instance.overridePort = null;
	}

	[Serializable]
	public struct NetConfigVals {
		[HideInInspector] public bool isActive;
		public string IpAddress;
		public ushort Port;
	}
}
