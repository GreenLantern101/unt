using UnityEngine;
using System;

public class TargetPosition: MonoBehaviour
{
	private float speed = 14f;
	private float threshUp;
	private float threshDown;

	void Start()
	{
		if (gameObject.name == "Target1T" || gameObject.name == "Target2T") {
			threshUp = -98f;
			threshDown = -160f;
		} else {
			threshUp = -150f;
			threshDown = -200f;	
		}
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
			if (curPosition.z > threshUp){
				//always negative
				speed = -Math.Abs(speed);
			}
			else if(curPosition.z < threshDown) {
				//always positive
				speed = Math.Abs(speed);
			}

			curPosition.z += Time.deltaTime * speed;
			curPosition.x += Time.deltaTime * speed * 0.3f;
		} else {
			curPosition.z = (float)GameInfo.initialPositionArray[GameController.targetTName];				
		}
		gameObject.transform.position = curPosition;
	}
}
