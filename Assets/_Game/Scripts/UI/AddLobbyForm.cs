using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddLobbyForm : MonoBehaviour {
	[SerializeField] LevelSelect levelSelect;
	[SerializeField] InputFieldWrapper levelNameField;
	public void CreateLobby(){
		LobbyUtility.CreateLobby(levelNameField.text, levelSelect.GetValue().text, 5, true);
	}
}
