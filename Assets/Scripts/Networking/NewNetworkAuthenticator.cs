using System;
using Mirror;
using UnityEngine;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class NewNetworkAuthenticator : NetworkAuthenticator
{
	#region Messages

	public enum LoginRequestType {
		LocalLoginRequest,
		LoginWithCustomIDRequest,
		LoginWithPlayFabRequest,
		RegisterPlayFabUserRequest,
	}
	public Action<string> AuthErrorEvent;
	public Action<AuthResponseMessage> AuthResponseEvent;
	public Action ClientAuthenticateEvent;

	#endregion

	#region Server

	/// <summary>
	/// Called on server from StartServer to initialize the Authenticator
	/// <para>Server message handlers should be registered in this method.</para>
	/// </summary>
	public override void OnStartServer()
	{
		// Debug.Log("NewNetworkAuthenticator OnStartServer");
		// register a handler for the authentication request we expect from client
		NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
	}

	/// <summary>
	/// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
	/// </summary>
	/// <param name="conn">Connection to client.</param>
	public override void OnServerAuthenticate(NetworkConnectionToClient conn)
	{
		// Debug.Log("NewNetworkAuthenticator OnServerAuthenticate");

	}

	/// <summary>
	/// Called on server when the client's AuthRequestMessage arrives
	/// </summary>
	/// <param name="conn">Connection to client.</param>
	/// <param name="msg">The message payload</param>
	public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
	{
		// Debug.Log($"NewNetworkAuthenticator OnAuthRequestMessage {msg.SessionTicket} {msg.EntityId}");
		try {
			conn.authenticationData = msg.playerEntity;
			NetworkingMessages.SendThroughConnection(conn, new AuthResponseMessage {
				playerEntity = msg.playerEntity,
			});
			// Accept the successful authentication
			ServerAccept(conn);
		} catch (Exception e) {
			Debug.LogError(e);
			OnLoginError(conn, e.Message);
		}
	}

	// Send error message from server to client.
	private void OnLoginError(NetworkConnectionToClient conn, string error)
	{
		Debug.Log("OnLoginError: " + error);
		NetworkingMessages.SendThroughConnection(conn, new AuthErrorMessage {error = error});
		// conn.Disconnect(); Disconnecting here prevents receiving the message.
	}

	#endregion

	#region Client

	/// <summary>
	/// Called on client from StartClient to initialize the Authenticator
	/// <para>Client message handlers should be registered in this method.</para>
	/// </summary>
	public override void OnStartClient()
	{
		// Debug.Log("NewNetworkAuthenticator OnStartClient");
		// register a handler for the authentication response we expect from server
		NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
		NetworkClient.RegisterHandler<AuthErrorMessage>(OnAuthErrorMessage, false);
	}



	/// <summary>
	/// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
	/// </summary>
	public override void OnClientAuthenticate()
	{
		// Debug.Log("OnClientAuthenticate");

		ClientAuthenticateEvent?.Invoke();
	}

	/// <summary>
	/// Called on client when the server's AuthResponseMessage arrives
	/// </summary>
	/// <param name="msg">The message payload</param>
	public void OnAuthResponseMessage(AuthResponseMessage msg)
	{
		// Debug.Log("OnAuthResponseMessage");
		AuthResponseEvent?.Invoke(msg);
		// Authentication has been accepted
		ClientAccept();
	}
	public void OnAuthErrorMessage(AuthErrorMessage msg)
	{
		// Debug.Log("OnAuthErrorMessage");
		AuthErrorEvent?.Invoke(msg.error);
	}

	#endregion
}
