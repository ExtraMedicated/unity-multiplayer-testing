using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketEngine : MonoBehaviour {
	Rigidbody shipRigidbody;
	[SerializeField] float thrust = 100;
	[SerializeField] float burnRate = 0.7f;

	bool isFiring;
	Player ship;
	void Awake(){
		shipRigidbody = GetComponentInParent<Rigidbody>();
		ship = GetComponentInParent<Player>();
	}

	public void Fire(bool fire){
		isFiring = fire;
	}

	void FixedUpdate(){
		if (isFiring){
			if (ship.fuel > 0){
				shipRigidbody.AddForceAtPosition(transform.up * thrust * Time.fixedDeltaTime, transform.position, ForceMode.Acceleration);
				// ship.CmdAddForceAtPosition(ship.playerId, transform.up * thrust, transform.position);
				ship.BurnFuel(burnRate * Time.fixedDeltaTime);
			} else {
				isFiring = false;
			}
		}
	}

}
