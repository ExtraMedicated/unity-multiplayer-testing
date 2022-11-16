using UnityEngine;

public class GasCan : Powerup {
	[SerializeField] float fuelAmount = 25;
	protected override void OnClaimItem(GameObject player)
	{
		player.GetComponent<Player>().AddFuel(fuelAmount);
	}
}