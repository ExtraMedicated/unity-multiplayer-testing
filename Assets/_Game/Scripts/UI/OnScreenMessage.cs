using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenMessage : MonoBehaviour {
	UIText uiText;
	static OnScreenMessage instance;
	void Start(){
		if (instance != null){
			Destroy(gameObject);
			return;
		}
		uiText = GetComponent<UIText>();
		instance = this;
		uiText.text = string.Empty;
	}
	void OnDestroy(){
		if (instance == this){
			instance = null;
		}
	}

	public static void SetText(string text){
		if (instance != null){
			instance.uiText.text = text;
		}
	}
	public static void SetText(string text, string color){
		instance.uiText.SetText(text, color);
	}
}
