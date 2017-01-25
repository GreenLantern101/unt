using UnityEngine;
using System.Collections;

public class MoveValidator : MonoBehaviour
{
	public float gap;

	public MoveValidator()
	{

	}

	/// <summary>
	/// Ises the move valid.
	/// </summary>
	/// <returns><c>true</c>, if move valid was ised, <c>false</c> otherwise.</returns>
	public bool isMoveValid(int _activePiece)
	{
		//no active piece
		if (_activePiece == -1)
			return false;
		//Not in the play mode
		if (!MainController.FSM.IsInState(PuzzleState.GAME_STEP)) {
			return false;
		}
				
		//Block is at the correct location
		if (GameInfo.blockSucceed[_activePiece])
			return false;
				
		//whether can be moved by the current player		

		return true;
	}

	public bool isSucceeded(int _activePiece)
	{
		Vector3 curPosition = GameInfo.blockList[_activePiece].transform.position;
		Vector3 targetPosition = GameInfo.getTargetPosition(_activePiece);
		if (targetPosition == Vector3.zero)
			print("The target position is 0");

		if (MainController.curGameNum > 6) {
			gap = 0.5f;		
		} else {
			gap = 2.0f;				
		}
		targetPosition.y = curPosition.y;
		if (Vector3.Distance(targetPosition, curPosition) > gap)
			return false;

//		Vector3 curRotation = GameInfo.blockList [_activePiece].transform.localEulerAngles;
//		float targetRotationY = GameInfo.getTargetRotation (_activePiece);
//		if (Mathf.Abs(curRotation.y - targetRotationY) > 10)
//						return false;

		return true;
	}
}









