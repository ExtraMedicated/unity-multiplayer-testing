using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormField<T> : MonoBehaviour where T : UnityEngine.UI.Selectable
{
	[SerializeField] string labelText = "Label";
	[SerializeField] UIText label;
	T _c;
	protected T component {
		get {
			if (_c == null){
				_c = GetComponentInChildren<T>(true);
			}
			return _c;
		}
	}

	public bool inputEnabled {
		get => component.interactable;
		set => component.interactable = value;
	}

	protected virtual void Awake(){
		label.text = labelText;
	}

	void OnValidate(){
		Awake();
	}
}
