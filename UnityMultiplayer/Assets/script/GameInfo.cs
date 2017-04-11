using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//This class is used to store static information of each game for each node
public static class GameInfo
{
	public const int blockNumber = 7;
	public static string[] blockNameArray;
	public static float[] initialRotationArray;
	private static GameObject SelectionPlan;
	private static Vector3 SelectionPosition;
	public static String[] hintsInfor;

	//color for two players
	public static bool[] ColorControl;
	public static bool[] OtherColorControl;

	//step control for two players
	public static bool[] stepControl;
	public static bool[] otherStepControl;

	//flags for two players
	public static bool NoColorTaskFlag;
	public static bool OtherNoColorTaskFlag;


	private static bool[] rotationHitFlagArray;
	public static Vector3[] blockPositionArray;
	public static Hashtable initialPositionArray;
	public static Hashtable gapHT;
	private static bool[] SelectFlagArray;
	private static string currTarget;
	public static bool[] blockSucceed;
	public static float[] targetRotationArray;
	private static bool[] resetPositionFlagArray;



	private static float stepTimer;
	private static float INFTIMER = 2400F;
	public static AudioClip[] audioClips;
	private static int NODEID;
	public static GameObject[] blockList;
	public static GameObject[] blockSmoothList;
	public static List<int> RandomList = new List<int>();
	private static string[] smoothBlockNameArray;
	public static int PreActiveBlock;
	public static float switchTimer;
	public const float switchLen = 4f;
	public static string[] blockNameStr;
	public static string[] blockNameStrOther;
	public static List<List<string>> blockColorNameList;
	//triangle, square, diamond, rectangle, quadrilateral
	//list of lists
	public static List<List<string>> blockShapeNameList;
	public const float activeLen = 3f;

	//involved in what blocks agent can see
	public static int[] peerList;
	public static int[] agentColorVisible;


	public static void NodeInfoInitialization()
	{
		blockNameArray = new string[blockNumber];
		for (int i = 0; i < blockNumber; i++)
			blockNameArray[i] = "block" + (1 + i);

		peerList = new int[blockNumber];
		peerList[0] = 1;
		peerList[1] = 0;
		peerList[2] = 4;
		peerList[3] = -1;
		peerList[4] = 2;
		peerList[5] = -1;
		peerList[6] = -1;

		agentColorVisible = new int[blockNumber];

		//define the color of each block	
		blockColorNameList = new List<List<string>> {
			//block0
			new List<string>{ "red" },
			//block1
			new List<string>{ "green" },
			//block2
			new List<string>{ "pink", "magenta", "purple" },
			//block3
			new List<string>{ "yellow" },
			//block4
			new List<string>{ "orange" },
			//block5
			new List<string>{ "blue", "navy" },
			//block6
			new List<string>{ "gray", "grey" },
		};
		
		//define the shape of each block
		blockShapeNameList = new List<List<string>> {
			//block0
			new List<string>{ "triangle" },
			//block1
			new List<string>{ "triangle" },
			//block2
			new List<string>{ "triangle" },
			//block3
			new List<string>{ "rectangle", "square", "diamond", "quadrilateral" },
			//block4
			new List<string>{ "triangle" },
			//block5
			new List<string>{ "parallelogram", "quadrilateral", "rhombus" },
			//block6
			new List<string>{ "triangle" },
		};


		smoothBlockNameArray = new string[blockNumber];
		for (int i = 0; i < blockNumber; i++)
			smoothBlockNameArray[i] = "block" + (1 + i) + "Smooth";

		initialRotationArray = new float[blockNumber];
		initialRotationArray[0] = 135.0f;
		initialRotationArray[1] = 135.0f;
		initialRotationArray[2] = 0.0f;
		initialRotationArray[3] = 0.0f;
		initialRotationArray[4] = 0.0f;
		initialRotationArray[5] = 0.0f;
		initialRotationArray[6] = 270.0f;

		//basically z-offset (of the entire target area)
		initialPositionArray = new Hashtable();
		initialPositionArray.Add("Target1T", -146f);
		initialPositionArray.Add("Target2T", -152f);
		initialPositionArray.Add("Target3T", -200f);
		initialPositionArray.Add("Target4T", -200f);
		initialPositionArray.Add("Target5T", -200f);
		initialPositionArray.Add("Target6T", -200f);
		initialPositionArray.Add("Target7T", -200f);
		initialPositionArray.Add("Target8T", -200f);
		initialPositionArray.Add("Target9T", -200f);
		initialPositionArray.Add("Target10T", -200f);
		initialPositionArray.Add("Target11T", -200f);

		
		gapHT = new Hashtable();
		gapHT.Add("Target1T", 52f);
		gapHT.Add("Target2T", 46f);
		gapHT.Add("Target3T", 2f);
		gapHT.Add("Target4T", 2f);
		gapHT.Add("Target5T", 2f);
		gapHT.Add("Target6T", 2f);
		gapHT.Add("Target7T", 2f);
		//custom vertical offsets
		gapHT.Add("Target8T", -3f);
		gapHT.Add("Target9T", -3f);
		gapHT.Add("Target10T", -1.5f);
		gapHT.Add("Target11T", -4f);

		blockSmoothList = new GameObject[blockNumber];
		for (int i = 0; i < blockNumber; i++)
			blockSmoothList[i] = GameObject.Find("block" + (1 + i) + "Smooth");

		blockList = new GameObject[blockNumber];
		for (int i = 0; i < blockNumber; i++)
			blockList[i] = GameObject.Find("b" + (1 + i));

		//color information	
		blockNameStr = new string[blockNumber];
		blockNameStr[0] = "two";
		blockNameStr[1] = "three";
		blockNameStr[2] = "four";
		blockNameStr[3] = "five";
		blockNameStr[4] = "six";
		blockNameStr[5] = "seven";
		blockNameStr[6] = "eight";

		blockNameStrOther = new string[blockNumber];
		for (int i = 0; i < blockNumber; i++)
			blockNameStrOther[i] = (i + 2).ToString();

		SelectionPlan = GameObject.Find("SelectionArea");
		SelectionPosition = SelectionPlan.transform.position;
		hintsInfor = new string[2];
		audioClips = new AudioClip[2];
		for (int i = 0; i < blockNumber; ++i) {
			RandomList.Add(i);
		}



		ColorControl = new bool[blockNumber];
		OtherColorControl = new bool[blockNumber];
		stepControl = new bool[blockNumber];
		otherStepControl = new bool[blockNumber];
		resetPositionFlagArray = new bool[blockNumber];
		targetRotationArray = new float[blockNumber];
		NoColorTaskFlag = false;
		OtherNoColorTaskFlag = false;
		rotationHitFlagArray = new bool[blockNumber];
		blockSucceed = new bool[blockNumber];
	}

	//called by GameController.gameInitialization();
	public static void InitializeParameters()
	{
		//two hints are included for the game
		hintsInfor[0] = "";
		hintsInfor[1] = "";
		blockPositionArray = new Vector3[blockNumber];
		SelectFlagArray = new bool[blockNumber];
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			SelectFlagArray[blockIndex] = true;
			resetPositionFlagArray[blockIndex] = true;
			blockPositionArray[blockIndex] = Vector3.zero;
			targetRotationArray[blockIndex] = initialRotationArray[blockIndex];
			rotationHitFlagArray[blockIndex] = false;
			blockSucceed[blockIndex] = false;
			agentColorVisible[blockIndex] = 0;
		}
		Shuffle();
		currTarget = "Target7";

		//infinite timer?
		stepTimer = INFTIMER;
		PreActiveBlock = -1;
		blockReset();

	}

	
	public static void blockReset()
	{
		for (int i = 0; i < blockNumber; ++i) {
			blockList[i].transform.position = new Vector3(180f, 0f, -145f - 10f * RandomList[i]);
			blockList[i].transform.localEulerAngles = new Vector3(0f, initialRotationArray[i], 0f);
		}
	}

	//------------------------ SET COLOR CONTROLS -----------------------------

	public static void setColorful()
	{
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			ColorControl[blockIndex] = true;
		}
		NoColorTaskFlag = false;
	}
	public static void setNoColor()
	{
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			ColorControl[blockIndex] = false;
		}
		NoColorTaskFlag = true;
	}
	public static void setOtherColorful()
	{
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			OtherColorControl[blockIndex] = true;
		}
		OtherNoColorTaskFlag = false;
	}
	public static void setOtherNoColor()
	{
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			OtherColorControl[blockIndex] = false;
		}
		OtherNoColorTaskFlag = true;
	}
	//----------------------------------------------------------------------


	public static void setSucceed(int blockI)
	{
		GameObject suObj = blockList[blockI];
		//set color
		suObj.renderer.material.mainTexture = suObj.GetComponent<BlockController>().defaultTexture;	
		//set location
		suObj.transform.position = new Vector3(blockPositionArray[blockI].x, blockPositionArray[blockI].y, blockPositionArray[blockI].z);
		//set rotation
		Vector3 tempRotation = suObj.transform.localEulerAngles;
		tempRotation.y = targetRotationArray[blockI];
		suObj.transform.localEulerAngles = new Vector3(tempRotation.x, tempRotation.y, tempRotation.z);
		//remove plant 
		suObj.GetComponent<BlockController>().releaseName(RandomList[blockI]);
		
		blockSucceed[blockI] = true;
		/*
		string s = "";
		for (int i = 0; i < blockSucceed.Length; i++) {
			s += i + " - " + blockSucceed[i] + "\n";
		}
		Debug.Log(s);
		*/
		
//		Debug.Log ("secondary active6 " + GameController.secondaryActivePiece);
//		GameController.secondaryActivePiece = -1;
//		MainController._localPlayer.setActivePiece (-1);
	}
	public static GameObject getSmoothBlock(int id)
	{
		return blockSmoothList[id];
	}
	

	public static void setStepControl(int stepIdx, bool flag)
	{
		stepControl[stepIdx] = flag;
	}

	public static void setOtherStepControl(int stepIdx, bool flag)
	{
		otherStepControl[stepIdx] = flag;
	}

	public static void setHintsInfor(int InforI, string text)
	{
		hintsInfor[InforI] = text;
	}

	public static void loadAudio(int index, string audioName)
	{
		audioClips[index] = Resources.Load(audioName) as AudioClip;
	}

	public static void setTarget(string targetName)
	{
		currTarget = targetName;
	}

	public static void Shuffle()
	{
		int n = 7;
		/* Conditions for shuffling:
		 * 1. produce a different random list each repeat of the same game
		 * 2. produce synchronized, deterministic list for player-player games 
		 */
		System.Random rnd = new System.Random(MainController.RANDOM_SEED);

		//reset random list first
		for (int i = 0; i < RandomList.Count; i++) {
			RandomList[i] = i;
		}


		while (n > 1) {
			int k = (rnd.Next(0, n));
			n--;
			int value = RandomList[k];
			RandomList[k] = RandomList[n];
			RandomList[n] = value;
		}
	}

	public static Vector3 getTargetPosition(int ID)
	{	
		return blockPositionArray[ID];
	}

	public static float getTargetRotation(int ID)
	{	
		return targetRotationArray[ID];
	}

	public static string getTarget()
	{
		return currTarget;
	}

	//the target rotation
	public static void setTargetBlockRotation(string blockName, float rotationY)
	{
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			if (blockNameArray[blockIndex] == blockName) {
				targetRotationArray[blockIndex] = rotationY;
			}
		}
	}

	//sets target block position
	public static void setTargetBlockPosition(string blockName, Vector3 positionT)
	{
//		Debug.Log (blockName + "'s target position is: " + positionT.x.ToString() + ", " + positionT.z);
		for (int blockIndex = 0; blockIndex < blockNumber; blockIndex++) {
			if (blockNameArray[blockIndex] == blockName) {
				blockPositionArray[blockIndex] = positionT;
			}
		}
	}

	public static int getObject(Hashtable _ht)
	{
		Debug.Log("start detect object ");
		//initilize 
		float Wid = 15f;
		//color
		float Wcolor = 9f;
		//current active obj
		float Wactive = 10f;
		//success --> extreme neg weight
		float Wsucc = -80f;
		//last obj
		float Wlast = 10f;
		//secondary active obj
		float Wsec = 9f;
		
		//shape
		float Wshape = 8f;
		
		
		float[] objectWeights = new float[blockNumber];
		for (int i = 0; i < blockNumber; ++i) {
			objectWeights[i] = 0f;
		}

		//calculate weight based on ID
		if (_ht["id"].ToString() != "Requ") {
			string name = _ht["id"].ToString();
			int id = -1;
			for (int i = 0; i < blockNumber; ++i) {
				if (blockNameStr[i] == name || blockNameStrOther[i] == name) {
					id = i;
					break;
				}
			}

			if (id != -1) {
				for (int i = 0; i < 7; ++i) {
					if (RandomList[i] == id) {
						objectWeights[i] += Wid;
					} else {
						objectWeights[i] -= Wid;
					}
				}
			}
		}

		//calculate weight based on active block
//		if(GameController.active_player == MainController._localPlayer || GameController.active_player == MainController._twoPlayers){
		int activeObj = LocalPlayer.activePiece;

		if (activeObj != -1) {
			objectWeights[activeObj] += Wactive;
		} else {			
			if (GameController.secondaryActivePiece != -1) {
				objectWeights[GameController.secondaryActivePiece] += Wsec;
			}		
		}
//		}

		//calculate weight based on color		
		if (_ht["color"].ToString() != "Requ") {
			string objColor = _ht["color"].ToString();
			for (int i = 0; i < 7; ++i) {
				//give the color name
				if (blockColorNameList[i].Contains(objColor)) {
					objectWeights[i] += Wcolor;
				} else {
					objectWeights[i] -= Wcolor;
				}
			}
		}

		//calculate weight for block already succeeded (negative punishment)
		for (int i = 0; i < blockNumber; ++i) {
			if (blockSucceed[i]) {
				objectWeights[i] += Wsucc;
			}
		}
		
		//calculate weight based on shape
		if (_ht["shape"].ToString() != "Requ") {
			string shape = _ht["shape"].ToString();
			for (int i = 0; i < 7; ++i) {
				//give the color name
				if (blockShapeNameList[i].Contains(shape)) {
					objectWeights[i] += Wshape;
				} else {
					objectWeights[i] -= Wshape;
				}
			}
		}

		//calculate weight based on whether is the last block
		int succCounter = 0;
		for (int i = 0; i < blockNumber; ++i) {
			if (blockSucceed[i]) {
				++succCounter;		
			}
		}
		if (succCounter == 6) {
			for (int i = 0; i < blockNumber; ++i) {
				if (!blockSucceed[i]) {
					objectWeights[i] += Wlast;
				}
			}
		}


		//return the block with highest weight
		float maxWeight = 0f;
		int theBlockIdx = -1;
		for (int i = 0; i < blockNumber; ++i) {
			if (objectWeights[i] > maxWeight) {
				maxWeight = objectWeights[i];
				theBlockIdx = i;
			}
		}

		//calculate weight based on block state
//		if(theBlockIdx != -1){
//			if(blockSucceed [theBlockIdx]){
//				return -1;		
//			}
//		} 
//		for (int i=0; i<blockNumber; ++i) {
//			if(blockSucceed[i]){
//				objectWeights[i] += Wsucc;
//			}
//		}
		//reset the secondary block
		if (theBlockIdx != -1) {
			Debug.Log("set secondary active block to -1 " + theBlockIdx);
			GameController.secondaryActivePiece = -1;		
		}
		return theBlockIdx;
	}

	public static int agentSeeColor(int blockI)
	{
		//HACK: prevents array index out of bounds exception
		if (blockI == -1) {
			Debug.Log("ERROR: blockI is a negative array index.");
			return -99999;
		}
		
		int peerBlock = peerList[blockI];
		if (peerBlock == -1 || (peerBlock != -1 && agentColorVisible[peerBlock] == 2) || blockSucceed[peerBlock] || blockSucceed[blockI]) {
			agentColorVisible[blockI] = 2;	 
		}

		return agentColorVisible[blockI];
	}
}
