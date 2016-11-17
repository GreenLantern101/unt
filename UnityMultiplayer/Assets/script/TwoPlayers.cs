using UnityEngine;
using System.Collections;


public class TwoPlayers : MonoBehaviour, IPlayerHandler {
	private IPlayerHandler player1;
	private IPlayerHandler player2;	
	public static int activePiece;
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	public static bool readyFlag;
	
	
	public int getActivePiece(){
		activePiece = player1.getActivePiece() == player2.getActivePiece() ? player1.getActivePiece(): -1;
		return activePiece;
	}

	public void setActivePiece(int _acI){
		activePiece = _acI;
	}

	public Vector3 getPosition(){
		//position of first player
		if(activePiece != -1){
			curPosition = GameInfo.blockList[activePiece].transform.position;
			Vector3 newPosition1 = player1.getPosition ();
			Vector3 newPosition2 = player2.getPosition();

			Vector3 diff1 = newPosition1 - curPosition;
			Vector3 diff2 = newPosition2 - curPosition;
			float averageX = 0f;
			float averageZ = 0f;
			if(diff1.x * diff2.x > 0){
				averageX = (diff1.x+diff2.x)/2.0f;
			}
			if(diff1.z * diff2.z > 0){				
				averageZ = (diff1.z+diff2.z)/2.0f;
			}
			curPosition += new Vector3(averageX, 0f, averageZ);
		}
		return curPosition;
	}
	
	public Vector3 getOrientation(){	
		if (GameInfo.NoColorTaskFlag) {
			curOrientation = player1.getOrientation();		
		} else {
			curOrientation = player2.getOrientation();						
		}
		return curOrientation;
	}

	public void setPlayers(IPlayerHandler _p1, IPlayerHandler _p2){
		player1 = _p1;
		player2 = _p2;
	}

	public void finishStep(){

	}

	public bool isReady(){
		return readyFlag;
	}

	public bool isControllable(int _block){
		return false;
	}

	public void skipThisTurn(){

	}
}
