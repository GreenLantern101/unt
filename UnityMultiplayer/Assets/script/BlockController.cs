﻿using UnityEngine;
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
		int b = gameObject.name.IndexOf('b') + 1;
		ID = Convert.ToInt16(gameObject.name.Substring(b)) - 1;
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
			if (GameController.active_player != MainController._localPlayer &&
			    GameController.active_player != MainController._twoPlayers)
				return;
			if (LocalPlayer.activePiece == ID) {
				//calculate the changed position
				Camera cam = Camera.main;
				Vector3 mousePos = Input.mousePosition;
				Vector3 curPosition = gameObject.transform.position;
				Vector3 camPosition = cam.transform.position;
				Vector3 newPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camPosition.y - curPosition.y));
				
				GameObject activeObject = GameInfo.blockList[ID];
				
				MainController._localPlayer.setPosition(newPosition);
				MainController._localPlayer.setOrientation(activeObject.transform.localEulerAngles);
				
				float delta = rotateRate * 10 * Time.deltaTime;

				//calculate the changed rotation and update cur player orientation
				if (Input.GetKey("left")) {
					MainController._localPlayer.changeOrientationY(-delta);
					activeObject.transform.Rotate(Vector3.down * delta);
				} else if (Input.GetKey("right")) {
					MainController._localPlayer.changeOrientationY(delta);
					activeObject.transform.Rotate(Vector3.up * delta);
				} else if (LocalPlayer.activePiece == ID || AgentPlayer.activePiece == ID) {
					//vibrating();
				}
				if (GameInfo.blockSucceed[ID]) {
					resetRotate();
				}
			}
		}
	}

	public void vibrating()
	{
		float rate = 10f;
		//cur rotate needs to be set to object orientation?
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
		plane = GameObject.Find("Plane" + (name + 1));
		Vector3 curPos = gameObject.transform.position;
		curPos.x = curPos.x - 10f;
		curPos.y = 1f;
		plane.transform.position = curPos;
	}

	public void relaseName(int name)
	{
		plane = GameObject.Find("Plane" + (name + 1));
		Vector3 curPos = new Vector3(-100f, -100f, -100f);
		plane.transform.position = curPos;
	}

	void OnMouseDown()
	{
		if (GameController.active_player != MainController._localPlayer &&
		    GameController.active_player != MainController._twoPlayers)
			return;
		
		print("Set active piece: " + ID);
		MainController._localPlayer.setActivePiece(ID);
		LogTimeData.setEvent(LogTimeData.dragStartEvent);
	}

	void OnMouseUp()
	{
		if (GameController.active_player != MainController._localPlayer &&
		    GameController.active_player != MainController._twoPlayers)
			return;
		
		MainController._localPlayer.setActivePiece(-1);
		LogTimeData.setEvent(LogTimeData.dragEndEvent);
	}
}