using UnityEngine;
using System;

public class LocalPlayer : MonoBehaviour, IPlayerHandler
{
	public static int activePiece { get; private set; }

	public static Vector3 curPosition { get; private set; }
	private static Vector3 diff;
	//last sent position
	private static Vector3 lastSentPos;
	//last sent orientation
	private static Vector3 lastSentOrientation;

	public static Vector3 curOrientation { get; private set; }
	public static bool readyFlag { get; private set; }
	public static float activeTimer;

	void Start()
	{
		setActivePiece(-1);
	}
	public void setReadyFlag(bool val)
	{
		readyFlag = val;

		//GameController
		//send ready flag immediately after setting
		sendReadyFlag();
	}
	public bool isReady()
	{
		return readyFlag;
	}
	private static void sendReadyFlag()
	{
		//send the player is ready to the other player
		string message = "readyFlag: true";
		GameController.SyncGame_command(message);
		Debug.Log("Local player broadcasted that it is ready.");
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
			print("set active and secondary active piece: " + _acI);
			activeTimer = 0f;
		}
		activePiece = _acI;
		//reset diff
		diff = Vector3.zero;

		sendActivePiece();
	}
	private static void sendActivePiece()
	{
		//send player's active piece
		string message = "activePiece: " + activePiece;
		GameController.SyncGame_command(message);
	}

	public Vector3 getPosition()
	{
		return curPosition;
	}
	public Vector3 getDiff()
	{
		return diff;
	}

	bool diffIsReset = false;
	public void setPosition(Vector3 newposition)
	{
		diff = newposition - curPosition;
		curPosition = newposition;
		//prevent abrupt jumps
		if (Math.Abs(diff.x) + Math.Abs(diff.z) > 50)
			diff = Vector3.zero;
		//only send if large enough delta
		if (manhattanDist(lastSentPos, curPosition) > 1) {
			sendPosition();
		}
		if (diff.magnitude > .01) {
			sendDiff();
			diffIsReset = false;
		}
		//send zero diff once only
		else if (!diffIsReset) {
			sendDiff();
			diffIsReset = true;
		}
	}
	//send player position over networking
	public void sendPosition()
	{
		string message = "position: " + curPosition.x + "," + curPosition.y + "," + curPosition.z;
		GameController.SyncGame_command(message);
		lastSentPos = curPosition;
	}
	private void sendDiff()
	{
		if (GameController.active_player != MainController._twoPlayers)
			return;
		string message = "diff: " + diff.x + "," + diff.y + "," + diff.z;
		GameController.SyncGame_command(message);
	}
	
	public Vector3 getOrientation()
	{
		return curOrientation;
	}
	//change orientation by delta
	public void changeOrientationY(float deltaY)
	{
		curOrientation = new Vector3(curOrientation.x, curOrientation.y + deltaY, curOrientation.z);
		trySendOrientation();
	}
	//set orientation
	public void setOrientation(Vector3 neworient)
	{
		curOrientation = neworient;
		trySendOrientation();
	}
	//send player orientation over networking
	private void trySendOrientation()
	{
		//smart: only send if large enough delta
		if (Math.Abs(curOrientation.y - lastSentOrientation.y) > 2) {
			sendOrientation();
		}
	}
	public void sendOrientation()
	{
		string message = "orientation: " + curOrientation.x + "," + curOrientation.y + "," + curOrientation.z;
		GameController.SyncGame_command(message);
		lastSentOrientation = curOrientation;
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
	
	//helper method:
	//calculates Manhattan Distance (rectilinear distance), faster than Pythagorean dist
	private float manhattanDist(Vector3 a, Vector3 b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
	}
}
