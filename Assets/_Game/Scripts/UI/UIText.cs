using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIText : MonoBehaviour
{
	public const string MATCH_STARTED = "The match has already started.";
	public const string MATCH_FULL = "Match is full.";
	public const string MATCH_NOT_FOUND = "Match not found.";
	TMP_Text tComp;
	TMP_Text textComponent {
		get {
			if (tComp == null){
				tComp = GetComponent<TMPro.TMP_Text>();
				if (tComp == null){
					tComp = GetComponentInChildren<TMPro.TMP_Text>();
				}
			}
			return tComp;
		}
	}

	public bool setDefaultByObjectName = true;

	public string text {
		get => textComponent.text;
		set => UpdateText(value);
	}

	void Start()
	{
		Debug.Log("TODO: Document UIText component!");
		Reset();
	}

	void OnValidate(){
		Reset();
	}
	void Reset(){
		if (setDefaultByObjectName){
			UpdateText(name);
		}
	}

	void UpdateText(string text){
		// TODO: Localization can happen here.
		textComponent.text = text;
	}
}
