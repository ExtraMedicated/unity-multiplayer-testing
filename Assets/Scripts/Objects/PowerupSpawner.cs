using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour {

	public enum PowerupType {
		GasCan,
	}

	public PowerupType powerupType;
	public int secondsToRespawn = -1; // <=0 means no respawn.

	void OnEnable(){
		SpawnItem();
	}

	internal void SpawnItem(){
		if (!NetworkServer.active) return;
		var item = Object.Instantiate(Resources.Load($"Powerups/{powerupType.ToString()}"), transform.position, Quaternion.identity) as GameObject;
		if (secondsToRespawn > 0){
			item.GetComponent<Powerup>().OnCollected += () => {
				// Debug.Log("Item Collected");
				StartCoroutine(RespawnItem());
			};
		}
		NetworkServer.Spawn(item);
	}

	IEnumerator RespawnItem(){
		if (secondsToRespawn > 0){
			yield return new WaitForSeconds(secondsToRespawn);
		}
		SpawnItem();
	}




}
