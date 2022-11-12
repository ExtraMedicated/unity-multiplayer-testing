using Newtonsoft.Json;

public static class ExtDebug {
	public static void LogJson(object obj){
		UnityEngine.Debug.Log(JsonConvert.SerializeObject(obj));
	}
	public static void LogJson(string prefix, object obj){
		UnityEngine.Debug.Log(prefix + JsonConvert.SerializeObject(obj));
	}
	public static void LogJsonError(object obj){
		UnityEngine.Debug.LogError(JsonConvert.SerializeObject(obj));
	}
	public static void LogJsonError(string prefix, object obj){
		UnityEngine.Debug.LogError(prefix + JsonConvert.SerializeObject(obj));
	}
}
