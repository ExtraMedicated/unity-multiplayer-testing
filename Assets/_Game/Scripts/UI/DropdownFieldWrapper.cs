using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownFieldWrapper : MonoBehaviour
{

	[SerializeField] string labelText = "Label";
	// [SerializeField] string placeholderText = "Enter text...";
	// [SerializeField] string defaultText = string.Empty;

	[SerializeField] UIText label;
	// [SerializeField] UIText placeholder;
	// [SerializeField] TMP_Text inputText;
	[SerializeField] TMP_Dropdown dropdown;

	public bool inputEnabled {
		get => dropdown.interactable;
		set => dropdown.interactable = value;
	}

	public TMP_Dropdown.OptionData GetValue() => dropdown.options[dropdown.value];

	public void SetOptions(List<TMP_Dropdown.OptionData> options){
		dropdown.options = options;
	}

	// public string text {
	// 	get => inputField.text.Replace("\u200B", "");
	// 	set => inputField.text = value;
	// }

	void OnValidate(){
		Start();
	}

	void Start(){
		label.text = labelText;
		// placeholder.text = placeholderText;
		// text = defaultText;
	}
}
