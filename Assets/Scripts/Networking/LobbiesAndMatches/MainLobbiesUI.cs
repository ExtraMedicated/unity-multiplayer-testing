using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.Multiplayer;
using UnityEngine;

public class MainLobbiesUI : MonoBehaviour {
	public List<GameObject> enableUIElements;
	void OnEnable() {
		foreach(var obj in enableUIElements){
			obj.SetActive(true);
		}
		StartCoroutine(DelayedSearch());
	}
	IEnumerator DelayedSearch(){
		// Delay refresh to allow time for changes to propagate.
		yield return new WaitForSeconds(2f);
		LobbyManager.instance.FindLobbies();
	}
}
