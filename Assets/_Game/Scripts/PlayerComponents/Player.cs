
using System;
using Cinemachine;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour
{
	Rigidbody _rigidbody;
	[SerializeField] CinemachineVirtualCamera virtualCamera;
	[SyncVar] internal ExtNetworkRoomPlayer networkRoomPlayer;
	internal ExtNetworkRoomManager networkManager;
	CameraMovement cameraMovement;

	[SyncVar] public float fuel = 100;

	PlayerControls _controls;
	PlayerControls controls {
		get {
			if (_controls == null){
				_controls = new PlayerControls();
			}
			return _controls;
		}
	}

	enum Side {
		Left,
		Right,
		Center
	}
	bool reverseControls;
	public bool isUpBtnPressed;

	bool inputChanged;
	float centerX, centerY;

	void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		centerX = Screen.width/2;
		centerY = Screen.height/2;
		networkManager = FindObjectOfType<ExtNetworkRoomManager>();
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		// cameraMovement = GetComponentInChildren<CameraMovement>(true);
		// cameraMovement.EnablePlayerCamera();
		virtualCamera.gameObject.SetActive(true);
		controls.Enable();
		controls.Player.Up.started += UpBtnDown;
		controls.Player.Up.canceled += UpBtnUp;
	}

	public override void OnStopClient()
	{
		// cameraMovement.gameObject.SetActive(false);
		virtualCamera.gameObject.SetActive(false);
		controls.Disable();
		controls.Player.Up.started -= UpBtnDown;
		controls.Player.Up.canceled -= UpBtnUp;
		base.OnStopClient();
	}

	public override void OnStartLocalPlayer()
	{
		_rigidbody.isKinematic = false;
		transform.rotation = Quaternion.identity;
		base.OnStartLocalPlayer();
	}

	void UpBtnDown(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => UpBtnDown();
	void UpBtnDown(){
		isUpBtnPressed = true;
		inputChanged = true;
	}
	void UpBtnUp(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => UpBtnUp();
	void UpBtnUp(){
		isUpBtnPressed = false;
		inputChanged = true;
	}

	internal void SetColor(Color color)
	{
		Debug.Log($"Set Color {color}");
		var mrs = GetComponentsInChildren<MeshRenderer>();
		foreach (var mr in mrs){
			mr.material.color = color;
		}
	}

	void Update()
	{
		if (!NetworkClient.isConnected || !isLocalPlayer)
			return;

		if (hasAuthority && Input.GetKeyDown(KeyCode.Escape)){
			if (networkManager.gameMode == ExtNetworkRoomManager.GameMode.Multiplayer){
				// Return to lobby.
				if (NetworkClient.isHostClient || (LobbyUtility.CurrentlyJoinedLobby != null && LobbyUtility.CurrentlyJoinedLobby.lobbyOwnerId == PlayerEntity.LocalPlayer.entityKey.Id)){
					LetsAllGoToTheLobby();
				}
			} else if (NetworkClient.isHostClient) {
				// Stop single player game.
				networkManager.StopHost();
			} else {
				// Rage quit.
				networkManager.StopClient();
			}
			return;
		}

		HandleInput();
	}

	[Command]
	void LetsAllGoToTheLobby(){
		networkManager.ServerChangeScene(networkManager.RoomScene);
	}

	void HandleInput(){
		if (inputChanged){
			if (isUpBtnPressed){
				CmdJump();
			}
		}
		inputChanged = false;
	}

	bool moveUp;
	void FixedUpdate()
	{
		if (!isLocalPlayer)
			return;
	}

	[Command]
	void CmdJump(){
		if (fuel > 1){
			fuel -= 1;
			_rigidbody.AddForce(transform.up * 5, ForceMode.Impulse);
		}
	}

	[Server]
	public void AddFuel(float amount){
		Debug.Log("AddFuel: " + amount);
		fuel += amount;
	}

	[Server]
	public void BurnFuel(float amount){
		fuel = Mathf.Max(fuel - amount, 0);
	}
}

