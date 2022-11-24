using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class AddLobbyForm : MonoBehaviour {
	[SerializeField] LevelSelect levelSelect;
	[SerializeField] InputFieldWrapper levelNameField;
	[SerializeField] ToggleFieldWrapper privacyToggle;
	[SerializeField] SliderFieldWrapper playerCountSlider;

	LobbiesUI lobbiesUI;
	void Start(){
		lobbiesUI = FindObjectOfType<LobbiesUI>(true);
	}

	public void CreateLobby(){
		// TODO: Not using the ACTUAL private lobby setting for now. It would require using invites.
		// Instead, just set a custom property to filter these out of the search results.
		lobbiesUI.CreateAndJoinLobby(GetRandomMatchID(), levelSelect.GetRawValue().text, (uint)playerCountSlider.value, privacyToggle.value, true);
	}


	// From (I think): https://youtu.be/w0Dzb4axdcw?list=PLDI3FQoanpm1X-HQI-SVkPqJEgcRwtu7M
	public static string GetRandomMatchID(){
		var id = string.Empty;
		for (int i=0; i<5; i++){
			int random = UnityEngine.Random.Range(0, 36);
			if (random < 26){
				id += (char) (random + 65);
			} else {
				id += (random - 26).ToString();
			}
		}
		Debug.Log($"Match ID is {id}.");
		return id;
	}
}
public static class LobbyExtensions {
	public static Guid ToGuid(this string id){
		MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
		byte[] inputBytes = Encoding.Default.GetBytes(id);
		byte[] hashBytes = provider.ComputeHash(inputBytes);
		return new Guid(hashBytes);
	}
}
