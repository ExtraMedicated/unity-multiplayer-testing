using System;
using kcp2k;
using Mirror;
using UnityEngine;

public class TransportWrapper : MonoBehaviour {
	public Transport transport;
	public ushort GetPort(){
		// if (transport as Tugboat != null){
		// 	return ((Tugboat)transport).GetPort();
		// }
		if (transport as TelepathyTransport != null){
			return ((TelepathyTransport)transport).port;
		}
		if (transport as KcpTransport != null){
			return ((KcpTransport)transport).Port;
		}
		throw new Exception("GetPort: Unhandled transport type: " + transport.GetType().Name);
	}

	public void SetPort(ushort port){
		// if (transport as Tugboat != null){
		// 	((Tugboat)transport).SetPort(port);
		// 	return;
		// }
		if (transport as TelepathyTransport != null){
			((TelepathyTransport)transport).port = port;
			return;
		}
		if (transport as KcpTransport != null){
			((KcpTransport)transport).Port = port;
			return;
		}
		throw new Exception("SetPort: Unhandled transport type: " + transport.GetType().Name);
	}

	public int GetTimeoutMS(){
		if (transport as TelepathyTransport != null){
			return ((TelepathyTransport)transport).SendTimeout;
		}
		if (transport as KcpTransport != null){
			return ((KcpTransport)transport).Timeout;
		}
		throw new Exception("SetPort: Unhandled transport type: " + transport.GetType().Name);

	}
	// public string GetClientAddress(){
	// 	if (transport as Tugboat != null){
	// 		return ((Tugboat)transport).GetClientAddress();
	// 	}
	// 	throw new Exception("GetPort: Unhandled transport type: " + transport.GetType().Name);
	// }

	// public void SetClientAddress(string address){
	// 	if (transport as Tugboat != null){
	// 		((Tugboat)transport).SetClientAddress(address);
	// 		return;
	// 	}
	// 	throw new Exception("SetPort: Unhandled transport type: " + transport.GetType().Name);
	// }
}
