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

	public Vector3 getDiff()
	{
		//should never be used
		return Vector3.zero;
	}

	public Vector3 getPosition()
	{
		//get average position of both players' combined movement
		if (activePiece != -1) {
			
			curPosition = GameInfo.blockList[activePiece].transform.position;

			Debug.Log("X: " + player1.getDiff().x + " Z: " + player1.getDiff().z);
			double aX = Math.Round(player1.getDiff().x, 4);
			double aZ = Math.Round(player1.getDiff().z, 4);
			double bX = Math.Round(player2.getDiff().x, 4);
			double bZ = Math.Round(player2.getDiff().z, 4);
			float averageX = 0;
			float averageZ = 0;
			//can't use float b/c 0f may not be equal to 0
			if (aX * bX > 0)
				averageX = (float)(aX + bX) / 2.0f;
			if (aZ * bZ > 0)
				averageZ = (float)(aZ + bZ) / 2.0f;
			curPosition += new Vector3(averageX, 0f, averageZ);
			
		}
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
	
	
	/// <summary>
	/// calculates distance squared, use only if performance-critical
	/// </summary>
	/// <param name="enemy"></param>
	/// <returns></returns>
	public static float DistSquared(float x1, float y1, float x2, float y2)
	{
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}
	public static float Distance(float x1, float y1, float x2, float y2)
	{
		return (float)Math.Sqrt(DistSquared(x1, y1, x2, y2));
	}
	/// <summary>
	///the angle between two points, in radians or degrees
	/// </summary>
	public static float Angle(float end_x, float end_y, float start_x, float start_y, bool InDegrees = false)
	{
		float n = 0;
		float dist = Distance(end_x, end_y, start_x, start_y);
		if (dist <= 0)
			return 0; //catches NaN errors start div by 0
		if (end_y >= start_y)
			n = (float)Math.Acos((end_x - start_x) / dist);
		else
			n = -(float)Math.Acos((end_x - start_x) / dist);
		if (InDegrees)
			n *= (float)(180 / Math.PI);
		return n;
	}
}
