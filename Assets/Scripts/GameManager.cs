using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GameState
{
	Playing,
	End,
}

public class GameManager : MonoBehaviour {

	public static int numberOfPlayers;

	public GameObject playerPrefab;

	public GameObject HUDCanvas; 

	public GameObject playerHUD;

	public GameObject gameOverOverlayPrefab;

	public GameObject[] powerUps;

	public GameObject[] tiles; 

	public GameObject wall;

	public GameObject deco;

	public GameState currentState;

	public float gameOverTime = 3.0f;

	float powerUPTime = 3.0f;

	float powerUpTimer = 0.0f;

	float borderWidth = 32;

	float borderHeight = 20;

	List<Player> players;

	List<GameObject> walls;

	List<PowerUp> onFieldPowerUps;

	GameObject gameOverOverlay;

	// Use this for initialization
	void Start () {
		currentState = GameState.Playing;

		StartNewGame();
	}
	
	// Update is called once per frame
	void Update () {
		if(currentState == GameState.Playing)
		{
			if(powerUpTimer > powerUPTime)
			{
				GeneratePowerUps();
				powerUpTimer = 0.0f;
			}
			else
			{
				powerUpTimer += Time.deltaTime;
			}

			int loseCount = 0;
			foreach(Player player in players)
			{				
				if(player.gameOverTimer > gameOverTime)
				{
					if(!player.isGameOver)
					{
						player.AnimateDead();
						player.isGameOver = true;

						Debug.Log("Player " + player.playerIndex + " lose!!");
					}
				}

				if(player.isGameOver)
					loseCount++;
			}

			if(loseCount >= players.Count - 1)
			{
				Player winningPlayer = null;

				foreach(Player player in players)
				{
					if(!player.isGameOver)
						winningPlayer = player;
				}

				currentState = GameState.End;

				gameOverOverlay = Instantiate(gameOverOverlayPrefab);
				gameOverOverlay.transform.SetParent(HUDCanvas.transform);
				gameOverOverlay.transform.localPosition = Vector3.zero;

				GameOverOverlay overlay = gameOverOverlay.GetComponent<GameOverOverlay>();

				if(winningPlayer == null)
				{
					overlay.nameText.text = "No Body";
					overlay.headLogo.gameObject.SetActive(false);
				}
				else
				{
					overlay.nameText.text = "Player " + winningPlayer.playerIndex;
					overlay.headLogo.sprite = winningPlayer.HUD.headLogos[winningPlayer.playerIndex];
				}

				overlay.restartDelegate = delegate() {
					SceneManager.LoadScene(1);
//					Destroy(gameOverOverlay);
//					StartNewGame();
				};
			}
		}
	}

	void ClearScene()
	{

	}

	void StartNewGame()
	{
		players = new List<Player>();

		walls = new List<GameObject>();

		onFieldPowerUps = new List<PowerUp>();

		GenerateBackgroundTiles(borderWidth, borderHeight);

		for(int i = 0 ; i < numberOfPlayers ; i++)
		{
			AddPlayer();
		}
	}

	public bool CheckNextPosition(int playerIndex, GameObject head, Direction playerDirection)
	{
		Vector3 nextPos = head.transform.position;
		if(playerDirection == Direction.Up)
		{
			nextPos += new Vector3(0, 1, 0);
		}
		else if(playerDirection == Direction.Down)
		{
			nextPos -= new Vector3(0, 1, 0);
		}
		else if(playerDirection == Direction.Left)
		{
			nextPos -= new Vector3(1, 0, 0);
		}
		else
		{
			nextPos += new Vector3(1, 0, 0);
		}

		foreach(Player player in players)
		{
			if(!player.isGameOver && player.CheckPositionBelongsToPlayer(nextPos))
			{				
				return false;
			}
		}

		foreach(GameObject wall in walls)
		{
			if((Vector2)(wall.transform.position) == (Vector2)(nextPos))
			{
				return false;
			}
		}

		if(CheckIfPositionExceedBorder(nextPos))
			return false;

		return true;
	}

	bool CheckIfPositionExceedBorder(Vector3 position)
	{
		
		if(position.x < borderWidth / 2 * -1 ||
			position.x >= borderWidth / 2 ||
			position.y < borderHeight / 2 * -1 ||
			position.y >= borderHeight / 2)		
		{
//			Debug.Log("Position is " + position + " " + borderWidth / 2 * -1);
			return true;
		}

		return false;
	}

	public void CheckKillEnclosedArea (Player predator)
	{
		// Get min/max bounding area of source player
		GameObject head = predator.head;
		Vector3 min = head.transform.position;
		Vector3 max = head.transform.position;
		foreach (BodyParts body in predator.bodies)
		{
			if (body.transform.position.x < min.x)
				min.x = body.transform.position.x;
			if (body.transform.position.y < min.y)
				min.y = body.transform.position.y;

			if (body.transform.position.x > max.x)
				max.x = body.transform.position.x;
			if (body.transform.position.y > max.y)
				max.y = body.transform.position.y;
		}

		// For each victim, do a flood fill starting from
		// the player's head. If the flood exceeds min/max, then
		// it is not enclosed within the sourcePlayer.
		foreach (Player victim in players)
		{
			// don't try to kill self!
			if (victim.id == predator.id)
				continue;

			// If the head isn't in bounding area, no need to check anymore
			if (!PositionIsWithinMinMax(victim.head.transform.position, min, max))
				continue;

			// If the head is within bounding area, check for rest of the body
			foreach (BodyParts body in victim.bodies)
			{
				// If any body part is outside bounding area, no need to check anymore
				if (!PositionIsWithinMinMax(body.transform.position, min, max))
					break;
			}

			// If we reached here without failing, then we need to do flood fill
			// check for this victim to see if it really got closed by sourcePlayer.
			// Store list of filled areas
			List<Vector3> fill = new List<Vector3>();

			// First, flood fill the boundary's positions
			fill.Add(head.transform.position);
			foreach (BodyParts body in predator.bodies)
				fill.Add(body.transform.position);

			// Flood fill from the head -- if it fills within the bounds, then it's dead for sure.
			bool isFillWithinBounds = DoFloodFill(fill, victim.head.transform.position, predator, min, max);

			if (isFillWithinBounds)
				victim.AnimateDead();
		}
	}

	bool PositionIsWithinMinMax (Vector3 pos, Vector3 min, Vector3 max)
	{
		Debug.Log("# check : " + pos + " , " + min + " , " + max);
		if (pos.x <= min.x ||
			pos.y <= min.y ||
			pos.x >= max.x ||
			pos.y >= max.y)
			return false;
		else
			return true;
	}

	bool DoFloodFill (List<Vector3> fill, Vector3 position, Player predator, Vector3 min, Vector3 max)
	{
		/*
			Flow:
			Is this position filled?
				If true, DoFloodFill on neighbours (right, up, left, down)
					if false, return false
			return true
		*/

		Debug.Log("do flood fill check : " + position);

		// If current position is occupied, don't check any further
		if (fill.Contains(position))
			return true;

		// If current position is outside bounds, return
		if (!PositionIsWithinMinMax(position, min, max)) {
			Debug.Log("FAILED!");
			return false;
		}

		// If current position is not occupied, fill in position and check neighbours
		fill.Add(position);

		// debug
		GameObject o = new GameObject("MARKER");
		o.transform.position = position;

		bool withinBounds = true;

		withinBounds &= DoFloodFill(fill, position + new Vector3(1, 0, 0), predator, min, max);
		withinBounds &= DoFloodFill(fill, position + new Vector3(0, 1, 0), predator, min, max);
		withinBounds &= DoFloodFill(fill, position + new Vector3(-1, 0, 0), predator, min, max);
		withinBounds &= DoFloodFill(fill, position + new Vector3(0, -1, 0), predator, min, max);

		return withinBounds;
	}

	void GenerateBackgroundTiles(float width, float height)
	{
		int maxRockCount = Random.Range(4, 10);
		for(int w = (int)(width / 2 * -1) ; w < (int)(width / 2) ; w++)
		{
			for(int h = (int)(height / 2 * -1) ; h < (int)(height / 2) ; h++)
			{
				GameObject tile = Instantiate(tiles[Random.Range(0, tiles.Length)]);
				tile.transform.position = new Vector3(w, h, 1);

				//add deco
				int r = Random.Range(0, 50);
				if(r == 15)
				{
					GameObject tileDeco = Instantiate(deco);
					tileDeco.transform.position = new Vector3(w, h, 0.5f);
				}
				else if(r == 40)
				{
					if(walls.Count < maxRockCount)
					{
						GameObject rock = Instantiate(wall);
						rock.transform.position = new Vector3(w, h, 0);

						walls.Add(rock);
					}						
				}
			}
		}
	}

	void AddPlayer()
	{
		switch(players.Count)
		{
		case 0:
			AddPlayer(KeyCode.LeftArrow, KeyCode.RightArrow);
			break;
		case 1:
			AddPlayer(KeyCode.Alpha1, KeyCode.Q);
			break;
		case 2:
			AddPlayer(KeyCode.Z, KeyCode.X);
			break;
		case 3:
			AddPlayer(KeyCode.N, KeyCode.M);
			break;
		}
	}

	void AddPlayer(KeyCode leftKey, KeyCode rightKey)
	{
		Vector3 playerInitialPos;
		Direction playerInitialDirection;

		if(players.Count == 0)
		{
			playerInitialPos = new Vector3(5, 0, 0);
			playerInitialDirection = Direction.Up;
		}
		else if(players.Count == 1)
		{
			playerInitialPos = new Vector3(0, 5, 0);
			playerInitialDirection = Direction.Left;
		}
		else if(players.Count == 2)
		{
			playerInitialPos = new Vector3(-5, 0, 0);
			playerInitialDirection = Direction.Down;
		}
		else
		{
			playerInitialPos = new Vector3(0, -5, 0);
			playerInitialDirection = Direction.Right;
		}

		GameObject player = Instantiate(playerPrefab);
		player.transform.position = playerInitialPos;

		Player p = player.GetComponent<Player>();
		p.gameManager = this;
		p.playerIndex = players.Count;
		p.left = leftKey;
		p.right = rightKey;
		p.currentDirection = playerInitialDirection;

		players.Add(p);

		int width = 150;
		int initialX = 80;
		int initialY = Screen.height - 25;
		GameObject HUD = Instantiate(playerHUD);
		HUD.transform.SetParent(HUDCanvas.transform);
		HUD.transform.position = new Vector3(initialX + p.playerIndex * width, initialY, 0);

		PlayerHUD pHUD = HUD.GetComponent<PlayerHUD>();
		pHUD.SetupHUD("Player " + p.playerIndex, p.playerIndex);

		p.HUD = pHUD;
	}

	void GeneratePowerUps()
	{
		GameObject newPowerUp = Instantiate(powerUps[Random.Range(0, powerUps.Length)]);

		bool isEmpty = false;
		Vector2 newPos = Vector2.zero;
		while(!isEmpty)
		{
			int randomX = Random.Range((int)(borderWidth / 2 * -1), (int)(borderWidth / 2));
			int randomY = Random.Range((int)(borderHeight / 2 * -1), (int)(borderHeight / 2));
			newPos = new Vector2(randomX, randomY);

			bool notEmpty = false;
			foreach(GameObject wall in walls)
			{
				if((Vector2)(wall.transform.position) == newPos)
				{
					notEmpty = true;
					break;
				}
			}

			if(notEmpty)
				continue;

			foreach(Player player in players)
			{
				if(!player.isGameOver && player.CheckPositionBelongsToPlayer((Vector3)newPos))
				{
					notEmpty = true;
					break;
				}
			}

			if(notEmpty)
				continue;

			foreach(GameObject powerUp in powerUps)
			{
				if((Vector2)(powerUp.transform.position) == newPos)
				{
					notEmpty = true;
					break;
				}
			}

			if(notEmpty)
				continue;

			isEmpty = true;
		}

		//set powerup at new position
		newPowerUp.transform.position = (Vector3)newPos;

		onFieldPowerUps.Add(newPowerUp.GetComponent<PowerUp>());
	}

	public void CheckPowerUpAtLocation(Vector3 position, Player player)
	{
		PowerUp powerUpEaten = null;
		foreach(PowerUp powerUp in onFieldPowerUps)
		{
			if((Vector2)(powerUp.transform.position) == (Vector2)position)
			{
				powerUpEaten = powerUp;
				break;
			}
		}

		if(powerUpEaten != null)
		{
			if(powerUpEaten.type == PowerUpType.IncreaseLength ||
				powerUpEaten.type == PowerUpType.IncreaseSpeed)
			{
				powerUpEaten.ApplyPowerUp(player);
			}
			else if(powerUpEaten.type == PowerUpType.DecreaseLength ||
				powerUpEaten.type == PowerUpType.DecreaseSpeed)
			{
				foreach(Player p in players)
				{
					//apply the debuff to other players
					if(p != player)
					{
						powerUpEaten.ApplyPowerUp(p);
					}
				}
			}

			onFieldPowerUps.Remove(powerUpEaten);
			Destroy(powerUpEaten.gameObject);
		}
	}
}
