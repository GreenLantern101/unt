using UnityEngine;

public class NetworkedPlayer : MonoBehaviour, IPlayerHandler
{

	//TODO: implement this with networking functionality

	public static int activePiece{ get; private set; }
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	public static bool readyFlag{ get; private set; }
	public static float activeTimer;
	
	void Start()
	{
		setActivePiece(-1);
	}
	public void setReadyFlag(bool val){
		readyFlag = val;
	}
	public bool isReady()
	{
		return true;
		return readyFlag;
	}
	
	public int getActivePiece()
	{
		return activePiece;
	}
	public void setActivePiece(int _acI)
	{
		activePiece = _acI;
	}
	
	public Vector3 getPosition()
	{
		return curPosition;
	}
	public void setPosition(Vector3 _position)
	{
		curPosition = _position;
	}
	
	public Vector3 getOrientation()
	{
		return curOrientation;
	}

	public void finishStep()
	{

	}

	public bool isControllable(int _block)
	{
		return false;
	}

	public void skipThisTurn()
	{
	}
}
