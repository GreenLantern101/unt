using UnityEngine;

public class TargetPosition: MonoBehaviour
{
	private float MoveRate = 20f;
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
			if (curPosition.z > threshUp || curPosition.z < threshDown) {
				MoveRate *= -1;
			}

			curPosition.z += Time.deltaTime * MoveRate;
			curPosition.x += Time.deltaTime * MoveRate * 0.3f;
		} else {
			curPosition.z = (float)GameInfo.initialPositionArray[GameController.targetTName];				
		}
		gameObject.transform.position = curPosition;
	}
}
