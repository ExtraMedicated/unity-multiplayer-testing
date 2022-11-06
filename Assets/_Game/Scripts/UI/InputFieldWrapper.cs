using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldWrapper : MonoBehaviour
{

	[SerializeField] string labelText = "Label";
	[SerializeField] string placeholderText = "Enter text...";
	[SerializeField] string defaultText = string.Empty;

	[SerializeField] TMP_Text label;
	[SerializeField] TMP_Text placeholder;
	// [SerializeField] TMP_Text inputText;
	[SerializeField] TMP_InputField inputField;

	public bool inputEnabled {
		get => inputField.interactable;
		set => inputField.interactable = value;
	}

	public string text {
		get => inputField.text.Replace("\u200B", "");
		set => inputField.text = value;
	}

	void OnValidate(){
		Start();
	}

	void Start(){
		SetText(label, labelText);
		SetText(placeholder, placeholderText);
		text = defaultText;
	}

	void SetText(TMP_Text component, string text){
		if (component == null) return;
		if (!string.IsNullOrEmpty(text)){
			// TODO: Localization can happen here.
			component.text = text;
		} else {
			component.text = string.Empty;
		}
	}
}
