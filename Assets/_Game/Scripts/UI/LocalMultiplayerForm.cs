using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMultiplayerForm : MonoBehaviour {
	[SerializeField] InputFieldWrapper localIPAddressField;
	[SerializeField] InputFieldWrapper localPortNumField;
	ExtNetworkRoomManager networkManager;
	TransportWrapper transportWrapper;
	MultiplayerMenu multiplayerMenu;
	void Awake(){
		multiplayerMenu = FindObjectOfType<MultiplayerMenu>();
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
		transportWrapper = networkManager.GetComponent<TransportWrapper>();
	}

	void OnEnable(){
		localIPAddressField.text = Config.Instance.localServerIP;
		localPortNumField.text = Config.Instance.localServerPort.ToString();
	}

	void UpdateTransport(){
		networkManager.networkAddress = localIPAddressField.text;
		if (ushort.TryParse(localPortNumField.text, out ushort p)){
			transportWrapper.SetPort(p);
		}
	}

	public void HostLocalServer(){
		if (multiplayerMenu.ValidateName()){
			UpdateTransport();
			OnScreenMessage.SetText("Starting host...");
			//Set PlayerEntity.LocalPlayer
			SetPlayerData();
			networkManager.StartHost();
			StartCoroutine(CheckConnectionStatus(transportWrapper.GetTimeoutMS()/1000f));
		}
	}

	public void JoinLocalServer(){
		if (multiplayerMenu.ValidateName()){
			UpdateTransport();
			OnScreenMessage.SetText("Starting client...");
			//Set PlayerEntity.LocalPlayer
			SetPlayerData();
			networkManager.StartClient();
			StartCoroutine(CheckConnectionStatus(transportWrapper.GetTimeoutMS()/1000f));
		}
	}

	void SetPlayerData(){
		PlayerEntity.SetLocalPlayer(new PlayerEntity(new PlayerInfo {
			PlayerName = PlayerPrefs.GetString(PlayerInfo.ONLINE_PLAYER_NAME_KEY),
		}));
	}

	IEnumerator CheckConnectionStatus(float time){
		yield return new WaitForSeconds(time);
		OnScreenMessage.SetText("Failed to connect to the game server.", "red");
	}

}
