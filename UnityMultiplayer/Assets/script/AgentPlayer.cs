using UnityEngine;
using System.Collections;
using System;
using System.Globalization;

public class AgentPlayer : MonoBehaviour, IPlayerHandler {
	public static int activePiece;	
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	private Vector3 targetPosition;
	private Vector3 targetOrientation;
	public static bool readyFlag;	
	private float speed = 20f;
	private float rate;
	public static bool moveBlockFlag = false;			//whether to move the block or not
	public static bool turnSkipFlag = false;			//whether to move the block or not
	public static bool RotateBlockFlag = false;
	public static float activeTimer;
	

	void Start(){
		setActivePiece (-1);
	}

	public int getActivePiece(){
		return activePiece;
	}
	
	public Vector3 getPosition(){
		return curPosition;
	}

    {
    }
	
	public Vector3 getOrientation(){
		return curOrientation;
	}

	public bool isReady(){
		return true;
	}

	public void finishStep(){
	}

	public void setActivePiece(int _acI){
		if (_acI == -1) {
			activeTimer = -1000f;				
		} else {
			GameController.secondaryActivePiece = _acI;
			print ("secondary active1 " + GameController.secondaryActivePiece);
			activeTimer = 0f;			
		}
		activePiece = _acI;
	}

	public bool isControllable(int _block){
		return false;
	}

	
	public void skipThisTurn(){
		turnSkipFlag = true;
	}


	//based on current and the target location, move the block
	public void startMoveBlock(int blockId){
		if (GameController.active_player == MainController._agentPlayer 
		    || GameController.active_player == MainController._twoPlayers) {
			setActivePiece(blockId);
			GameObject activeObject = GameInfo.blockList [activePiece];
			curPosition = activeObject.transform.position;
			targetPosition = GameInfo.getTargetPosition(activePiece);
			moveBlockFlag = true;
		} else {
			moveBlockFlag = false;
			setActivePiece(-1);
		}
	}

	public void startRotateBlock(int blockId){	
//		print ("The rotated block is " + blockId);
//		activePiece = blockId;
//		GameObject activeObject = GameInfo.blockList[blockId];
//		curOrientation = activeObject.transform.localEulerAngles;
//		targetOrientation = GameInfo.getTargetRotation[blockId];
//		RotateBlockFlag = true;
	}

	void Update(){

		//detect the failure actions

		if (moveBlockFlag) {	
			if(Vector3.Distance(GameInfo.blockList [activePiece].transform.position, GameInfo.getTargetPosition(activePiece)) > 2.5){
				rate = 1;
			}else{
				rate = 5;
			}
			float step = speed * Time.deltaTime/rate;
			Vector3 newPosition = Vector3.MoveTowards(GameInfo.blockList[activePiece].transform.position, GameInfo.getTargetPosition(activePiece), step);
			curPosition = newPosition;
			if (curPosition == GameInfo.getTargetPosition(activePiece)){
				moveBlockFlag = false;
				setActivePiece(-1);
			}
		} else {
			setActivePiece(-1);
		}
		if (activePiece != -1 && GameController.active_player.getActivePiece () != activePiece) {
//			print ("agent active " + activePiece + "active player active " + GameController.active_player.getActivePiece());
				activeTimer += Time.deltaTime;
				if (activeTimer > GameInfo.activeLen) {
						activeTimer = -1000f;
						LanguageManager.feedbackTimer1 = 0;
				}
		} else {
			activeTimer = -1000f;				
		}
	}



}
