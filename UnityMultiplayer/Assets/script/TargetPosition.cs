using UnityEngine;
using System.Collections;

public class TargetPosition: MonoBehaviour{
	private float MoveRate = 20f;
	private bool moveFlag = false;
	private float threshUp;
	private float threshDown;

	void Start(){
		if (gameObject.name == "Target1T" || gameObject.name == "Target2T" ) {
			threshUp = -98f;
			threshDown = -160f;
		} else {
			threshUp = -150f;
			threshDown = -200f;				
		}
	}

	void Update(){
		
		Vector3 curPostition = gameObject.transform.position;
		if (MainController.FSM.IsInState (PuzzleState.GAME_STEP)) {
			if(gameObject.name == "PlayArea"){
				Vector3 targetPosition = GameObject.Find(GameController.targetTName).transform.position;
				targetPosition.z -= (float)GameInfo.gapHT[GameController.targetTName];
				targetPosition.y = 0f;
				gameObject.transform.position = targetPosition;
			}else{
				if (gameObject.name == GameController.targetTName) {
					if (MainController.curGameNum == 7 || MainController.curGameNum == 8 || MainController.curGameNum == 9) {						
						GameController.setPostion("");
						if ((curPostition.z > threshUp && !moveFlag) || (curPostition.z < threshDown && moveFlag)) {
								moveFlag = !moveFlag;
								MoveRate = 0 - MoveRate;
						}

						curPostition.z += Time.deltaTime * MoveRate;
						curPostition.x += Time.deltaTime * MoveRate * Random.Range(0f, 0.5f);
						gameObject.transform.position = curPostition;
					} else {
							curPostition.z = (float)GameInfo.initialPositionArray[GameController.targetTName];
							gameObject.transform.position = curPostition;					
					}
				} else {
					curPostition.z = 300f;
					gameObject.transform.position = curPostition;						
				}
			}
		}
	}
}
