using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownFieldWrapper : FormField<TMP_Dropdown>
{
	public TMP_Dropdown.OptionData GetValue() => component.options[component.value];

	public void SetOptions(List<TMP_Dropdown.OptionData> options){
		component.options = options;
	}
}
