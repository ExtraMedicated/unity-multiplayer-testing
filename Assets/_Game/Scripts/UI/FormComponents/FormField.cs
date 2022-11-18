using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormField<T> : MonoBehaviour where T : UnityEngine.UI.Selectable
{
	[SerializeField] string labelText = "Label";
	[SerializeField] UIText label;
	protected T component;

	public bool inputEnabled {
		get => component.interactable;
		set => component.interactable = value;
	}

	protected virtual void Awake(){
		component = GetComponentInChildren<T>();
		label.text = labelText;
	}

	void OnValidate(){
		Awake();
	}
}
