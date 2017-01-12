using UnityEngine;
using System.Collections;
using System;

public class BlockController : MonoBehaviour
{
	public int ID;
	public bool Ownership = false;
	//whether this block is owned by this node
	public int collided = 0;
	//The collided number
	private GameObject plane;
	public Texture grayTexture;
	public Texture defaultTexture;
	private float rotateRate;


	void Start()
	{
		assignID();
		rotateRate = 3f;
	}
	
	public void assignID()
	{
		switch (gameObject.name) {
			case "b1":
				ID = 0;
				break;
			case "b2":
				ID = 1;
				break;
			case "b3":
				ID = 2;
				break;
			case "b4":
				ID = 3;
				break;
			case "b5":
				ID = 4;
				break;
			case "b6":
				ID = 5;
				break;
			case "b7":
				ID = 6;
				break;
		}
	}

	void Update()
	{
		if (MainController.FSM.IsInState(PuzzleState.GAME)) {	
			if (!GameInfo.blockSucceed[ID]) {
				assignName(GameInfo.RandomList[ID]);
				if (!GameInfo.NoColorTaskFlag) {
					renderer.material.mainTexture = defaultTexture;	
				} else {
					renderer.material.mainTexture = grayTexture;	
				}
			} else {
				GameInfo.setSucceed(ID);
			}
		}


		if (MainController.FSM.IsInState(PuzzleState.GAME_STEP)) {

			if (LocalPlayer.activePiece == ID) {
				//calculate the changed position
				Camera cam = Camera.main;
				Vector3 mousePos = Input.mousePosition;
				Vector3 curPosition = gameObject.transform.position;
				Vector3 camPosition = cam.transform.position;
				Vector3 newPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camPosition.y - curPosition.y));
				MainController._localPlayer.setPosition(newPosition);

				//calcuate the changed rotation and update cur player oritation
				if (Input.GetKey("left")) {
					MainController._localPlayer.changeOrientationY(rotateRate * 10* Time.deltaTime);
					GameObject activeObject = GameInfo.blockList[ID];
					activeObject.transform.Rotate(Vector3.down * rotateRate * 10 * Time.deltaTime);
					//Debug.Log("LEFT ROTATION");
				} else if (Input.GetKey("right")) {
					MainController._localPlayer.changeOrientationY(rotateRate * 10 * Time.deltaTime);
					GameObject activeObject = GameInfo.blockList[ID];
					activeObject.transform.Rotate(Vector3.up * rotateRate * 10 * Time.deltaTime);
					//Debug.Log("RIGHT ROTATION");
				} else if (LocalPlayer.activePiece == ID || AgentPlayer.activePiece == ID) {
					vibrating();
				} else if (!GameInfo.blockSucceed[ID]) {
					resetRotate();
				}
			}
		}
	}

	public void vibrating()
	{
		float rate = 10f;
		float curRotate = transform.localEulerAngles.y;
		if (Mathf.Abs(curRotate - GameInfo.initialRotationArray[ID]) > 20f) {
			transform.Rotate(Vector3.up * rate);
		} else {
			transform.Rotate(Vector3.down * rate);			
		}
	}

	public void resetRotate()
	{
		transform.localEulerAngles = new Vector3(0f, GameInfo.initialRotationArray[ID], 0f);
	}

	public void assignName(int name)
	{
		switch (name) {
			case 0:
				plane = GameObject.Find("Plane1");
				break;
			case 1:
				plane = GameObject.Find("Plane2");
				break;
			case 2:
				plane = GameObject.Find("Plane3");
				break;
			case 3:
				plane = GameObject.Find("Plane4");
				break;
			case 4:
				plane = GameObject.Find("Plane5");
				break;
			case 5:
				plane = GameObject.Find("Plane6");
				break;
			case 6:
				plane = GameObject.Find("Plane7");
				break;
		}
		Vector3 curPos = gameObject.transform.position;
		curPos.x = curPos.x - 10f;
		curPos.y = 1f;
		plane.transform.position = curPos;
	}

	public void relaseName(int name)
	{
		switch (name) {
			case 0:
				plane = GameObject.Find("Plane1");
				break;
			case 1:
				plane = GameObject.Find("Plane2");
				break;
			case 2:
				plane = GameObject.Find("Plane3");
				break;
			case 3:
				plane = GameObject.Find("Plane4");
				break;
			case 4:
				plane = GameObject.Find("Plane5");
				break;
			case 5:
				plane = GameObject.Find("Plane6");
				break;
			case 6:
				plane = GameObject.Find("Plane7");
				break;
		}
		Vector3 curPos = new Vector3(-100f, -100f, -100f);
		plane.transform.position = curPos;
	}

	void OnMouseDown()
	{
		print("set active play" + ID);
		MainController._localPlayer.setActivePiece(ID);
		LogTimeData.setEvent(LogTimeData.dragStartEvent);
	}

	void OnMouseUp()
	{
		MainController._localPlayer.setActivePiece(-1);
		LogTimeData.setEvent(LogTimeData.dragEndEvent);
	}
}























