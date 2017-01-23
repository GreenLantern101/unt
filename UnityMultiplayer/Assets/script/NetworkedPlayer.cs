using UnityEngine;

public class NetworkedPlayer : MonoBehaviour, IPlayerHandler
{

	public static int activePiece { get; private set; }
	public static Vector3 curPosition { get; private set; }
	private static Vector3 diff;
	public static Vector3 curOrientation;
	public static bool readyFlag { get; private set; }
	public static float activeTimer;

	void Start()
	{
		setActivePiece(-1);
	}
	public void setReadyFlag(bool val)
	{
		readyFlag = val;
	}
	public bool isReady()
	{
		//return true;
		return readyFlag;
	}

	public int getActivePiece()
	{
		return activePiece;
	}
	public void setActivePiece(int _acI)
	{
		activePiece = _acI;
		//reset diff
		diff = Vector3.zero;
	}

	public Vector3 getPosition()
	{
		return curPosition;
	}
	public Vector3 getDiff()
	{
		return diff;
	}
	public Vector3 getOrientation()
	{
		return curOrientation;
	}
	
	//for syncing
	public void setDiff(Vector3 newdiff){
		diff = newdiff;
	}
	//for syncing
	public void setPosition(Vector3 newposition)
	{
		diff = newposition - curPosition;
		curPosition = newposition;
	}
	//for syncing
	public void setOrientation(Vector3 neworientation)
	{
		curOrientation = neworientation;
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
	
	void Update()
	{
		if (activePiece != -1 && GameController.active_player.getActivePiece() != activePiece) {
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
