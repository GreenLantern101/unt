using UnityEngine;
using System.Collections;

public class NetworkedPlayer : MonoBehaviour, IPlayerHandler {

	//TODO: implement this with networking functionality

	public static int activePiece;
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	public static bool readyFlag;
	
	
	public int getActivePiece(){
		return activePiece;
	}
	
	public Vector3 getPosition(){
		return curPosition;
	}
	
	public Vector3 getOrientation(){
		return curOrientation;
	}

	public void finishStep(){

	}

	public bool isReady(){
		return readyFlag;
	}
	public void setActivePiece(int _acI){
		activePiece = _acI;
	}

	public static void sendReadyFlag(){
		//send the player is ready to the other player
	}

	public bool isControllable(int _block){
		return false;
	}

	public void skipThisTurn(){
	}
}
