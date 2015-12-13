using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public void LoadScene(int playerNumber) {
		GameManager.numberOfPlayers = playerNumber;

		SceneManager.LoadScene(1);
	}
}
