using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

	void Start(){
		// Return to multiplayer menu if already logged in.
		if (PlayerEntity.LocalPlayer?.HasSession ?? false){
			var mm = FindObjectOfType<MultiplayerMenu>(true);
			// A bit of an extra step, going through the multiplayer menu, but could be useful if there is ever any extra stuff added to the ViewLobbies function.
			mm?.gameObject.SetActive(true);
		}
	}

	public void OnClickQuit(){
		Application.Quit();
	}
}
