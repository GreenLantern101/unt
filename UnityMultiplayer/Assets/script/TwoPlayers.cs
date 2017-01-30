using UnityEngine;
using System;

//for collaborative games --> combines the actions of two cooperative players
public class TwoPlayers : MonoBehaviour, IPlayerHandler
{
	private IPlayerHandler player1;
	private IPlayerHandler player2;
	public static int activePiece;
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	public static bool readyFlag;
	
	
	public int getActivePiece()
	{
		activePiece = player1.getActivePiece() == player2.getActivePiece() ? player1.getActivePiece() : -1;
		return activePiece;
	}

	public void setActivePiece(int _acI)
	{
		activePiece = _acI;
	}

	
	Vector3 p1diff;
	Vector3 p2diff;
	
	Vector3 lastSentPos;
	
	Vector3 p1oldpos;
	Vector3 p2oldpos;
	
	public void setPosition(Vector3 newpos)
	{
		curPosition = newpos;
	}

	public Vector3 getPosition()
	{
		//get average position of both players' combined movement
		if (activePiece != -1) {
			
			curPosition = GameInfo.blockList[activePiece].transform.position;

			
			p1diff = player1.getPosition() - p1oldpos;
			p2diff = player2.getPosition() - p2oldpos;
			
			if (p1diff.magnitude > 50)
				p1diff = Vector3.zero;
			if (p2diff.magnitude > 50)
				p2diff = Vector3.zero;
			
			double aX = Math.Round(p1diff.x, 4);
			double aZ = Math.Round(p1diff.z, 4);
			double bX = Math.Round(p2diff.x, 4);
			double bZ = Math.Round(p2diff.z, 4);
			float averageX = 0;
			float averageZ = 0;
			//can't use float b/c 0f may not be equal to 0
			if (aX * bX > 0)
				averageX = (float)(aX + bX) / 2.0f;
			if (aZ * bZ > 0)
				averageZ = (float)(aZ + bZ) / 2.0f;
			curPosition += new Vector3(averageX, 0f, averageZ);
			
			p1oldpos = player1.getPosition();
			p2oldpos = player2.getPosition();
			
			
			//black node player is more powerful, commands sync
			if (MainController.curNode == NODE.BLACK_NODE &&
			   Vector3.Magnitude(curPosition - lastSentPos) > 1) {
				string message = "twoplayerpos: " + curPosition.x + "," + curPosition.y + "," + curPosition.z;
				GameController.SyncGame_command(message);
				lastSentPos = curPosition;
			}
			
		}
		//Debug.Log("POS: " + curPosition);
		return curPosition;
	}
	
	public Vector3 getOrientation()
	{	
		if (GameInfo.NoColorTaskFlag) {
			curOrientation = player1.getOrientation();		
		} else if (GameInfo.OtherNoColorTaskFlag) {
			curOrientation = player2.getOrientation();				
		} else {
			return (player1.getOrientation() + player2.getOrientation()) / 2;
		}
		return curOrientation;
	}

	public void setPlayers(IPlayerHandler _p1, IPlayerHandler _p2)
	{
		player1 = _p1;
		player2 = _p2;
	}

	public void finishStep()
	{

	}

	public bool isReady()
	{
		return readyFlag;
	}

	public bool isControllable(int _block)
	{
		return false;
	}

	public void skipThisTurn()
	{

	}
}
