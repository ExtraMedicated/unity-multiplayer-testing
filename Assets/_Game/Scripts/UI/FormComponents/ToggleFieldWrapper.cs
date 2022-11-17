using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ToggleFieldWrapper : FormField<Toggle>
{
	public UnityEvent<bool> OnValueChanged;
	protected override void Start()
	{
		base.Start();
		component.onValueChanged.AddListener(OnChangedValue);
		OnChangedValue(value);
	}

	public bool value { get => component.isOn; set => component.isOn = value; }

	void OnChangedValue(bool value){
		OnValueChanged?.Invoke(value);
	}

}
