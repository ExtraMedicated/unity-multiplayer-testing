using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[DisallowMultipleComponent]
public class AddLobbyForm : MonoBehaviour {
	const int MIN_PLAYERS = 2;
	const int DEFAULT_PLAYERS = 10;
	const int MAX_PLAYERS = 10;

	[SerializeField] TMP_InputField nameField;
	[SerializeField] TMP_InputField numberField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] Button submitBtn;
	[SerializeField] GameObject loadingOverlay;

	string inputText;
	int numPlayers;

	void OnEnable(){
		submitBtn.interactable = false;
		errorText.text = string.Empty;
		numPlayers = DEFAULT_PLAYERS;
		numberField.text = DEFAULT_PLAYERS.ToString();
	}

	public void OnNameChanged(string text){
		inputText = Regex.Replace(text, @"[^\u0020-\u007E]", string.Empty);
		submitBtn.interactable = !string.IsNullOrWhiteSpace(inputText);
	}

	public void NumPlayersChanged(string input){
		if (int.TryParse(input, out int num)){
			numPlayers = Mathf.Clamp(num, MIN_PLAYERS, MAX_PLAYERS);
		} else {
			numPlayers = DEFAULT_PLAYERS;
		}
		numberField.text = numPlayers.ToString();
	}

	public void CreateLobby(){
		errorText.text = string.Empty;
		LobbyManager.instance.CreateLobby(inputText, (uint)numPlayers);
	}

	public void SetBusy(bool val){
		loadingOverlay.SetActive(val);
		nameField.interactable = !val;
		numberField.interactable = !val;
		submitBtn.interactable = !val;
	}

	public void SetError(string msg){
		errorText.text = msg;
	}
}
