using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class DropdownFieldWrapper : FormField<TMP_Dropdown>
{
	List<string> options = new List<string>();
	public Func<string, string> formatter;
	public TMP_Dropdown.OptionData GetRawValue() => component.options[component.value];
	public string GetFormattedValue() => formatter?.Invoke(options[component.value]) ?? options[component.value];
	public void SetOptions(List<TMP_Dropdown.OptionData> options){
		this.options = options.Select(o => o.text).ToList();
		if (formatter != null){
			component.options = options.Select(o => {
				o.text = formatter.Invoke(o.text);
				return o;
			}).ToList();
		} else {
			component.options = options;
		}
	}
}
