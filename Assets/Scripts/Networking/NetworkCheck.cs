using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCheck : MonoBehaviour {
	public List<GameObject> destroyObjects;
	void Awake () {
		if (FindObjectOfType<ExtNetworkRoomManager>() == null){
			foreach (var obj in destroyObjects){
				Destroy(obj);
			}
			SceneManager.LoadScene("Offline");
		} else {
			Destroy(gameObject);
		}
	}
}
