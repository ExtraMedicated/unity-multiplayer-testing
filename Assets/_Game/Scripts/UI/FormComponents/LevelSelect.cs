using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class LevelSelect : DropdownFieldWrapper {
	[SerializeField] List<string> scenes;

	protected override void Awake(){
		base.Awake();
		SetOptions(scenes.Select(s => new TMPro.TMP_Dropdown.OptionData(s)).ToList());
	}
}
