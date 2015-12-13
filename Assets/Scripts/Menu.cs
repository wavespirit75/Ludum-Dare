using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public void LoadScene(int playerNumber) {
//		GameManager.numberOfPlayers = playerNumber;
		Application.LoadLevel(playerNumber);
	}
}
