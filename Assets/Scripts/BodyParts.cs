using UnityEngine;
using System.Collections;

public class BodyParts : MonoBehaviour {

	public GameObject Explosion;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	public void AnimateExplosion()
	{
		StartCoroutine(Die());
	}

	private IEnumerator Die()
	{
		GameObject explosion = Instantiate(Explosion);
		explosion.transform.position = transform.position;
		yield return new WaitForSeconds (0.25f);
		Destroy(explosion);
	}
}
