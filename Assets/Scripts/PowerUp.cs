using UnityEngine;
using System.Collections;

public enum PowerUpType
{
	None,
	IncreaseLength,
	DecreaseLength,
	IncreaseSpeed,
	DecreaseSpeed,
}

public class PowerUp : MonoBehaviour {

	public AudioClip powerUpSound;

	public PowerUpType type;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ApplyPowerUp(Player player)
	{
		switch(type)
		{
		case PowerUpType.IncreaseSpeed:
			player.IncreaseSpeed();
			break;
		case PowerUpType.DecreaseSpeed:
			player.DecreaseSpeed();
			break;
		case PowerUpType.IncreaseLength:
			player.AddBodyPart();
			break;
		case PowerUpType.DecreaseLength:
			player.DecreaseBodyPart();
			break;
		}

		AudioSource.PlayClipAtPoint(powerUpSound, transform.position);
	}
}
