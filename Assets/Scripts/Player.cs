using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
	Up,
	Down,
	Left,
	Right,
}

public class Player : MonoBehaviour {

	public GameObject[] headPrefabs;

	public GameObject[] bodyPrefabs;

	public AudioClip explosionSound;

	public GameObject head;

	public GameObject tail;

	public KeyCode left;

	public KeyCode right;

	public int playerIndex;

	public GameManager gameManager;

	public float gameOverTimer;

	bool isCountingGameOver = false;

	Direction currentDirection;

	List<BodyParts> bodies;

	void Awake() {
		
	}

	// Use this for initialization
	void Start () {
		head = Instantiate(headPrefabs[playerIndex]);
		head.transform.SetParent(transform);
		head.transform.localPosition = Vector3.zero;

		tail = Instantiate(bodyPrefabs[playerIndex]);
		tail.transform.SetParent(transform);
		tail.transform.localPosition = new Vector3(-1, 0 , 0);

		bodies = new List<BodyParts>();

		ChangeCurrentDirection(Direction.Right);

		AddBodyPart();
	}
		
	float interval = 0.75f; 
	float nextTime = 0;
	// Update is called once per frame
	void Update () {
		if(gameManager.currentState == GameState.End)
		{			
			return;
		}

		if(isCountingGameOver)
		{			
			gameOverTimer += Time.deltaTime;
		}

		if (Time.time >= nextTime) 
		{			
			if(gameManager.CheckNextPosition(playerIndex, head, currentDirection))
			{
				isCountingGameOver = false;
				gameOverTimer = 0;

				Vector3 previousPos = head.transform.position;
				MoveObjectInDirection(head, currentDirection);
				
				GameObject frontObject = head;
				Vector3 previousBodyPos;
				foreach(BodyParts part in bodies)
				{
					previousBodyPos = part.transform.position;

					Direction goingInDirection = GetDirection(previousPos, part.transform.position);
					MoveObjectInDirection(part.gameObject, goingInDirection);

					ChangeRotationForObject(part.gameObject, goingInDirection);
					
					previousPos = previousBodyPos;
					frontObject = part.gameObject;
				}

				ChangeRotationForObject(tail, GetDirection(previousPos, tail.transform.position));
				tail.transform.position = previousPos;
			}
			else
			{
				//hit something need to count the game over timer
				isCountingGameOver = true;
			}

			nextTime += interval; 			
		}
		
		if(Input.GetKeyDown(left))
		{
			ChangeCurrentDirection(GetTurnedDirection(currentDirection, Direction.Left));
		}

		if(Input.GetKeyDown(right))
		{			
			ChangeCurrentDirection(GetTurnedDirection(currentDirection, Direction.Right));
		}

		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			AddBodyPart();
		}
	}

	void AddBodyPart()
	{
		Vector3 tailOldPos = tail.transform.position;

		Direction tailDirection;

		if(bodies.Count > 0)
		{
			Vector3 lastBodyPos = bodies[bodies.Count - 1].transform.position;
			tailDirection = GetDirection(lastBodyPos, tailOldPos);
		}
		else
		{
			// no body at all use head position
			tailDirection = GetDirection(head.transform.position, tailOldPos);
		}

		if(tailDirection == Direction.Up)
			tail.transform.position -= new Vector3(0, 1, 0);
		else if(tailDirection == Direction.Down)
			tail.transform.position += new Vector3(0, 1, 0);
		else if(tailDirection == Direction.Left)
			tail.transform.position += new Vector3(1, 0, 0);
		else if(tailDirection == Direction.Right)
			tail.transform.position -= new Vector3(1, 0, 0);

		GameObject newBody = Instantiate(bodyPrefabs[playerIndex]);
		newBody.transform.SetParent(transform);
		newBody.transform.position = tailOldPos;

		BodyParts body = newBody.GetComponent<BodyParts>();

		bodies.Add(body);
	}

	void DecreaseBodyPart()
	{

	}

	Direction GetDirection(Vector3 front, Vector3 back)
	{
		Direction d = Direction.Down;
		if(front.x > back.x)
		{
			d = Direction.Right;
		}
		else if(front.x < back.x)
		{
			d = Direction.Left;
		}
		else if(front.y > back.y)
		{
			d = Direction.Up;
		}
		else if(front.y < back.y)
		{
			d = Direction.Down;
		}

		return d;
	}

	void MoveObjectInDirection(GameObject obj, Direction direction)
	{
		if(direction == Direction.Up)
		{
			obj.transform.position += new Vector3(0, 1, 0);
		}
		else if(direction == Direction.Down)
		{
			obj.transform.position -= new Vector3(0, 1, 0);
		}
		else if(direction == Direction.Left)
		{
			obj.transform.position -= new Vector3(1, 0, 0);
		}
		else
		{
			obj.transform.position += new Vector3(1, 0, 0);
		}
	}

	Direction GetTurnedDirection(Direction direction, Direction turn)
	{
		Direction result = direction;
		if(turn == Direction.Left)
		{
			if(direction == Direction.Up)
				result = Direction.Left;
			else if(direction == Direction.Down)
				result = Direction.Right;
			else if(direction == Direction.Left)
				result = Direction.Down;
			else
				result = Direction.Up;
		}
		else
		{
			if(direction == Direction.Up)
				result = Direction.Right;
			else if(direction == Direction.Down)
				result = Direction.Left;
			else if(direction == Direction.Left)
				result = Direction.Up;
			else
				result = Direction.Down;
		}

		return result;
	}

	public bool CheckPositionBelongsToPlayer(Vector3 position)
	{
		if(head.transform.position == position || tail.transform.position == position)
			return true;

		foreach(BodyParts part in bodies)
		{
			if(part.transform.position == position)
				return true;
		}

		return false;
	}

	void ChangeCurrentDirection(Direction newDirection)
	{
		currentDirection = newDirection;

		ChangeRotationForObject(head, currentDirection);
	}

	void ChangeRotationForObject(GameObject obj, Direction direction)
	{
		if(direction == Direction.Up)
		{
			obj.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
		}
		else if(direction == Direction.Down)
		{
			obj.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
		}
		else if(direction == Direction.Left)
		{
			obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
		}
		else
		{
			obj.transform.rotation = Quaternion.AngleAxis(270, Vector3.forward);
		}
	}

	public void AnimateDead()
	{
		StartCoroutine(Die());
	}

	private IEnumerator Die()
	{
		BodyParts headPart = head.GetComponent<BodyParts>();
		headPart.AnimateExplosion();
		AudioSource.PlayClipAtPoint(explosionSound, headPart.transform.position);
		yield return new WaitForSeconds (0.35f);
		Destroy(headPart.gameObject);

		foreach(BodyParts part in bodies)
		{			
			part.AnimateExplosion();
			AudioSource.PlayClipAtPoint(explosionSound, part.transform.position);
			yield return new WaitForSeconds (0.35f);
			Destroy(part.gameObject);
		}

		BodyParts tailPart = tail.GetComponent<BodyParts>();
		tailPart.AnimateExplosion();
		AudioSource.PlayClipAtPoint(explosionSound, tail.transform.position);
		yield return new WaitForSeconds (0.35f);
		Destroy(tailPart.gameObject);
	}
}
