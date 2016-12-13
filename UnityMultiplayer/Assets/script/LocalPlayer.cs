using UnityEngine;

public class LocalPlayer : MonoBehaviour, IPlayerHandler
{
	public static int activePiece{get; private set;}
	public static Vector3 curPosition;
	public static Vector3 curOrientation;
	public static bool readyFlag{get; private set;}
	public static float activeTimer;

	void Start()
	{
		setActivePiece(-1);
	}
	public void setReadyFlag(bool val){
		readyFlag = val;
		
		//GameController
		//send ready flag immediately after setting
		sendReadyFlag();
	}
	public bool isReady()
	{
		return readyFlag;
	}
	static void sendReadyFlag()
	{
		//send the player is ready to the other player
	}
	
	
	public int getActivePiece()
	{
		return activePiece;
	}
	public void setActivePiece(int _acI)
	{
		if (_acI == -1) {
			activeTimer = -1000f;				
		} else {
			GameController.secondaryActivePiece = _acI;
			print("set active and secondary active player" + GameController.secondaryActivePiece);
			activeTimer = 0f;			
		}
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
