using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class LevelSelect : DropdownFieldWrapper {
	[SerializeField] List<string> scenes;

	protected override void Awake(){
		base.Awake();
		formatter = (Func<string, string>)(s => s.Replace("_Game/Scenes/Levels/", "").Replace(".unity", ""));
		SetOptions(scenes.Select(s => new TMPro.TMP_Dropdown.OptionData(s)).ToList());
	}


	// This is not called automatically. Add this as a listener to the dropdown component in order to use it.
	public void UseSelectedLevel(){
		var netMgr = NetworkManager.singleton as ExtNetworkRoomManager;
		netMgr.GameplayScene = GetRawValue().text;
	}
}
