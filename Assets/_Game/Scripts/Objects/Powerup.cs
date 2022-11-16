using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Parts of this came from the Reward.cs script from the Mirror NetworkRoom example.
public class Powerup : NetworkBehaviour {


	public bool available = true;

	public Action OnCollected;

	[ServerCallback]
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			ClaimItem(other.gameObject);
		}
	}

	// This is called from PlayerController.CmdClaimPrize which is invoked by PlayerController.OnControllerColliderHit
	// This only runs on the server
	public void ClaimItem(GameObject player)
	{
		if (available)
		{
			// This is a fast switch to prevent two players claiming the prize in a bang-bang close contest for it.
			// First hit turns it off, pending the object being destroyed a few frames later.
			available = false;

			OnClaimItem(player);
			OnCollected?.Invoke();

			// // award the points via SyncVar on the PlayerController
			// player.GetComponent<PlayerScore>().score += points;

			// // spawn a replacement
			// Spawner.SpawnReward();

			// destroy this item
			NetworkServer.Destroy(gameObject);
		}
	}

	protected virtual void OnClaimItem(GameObject player){}
}
