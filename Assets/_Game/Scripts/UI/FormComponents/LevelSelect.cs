using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class LevelSelect : DropdownFieldWrapper {
	[SerializeField] List<string> scenes;

	protected override void Start(){
		base.Start();
		SetOptions(scenes.Select(s => new TMPro.TMP_Dropdown.OptionData(s)).ToList());
	}
}
