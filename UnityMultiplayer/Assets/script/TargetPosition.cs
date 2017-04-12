using UnityEngine;
using System;

public class TargetPosition: MonoBehaviour
{
	private float speed = 14f;
	private float threshUp;
	private float threshDown;
	
	//jagged array (array of arrays)
	private static Coord[][] movePatterns;
	private static Coord[] movePattern;
	//current destination point it is moving toward (index of move pattern)
	private static int curDest = 0;

	void Start()
	{
		if (gameObject.name == "Target1T" || gameObject.name == "Target2T") {
			threshUp = -98f;
			threshDown = -160f;
		} else {
			threshUp = -150f;
			threshDown = -200f;	
		}
		
		const float rightmax = 280f;
		const float leftmax = 225f;
		
		//the target moves through the list of coords, repeating when it reaches final coord
		movePatterns = new Coord[][] {
			//no elems = stationary
			//new Coord[0],
			// line
			new Coord[2] {
				new Coord(threshDown, leftmax),
				new Coord(threshUp, rightmax)
			},
			//square
			new Coord[4] {
				new Coord(threshDown, leftmax),
				new Coord(threshDown, rightmax),
				new Coord(threshUp, rightmax),
				new Coord(threshUp, leftmax)
			},
			//reverse square
			new Coord[4] {
				new Coord(threshDown, leftmax),
				new Coord(threshUp, leftmax),
				new Coord(threshUp, rightmax),
				new Coord(threshDown, rightmax)
			},
			//zig-zag
			new Coord[4] {
				new Coord(threshUp, leftmax),
				new Coord(threshDown, rightmax),
				new Coord(threshUp, rightmax),
				new Coord(threshDown, leftmax)
			},
			//reverse zig-zag
			new Coord[4] {
				new Coord(threshUp, leftmax),
				new Coord(threshDown, leftmax),
				new Coord(threshUp, rightmax),
				new Coord(threshDown, rightmax)
			}
		};
	}
	
	/// <summary>
	/// Generates a new move pattern.
	/// Should be called each time game restarts or each new game.
	/// </summary>
	public static void generateNewMovePattern()
	{
		System.Random r = new System.Random(MainController.RANDOM_SEED);
		int index = r.Next(0, movePatterns.Length * 100 - 1) / 100;
		movePattern = movePatterns[index];
	}

	void Update()
	{
		if (!MainController.FSM.IsInState(PuzzleState.GAME_STEP))
			return;
		
		Vector3 curPosition = gameObject.transform.position;
		
		if (gameObject.name == "PlayArea") {
			Vector3 targetPosition = GameObject.Find(GameController.targetTName).transform.position;
			targetPosition.z -= (float)GameInfo.gapHT[GameController.targetTName];
			targetPosition.y = 0f;
			gameObject.transform.position = targetPosition;
			return;
		}
		if (gameObject.name != GameController.targetTName) {
			curPosition.z = 300f;
			gameObject.transform.position = curPosition;	
			return;
		}
		
		//only happens if(gameObject.name == GameController.targetTName)
		int n = MainController.curGameNum;
		if (n == 7 || n == 8 || n == 9) {	
			GameController.setPosition("");
			
			//if distance is near enough to dest point, increment point, looping if necessary
			if (closeToDest(curPosition)) {
				curDest = (curDest + 1) % movePattern.Length;
			}
			//else move toward point
			else {
				Coord dest = movePattern[curDest];
				float tmp = Time.deltaTime * speed;
				float buffer = .5f;
				
				if (dest.y > curPosition.x + buffer) {
					curPosition.x += tmp;
				} else if (dest.y < curPosition.x - buffer) {
					curPosition.x -= tmp;
				}
				if (dest.x > curPosition.z + buffer) {
					curPosition.z += tmp;
				} else if (dest.x < curPosition.z - buffer){
					curPosition.z -= tmp;
				}
			}
			
		} else {
			curPosition.z = (float)GameInfo.initialPositionArray[GameController.targetTName];				
		}
		gameObject.transform.position = curPosition;
	}
	
	private bool closeToDest(Vector3 pos)
	{
		Coord dest = movePattern[curDest];
		// note x-y-z coords messed up b/c different coordinate axis systems
		return Math.Pow(pos.x - dest.y, 2) + Math.Pow(pos.z - dest.x, 2) < 1;
		                                       
	}
	
	//coordinate struct
	private struct Coord
	{
		public Coord(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		public float x, y;
	}
}
