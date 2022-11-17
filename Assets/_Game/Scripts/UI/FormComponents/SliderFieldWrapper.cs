using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderFieldWrapper : FormField<Slider>
{
	[SerializeField] TMP_Text valueDisplay;
	protected override void Start()
	{
		base.Start();
		component.onValueChanged.AddListener(OnChangedValue);
		OnChangedValue(value);
	}

	public float value { get => component.value; set => component.value = value; }

	void OnChangedValue(float value){
		valueDisplay.text = component.wholeNumbers ? value.ToString("0") : value.ToString("0.00");
	}
}
