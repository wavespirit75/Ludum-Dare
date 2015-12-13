using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

	public Image healthBar;

	public Image headLogo;

	public Text playerName;

	public Sprite[] headLogos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetupHUD(string name, int playerIndex)
	{
		playerName.text = name;
		headLogo.sprite = headLogos[playerIndex];
	}

	public void UpdateHealthBar(float healthBarPercentage)
	{
		healthBar.fillAmount = healthBarPercentage / 100;
	}
}
