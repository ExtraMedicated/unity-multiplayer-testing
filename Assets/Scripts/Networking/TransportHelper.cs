using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;

public class TransportHelper : MonoBehaviour {
	public Transport transport;
	public ushort GetPort(){
		if (transport as TelepathyTransport != null){
			return ((TelepathyTransport)transport).port;
		}
		if (transport as KcpTransport != null){
			return ((KcpTransport)transport).Port;
		}
		throw new Exception("GetPort: Unhandled transport type: " + transport.GetType().Name);
	}

	public void SetPort(ushort port){
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
}
