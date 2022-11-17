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

	public bool setDefaultByObjectName;

	public string text {
		get => textComponent.text;
		set => UpdateText(LocalizedText(value));
	}

	public void SetText(string text, Color color){
		SetText(text, "#"+ColorUtility.ToHtmlStringRGB(color));
	}
	public void SetText(string text, string color){
		if (!string.IsNullOrWhiteSpace(color)){
			UpdateText($"<color={color}>{LocalizedText(text)}</color>");
		} else {
			UpdateText(LocalizedText(text));
		}
	}

	void Start()
	{
		// Debug.Log("TODO: Document UIText component!");
		Reset();
		UpdateText(LocalizedText(textComponent.text));
	}

	void OnValidate(){
		Reset();
	}
	void Reset(){
		if (setDefaultByObjectName){
			UpdateText(name);
		}
	}

	string LocalizedText(string text){
		// TODO: Localization can happen here.
		return text;
	}

	void UpdateText(string text){
		textComponent.text = text;
	}
}
