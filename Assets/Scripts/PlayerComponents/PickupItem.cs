using UnityEngine;
using System.Collections;
using Mirror;

public class PickupItem : NetworkBehaviour {

	Player playerController;
	// Use this for initialization
	void Start () {
		playerController = gameObject.GetComponent<Player>();
	}

	[ServerCallback]
	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "GasCan")
		{
			playerController.AddFuel(100);
			Destroy(other.gameObject);
		}

	}
}
