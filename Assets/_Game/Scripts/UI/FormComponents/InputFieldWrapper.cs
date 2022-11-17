using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldWrapper : FormField<TMP_InputField>
{
	[SerializeField] string placeholderText = "Enter text...";
	[SerializeField] string defaultText = string.Empty;

	[SerializeField] UIText placeholder;

	public string text {
		get => component.text.Replace("\u200B", "");
		set => component.text = value;
	}

	protected override void Start(){
		base.Start();
		placeholder.text = placeholderText;
		text = defaultText;
	}
}
