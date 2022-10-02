using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static NetworkConfig;

[CustomEditor(typeof(NetworkConfig))]
public class NetworkConfigEditor : Editor {

	// NetConfigVals[] configs;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		var t = target as NetworkConfig;

		GUILayout.Label($"Local: {NetworkConfig.LocalIpAddress}:{NetworkConfig.LocalPort}");
		GUILayout.Label($"Online: {NetworkConfig.OnlineIpAddress}:{NetworkConfig.OnlinePort}");

		// configs = t.configs;
		// EditorGUI.BeginChangeCheck();

		// for (int i = 0; i < configs.Length; i++)
		// {
		// 	if (GUILayout.Button($"Use {configs[i].IpAddress}:{configs[i].Port}")){
		// 		for (int j = 0; j < configs.Length; j++)
		// 		{
		// 			configs[j].isActive = j == i;
		// 		}
		// 	}
		// }

		// if (EditorGUI.EndChangeCheck()){
		// 	Undo.RecordObject(target, "Change active config");
		// 	for (int i = 0; i < configs.Length; i++)
		// 	{
		// 		t.configs[i].isActive = configs[i].isActive;
		// 	}
		// }

	}
}
