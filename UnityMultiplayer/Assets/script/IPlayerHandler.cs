using UnityEngine;
using System.Collections;

public interface IPlayerHandler {
	//get current active piece
	int getActivePiece();

	//get current position 
	Vector3 getPosition();

	//get current orientation 
	Vector3 getOrientation();

	//finish current step
	void finishStep();

	//Should be ready for a new game
	bool isReady();

	//whehter can move the block
	bool isControllable(int _block);

	//let the other player move the block
	void skipThisTurn();	

	//set active piece
	void setActivePiece(int _acI);
}
