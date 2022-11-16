using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

// See: https://www.youtube.com/watch?v=RIvEJ2LP0W0
public class BuildScript
{
	const string SERVER_DEFINES = "MIRROR;MIRROR_17_0_OR_NEWER;MIRROR_18_0_OR_NEWER;MIRROR_24_0_OR_NEWER;MIRROR_26_0_OR_NEWER;MIRROR_27_0_OR_NEWER;MIRROR_28_0_OR_NEWER;MIRROR_29_0_OR_NEWER;MIRROR_30_0_OR_NEWER;MIRROR_30_5_2_OR_NEWER;MIRROR_32_1_2_OR_NEWER;MIRROR_32_1_4_OR_NEWER;MIRROR_35_0_OR_NEWER;MIRROR_35_1_OR_NEWER;MIRROR_37_0_OR_NEWER;MIRROR_38_0_OR_NEWER;MIRROR_39_0_OR_NEWER;MIRROR_40_0_OR_NEWER;MIRROR_41_0_OR_NEWER;MIRROR_42_0_OR_NEWER;MIRROR_43_0_OR_NEWER;MIRROR_44_0_OR_NEWER;MIRROR_46_0_OR_NEWER;MIRROR_47_0_OR_NEWER;MIRROR_53_0_OR_NEWER;MIRROR_55_0_OR_NEWER;MIRROR_57_0_OR_NEWER;MIRROR_58_0_OR_NEWER;MIRROR_65_0_OR_NEWER;MIRROR_66_0_OR_NEWER;ENABLE_PLAYFABSERVER_API";
	const string CLIENT_DEFINES = "MIRROR;MIRROR_17_0_OR_NEWER;MIRROR_18_0_OR_NEWER;MIRROR_24_0_OR_NEWER;MIRROR_26_0_OR_NEWER;MIRROR_27_0_OR_NEWER;MIRROR_28_0_OR_NEWER;MIRROR_29_0_OR_NEWER;MIRROR_30_0_OR_NEWER;MIRROR_30_5_2_OR_NEWER;MIRROR_32_1_2_OR_NEWER;MIRROR_32_1_4_OR_NEWER;MIRROR_35_0_OR_NEWER;MIRROR_35_1_OR_NEWER;MIRROR_37_0_OR_NEWER;MIRROR_38_0_OR_NEWER;MIRROR_39_0_OR_NEWER;MIRROR_40_0_OR_NEWER;MIRROR_41_0_OR_NEWER;MIRROR_42_0_OR_NEWER;MIRROR_43_0_OR_NEWER;MIRROR_44_0_OR_NEWER;MIRROR_46_0_OR_NEWER;MIRROR_47_0_OR_NEWER;MIRROR_53_0_OR_NEWER;MIRROR_55_0_OR_NEWER;MIRROR_57_0_OR_NEWER;MIRROR_58_0_OR_NEWER;MIRROR_65_0_OR_NEWER;MIRROR_66_0_OR_NEWER";

	const string COPY_SERVER_TO_PATH = "C:/tmp/Build";

	static string[] scenes = new[] {
		"Assets/_Game/Scenes/MainMenu.unity",
		"Assets/_Game/Scenes/Lobby.unity",
		"Assets/_Game/Scenes/Levels/Level1.unity",
		"Assets/_Game/Scenes/Levels/Level2.unity",
		"Assets/_Game/Scenes/Levels/Level3.unity",
	};

	[MenuItem("Build/Build All")]
	public static void BuildAll(){
		BuildWindowsClient();
		BuildWindowsServer();
		SwitchToWindowsStandalone();
	}

	private static void ClearFolder(string path){
		// https://stackoverflow.com/a/1288747/5372006
		System.IO.DirectoryInfo di = new DirectoryInfo(path);
		if (!di.Exists){
			di.Create();
			return;
		}
		foreach (FileInfo file in di.GetFiles())
		{
			file.Delete();
		}
		foreach (DirectoryInfo dir in di.GetDirectories())
		{
			dir.Delete(true);
		}
	}

	[MenuItem("Build/Build Windows Server")]
	public static void BuildWindowsServer(){
		const string FOLDER = "Builds/Windows/Server";
		try
		{

			ClearFolder(FOLDER);

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = scenes;
			buildPlayerOptions.locationPathName = FOLDER + $"/{Application.productName}.exe";
			buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
			buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
			buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
			buildPlayerOptions.extraScriptingDefines = SERVER_DEFINES.Split(';');

			Console.WriteLine("Building Windows server...");
			var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
			if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded){
				Console.WriteLine("Built Windows server.");
				// https://stackoverflow.com/a/32395487/5372006
				ZipFile.CreateFromDirectory(FOLDER, "Server.zip");
				File.Move("Server.zip", FOLDER + "/Server.zip");
				Process.Start($"{Environment.CurrentDirectory}/{FOLDER}");
				ZipFile.ExtractToDirectory($"{FOLDER}/Server.zip", COPY_SERVER_TO_PATH, true);
			}
		}
		catch (Exception exception)
		{
			//The system cannot find the file specified...
			UnityEngine.Debug.LogError(exception);
			Console.WriteLine(exception.Message);
		}
	}

	[MenuItem("Build/Build Windows Client")]
	public static void BuildWindowsClient(){
		const string FOLDER = "Builds/Windows/Client";
		try
		{
			ClearFolder(FOLDER);

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.scenes = scenes;
			buildPlayerOptions.locationPathName = FOLDER + $"/{Application.productName}.exe";
			buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
			buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
			buildPlayerOptions.extraScriptingDefines = CLIENT_DEFINES.Split(';');

			Console.WriteLine("Building Windows client...");
			var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
			if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded){
				BuildPipeline.BuildPlayer(buildPlayerOptions);
				Console.WriteLine("Built Windows client.");
				Process.Start($"{Environment.CurrentDirectory}/{FOLDER}");
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogError(exception);
		}
	}
	// [MenuItem("Build/Build Android Client")]
	// public static void BuildAndroidClient(){
	// 	const string FOLDER = "Builds/Android";
	// 	try
	// 	{
	// 		ClearFolder(FOLDER);

	// 		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
	// 		buildPlayerOptions.scenes = scenes;
	// 		buildPlayerOptions.locationPathName = FOLDER + $"/{Application.productName}.exe";
	// 		buildPlayerOptions.target = BuildTarget.Android;
	// 		buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
	// 		buildPlayerOptions.extraScriptingDefines = CLIENT_DEFINES.Split(';');

	// 		Console.WriteLine("Building Android client...");
	// 		var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
	// 		if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded){
	// 			BuildPipeline.BuildPlayer(buildPlayerOptions);
	// 			Console.WriteLine("Built Android client.");
	// 			// Process.Start($"{Environment.CurrentDirectory}/{FOLDER}");
	// 		}
	// 	}
	// 	catch (Exception exception)
	// 	{
	// 		UnityEngine.Debug.LogError(exception);
	// 	}
	// }

	[MenuItem("Build/Switch to Windows Standalone")]
	public static void SwitchToWindowsStandalone()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
	}
	[MenuItem("Build/Switch to Windows Server")]
	public static void SwitchToWindowsServer()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
	}

}
