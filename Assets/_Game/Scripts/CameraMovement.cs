using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {
	const float CAMERA_Z_OFFSET = -30;
	public GameObject player;
	Rigidbody playerRb;
	// public GameObject sceneController;
	public GameObject target;
	//public GUIText targetDistText;
	private Vector3 targetPosition;
	private Camera playerCamera;

	public void EnablePlayerCamera(){
		playerCamera = Camera.main;
		targetPosition = new Vector3();
		playerRb = player.GetComponent<Rigidbody>();
		playerCamera.transform.SetParent(transform);
		playerCamera.transform.localPosition = new Vector3(0, 0, CAMERA_Z_OFFSET);
		playerCamera.transform.localRotation = Quaternion.identity;
		gameObject.SetActive(true);
	}

	void OnDisable(){
		playerCamera.transform.SetParent(null);
	}

	void Update () {
		// if (!sceneControllerScript.isGameover){
			transform.position = player.transform.position + playerRb.centerOfMass;
			//Quaternion targetRot = Vector3.RotateTowards(
			//playerCamera.transform.LookAt(player.transform.position + player.rigidbody.velocity/3);
			Vector3 delta = playerRb.velocity/3;
			delta.x = Mathf.Clamp(delta.x, -20, 20);
			delta.y = Mathf.Clamp(delta.y, -20, 20);
			delta.z = Mathf.Clamp(delta.z, -20, 20);
			targetPosition = player.transform.position + delta;
			target.transform.position = Vector3.Lerp (target.transform.position, targetPosition, 3*Time.deltaTime);
			//targetPosition = Vector3.Lerp (targetPosition, player.transform.position + delta, 3*Time.deltaTime);
			//targetDistText.text = "" + Vector3.Distance(player.transform.position, targetPosition);
			playerCamera.transform.LookAt(target.transform.position);
//			playerCamera.transform.LookAt(targetPosition);
		// }
	}
}
