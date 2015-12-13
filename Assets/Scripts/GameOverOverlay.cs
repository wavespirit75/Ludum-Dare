using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverOverlay : MonoBehaviour {

	public delegate void RestartGameDelegate();

	public RestartGameDelegate restartDelegate;

	public Text nameText;

	public Image headLogo;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnRestartGameButtonSelected()
	{
		if(restartDelegate != null)
			restartDelegate();
	}

	public void OnMainMenuButtonSelected()
	{
		SceneManager.LoadScene(0);
	}
}
