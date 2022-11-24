using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class SignalRConnection {
	public string ConnectionHandle { get; private set; }
	public SignalR signalR;
	public bool isConnected;

	public void Start(){
		var url = $"https://{Config.PlayFabTitleId}.playfabapi.com/pubsub";
		if (signalR == null){
			signalR = new SignalR();
		}
		Debug.Log("SignalR URL: " + url);
		signalR.Init(url, opts => {
			opts.Headers = new Dictionary<string, string>{
				{"X-EntityToken", PlayerEntity.EntityToken}
			};
		});

		signalR.ConnectionStarted += async (object sender, ConnectionEventArgs e) =>
		{
			isConnected = true;
			Debug.Log($"SignalR Connected: {e.ConnectionId}");
			await StartOrRecoverSession();
		};
		signalR.ConnectionClosed += (object sender, ConnectionEventArgs e) =>
		{
			isConnected = false;
			Debug.Log($"SignalR Disconnected: {e.ConnectionId}");
			ConnectionHandle = null;
		};
		signalR.Connect();
	}

	public async void Stop(){
		if (signalR != null && isConnected){
			var response = await signalR.Invoke<EndSessionResponse>("EndSession", new EndSessionRequest());
			ExtDebug.LogJson("EndSessionResponse: ", response);
			signalR.Stop();
		}
	}

	async Task StartOrRecoverSession(){
		if (isConnected){
			// var upstreamActivity = new System.Diagnostics.Activity("Upstream");
			// upstreamActivity.Start();
			var response = await signalR.Invoke<StartOrRecoverSessionResponse>("StartOrRecoverSession", new StartOrRecoverSessionRequest {
				oldConnectionHandle = ConnectionHandle
			});
			ExtDebug.LogJson("StartOrRecoverSessionResponse: ", response);
			// upstreamActivity.Stop();
			if (response.status.ToLower() == "success"){
				ConnectionHandle = response.newConnectionHandle;
			} else {
				ExtDebug.LogJsonError("StartOrRecoverSession returned non-success status.", response);
			}
		}
	}

	struct StartOrRecoverSessionRequest {
		public string oldConnectionHandle { get; set; }
		public string traceParent { get; set; }
	}
	class StartOrRecoverSessionResponse {
		public string newConnectionHandle { get; set; }
		public string recoveredTopics { get; set; }
		public string status { get; set; }
		public string traceId { get; set; }
	}

	struct EndSessionRequest {
		public string traceId { get; set; }
	}

	struct EndSessionResponse {
		public string status { get; set; }
		public string traceId { get; set; }
	}
	public class Message{
		public string topic { get; set; }
		public byte[] payload { get; set; }
		public string traceId { get; set; }
	}
}
