using UnityEngine;
using System.Net.Sockets;
using System;
using System.Xml;

public enum stepType
{
	nullStep,
	successStep,
	failureStep}
;

public enum PlayState
{
	UserMoveAgentColor,
	UserMoveBothColor,
	AgentMoveUserColor,
	AgentMoveBothColor,
	BothMoveAgentColor,
	BothMoveUserColor,
	BothMoveBothColor,
	Unknown,
};


public class GameController : MonoBehaviour
{
	public MoveValidator _moveValid;
	public static int turnNumber;
	//from 0 to 9
	public static AudioClip DClip;
	//The sound at the end of each step


	//keeps track of whose turn it is (inherently for two-player game)
	public static IPlayerHandler active_player;
	public static IPlayerHandler inactive_player;

	public static int totalStepNum;
	
	public static AudioSource audioP;
	public static stepType thisStep;


	public static int activePiece{ get; private set; }
	public static int secondaryActivePiece;


	private static float activeTimer;
	private static float secondaryActiveTimer;


	// Time for the count down
	public static float GameTimer{ get; private set; }
	public static float gameLen{ get; private set; }
	
	
	public static bool flag;
	private int blockNumber = 7;
	public static PlayState curPlayState;
	public static string targetTName;
	private LogTimeData logTimeDataScript;
	
	//--------- networking vars ---------
	public static Server _server{ get; private set; }
	private static TcpClient _tcpclient;
	

	// Use this for initialization
	void Start()
	{
		_moveValid = new MoveValidator();
		DClip = Resources.Load("DING") as AudioClip;
		audioP = GetComponent<AudioSource>();
		totalStepNum = 7;
		thisStep = stepType.nullStep;
		activePiece = -1;
		logTimeDataScript = gameObject.GetComponent<LogTimeData>();
		print("secondary active2 " + secondaryActivePiece);
		secondaryActivePiece = -1;
		curPlayState = PlayState.Unknown;
		
		//--------------- initialize networking ------------------
		_server = new Server();
		_server.Start(this);
	}
	
	public static void resetGameTimer()
	{
		GameTimer = gameLen;
	}

	//GameController should be initialized for each game
	//gameInitialization depends on the feature of each game
	//Initilize all the blocks
	public static void gameInitialization()
	{
		//get the game configuration
		string gameName = MainInfo.getAssignmentName();
		string targetName = MainInfo.getTargetName();

		//initialize parameters
		GameInfo.InitializeParameters();
		GameInfo.setTarget(targetName);

		//read the feature for the current node 
		string curNodeStr = "";
		string otherNodeStr = "";
		if (MainController.curNode == NODE.BLACK_NODE) {
			curNodeStr = "first";
			otherNodeStr = "second";
		} else {
			curNodeStr = "second";	
			otherNodeStr = "first";
		}
		//record current node information
		TaskConfNode(gameName, curNodeStr, otherNodeStr);
		//Start the first step
		turnNumber = -1;
		targetTName = targetName + "T";
		Vector3 targetTPosition = GameObject.Find(targetTName).transform.position;
		targetTPosition.y = -2f;
		targetTPosition.z = (float)GameInfo.initialPositionArray[targetTName];
		GameObject.Find(targetTName).transform.position = targetTPosition;
		setTargetPosition(targetName);
		
		//set players
		MainController.setPlayer();
		//initialize active player as the black player
		active_player = MainController.black_player;
		inactive_player = MainController.white_player;
		print("Active player set as " + MainController.WhoIs(MainController.black_player));
		

		GameInfo.switchTimer = GameInfo.switchLen; 
		print("secondary active3 " + GameController.secondaryActivePiece);
		secondaryActivePiece = -1;
		
		//log all game start params
		LogTimeData.logParams_startGame();
	}


	public static void setTargetPosition(string targetName)
	{
		//move the target object to the visible area
//		GameObject targetObj = GameObject.Find (targetName);
//		targetObj.transform.position = new Vector3 (200f,0f,-155f);
		setPosition(targetTName);

		//show where to put the blocks
		for (int i = 0; i < 7; ++i) {
			Vector3 ePosition = GameInfo.getTargetPosition(i);
			float eRotation = GameInfo.getTargetRotation(i);
			GameObject eObject = GameInfo.getSmoothBlock(i);
			EdgeDetectionShader eInst = eObject.GetComponent<EdgeDetectionShader>();
			eInst.showEdge(ePosition, eRotation);
		}
	}

	public static void setPosition(string targetName)
	{
//		print ("target name is: " + targetName);
		GameObject targetObj = GameObject.Find(targetTName);	//reset target position and rotation	
		Transform TargetTransform = targetObj.transform;
		Transform ObjectCollection = targetObj.GetComponentInChildren<Transform>();
		Vector3[] targetPositionArray = new Vector3[7];
		foreach (Transform aObject in ObjectCollection) {
//			print ("set the target position" + targetName + ", " + aObject.name);
			float rotationY = TargetTransform.localEulerAngles.y + aObject.localEulerAngles.y;
			GameInfo.setTargetBlockRotation(aObject.name, rotationY);
			Vector3 targetBlockPosition = TargetTransform.localPosition + aObject.localPosition;
			targetBlockPosition.y = 1f;
			GameInfo.setTargetBlockPosition(aObject.name, targetBlockPosition);
		}
	}

	//reads node info from XML file, determines conditions/params of game
	public static void TaskConfNode(string fileName, string curNodeStr, string otherNodeStr)
	{
		//print ("fileName is: " + fileName);
		TextAsset AssignmentXmlFile;
		XmlDocument XmlDoc = new XmlDocument();
		AssignmentXmlFile = (TextAsset)Resources.Load(fileName, typeof(TextAsset));
		XmlDoc.LoadXml(AssignmentXmlFile.text);
		//set timer info
		XmlNodeList timerList = XmlDoc.SelectNodes("/Assignment/timer");
		gameLen = (float)System.Convert.ToDouble(timerList[0]["timer"].InnerXml);
		//read the color information
		XmlNodeList colorList = XmlDoc.SelectNodes("/Assignment/color");
		foreach (XmlNode colorNode in colorList) {
			if (colorNode[curNodeStr].InnerXml == "True")
				GameInfo.setColorful();
			else
				GameInfo.setNoColor();

			if (colorNode[otherNodeStr].InnerXml == "True")
				GameInfo.setOtherColorful();
			else
				GameInfo.setOtherNoColor();
		}
		
		//read the step control information
		int stepIndex = 0;
		XmlNodeList stepList = XmlDoc.SelectNodes("/Assignment/steps/step");
		foreach (XmlNode stepNode in stepList) {
			GameInfo.setStepControl(stepIndex, (Convert.ToInt16(stepNode[curNodeStr].InnerXml) == 1));
			GameInfo.setOtherStepControl(stepIndex, (Convert.ToInt16(stepNode[otherNodeStr].InnerXml) == 1));
			stepIndex++;
		}
		
		//read the first hint information
		XmlNodeList Hint1List = XmlDoc.SelectNodes("/Assignment/hint1");
		foreach (XmlNode stepNode in Hint1List) {
			GameInfo.setHintsInfor(0, stepNode[curNodeStr].InnerXml);
			GameInfo.loadAudio(0, stepNode[curNodeStr + "Audio"].InnerXml);
		}
		
		//read the second hint information
		XmlNodeList Hint2List = XmlDoc.SelectNodes("/Assignment/hint2");
		foreach (XmlNode stepNode in Hint2List) {
			GameInfo.setHintsInfor(1, stepNode[curNodeStr].InnerXml);
			GameInfo.loadAudio(1, stepNode[curNodeStr + "Audio"].InnerXml);
		}
	}

	//decides when to switch agent-local to networked-local and vice-versa
	public static float agent_player_switch_timer {
		get;
		private set;
	}
	private const float GAME_LENGTH = 80;

	// Update is called once per frame
	void Update()
	{
		if (MainController.FSM.IsInState(PuzzleState.GAME_INITIALIZATION)) {
			GameInfo.switchTimer -= Time.deltaTime;
			if (GameInfo.switchTimer < 0) {
				//Finish all initilization
				MainController.FSM.Fire(Trigger.startStep);
				LanguageManager.DMFSM.Fire(DMTrigger.GameStart);
				return;
			}
		}

		//During the game
		if (MainController.FSM.IsInState(PuzzleState.GAME_STEP)) {
			//Set the moving target
			setTargetPosition(MainInfo.getTargetName());

			//game timer
			GameTimer -= Time.deltaTime;

			//agent timer (2 minutes)
			agent_player_switch_timer += Time.deltaTime;
			//switch regularly
			if (agent_player_switch_timer > GAME_LENGTH) {
				//reset timer
				agent_player_switch_timer = 0;
				//reset num games played for current assignment
				MainController.numGamesPlayed = 0;

				//switch whether agent active
				MainController.setisAgentActive(!MainController.isAgentActive);
                
				if (MainController.isAgentActive)
					Debug.Log("SWITCHED: Agent player is active.");
				else
					Debug.Log("SWITCHED: Agent player is not active.");

				MainController.FSM.Fire(Trigger.endGame);
			} else if (agent_player_switch_timer % 3 < .015f)
				Debug.Log("TIMER: " + agent_player_switch_timer);

            
			if (GameTimer <= 0) {
				//automatically move a block
				print("reset secondary active 10");
				GameController.secondaryActivePiece = -1;
				GameInfo.setSucceed(GameInfo.PreActiveBlock);
				thisStep = stepType.failureStep;
				LogTimeData.setEvent(LogTimeData.stepFailEvent);
				MainController.FSM.Fire(Trigger.endStep);
				return;
			} else {

				if (MainController._localPlayer.getActivePiece() != -1) {
//					secondaryActivePiece = MainController._localPlayer.getActivePiece();
					secondaryActiveTimer = 3f;
				} else {
					if (secondaryActivePiece != -1) {
						secondaryActiveTimer -= Time.deltaTime;
						if (secondaryActiveTimer < 0) {
							secondaryActivePiece = -1;
							print("secondary active4 " + secondaryActivePiece);
						}
					}
				}
				

				activePiece = active_player.getActivePiece();

				int id = MainController.isAgentActive ? MainController._agentPlayer.getActivePiece() : MainController._networkedPlayer.getActivePiece();

				if (activePieceChanged && id != -1) {
					//if block succeeded, do not incorporate synced changes
					if (GameInfo.blockSucceed[id])
						return;
					GameObject activeObject = GameInfo.blockList[id];
					//NEW: reduce flicker by setting player pos/orient to match active block
					//flicker caused by player pos/orient matching last active block before packet update

					MainController._networkedPlayer.setPosition(activeObject.transform.position);
					MainController._networkedPlayer.setOrientation(activeObject.transform.localEulerAngles);
					
					//reset flag
					activePieceChanged = false;
					newOrient = activeObject.transform.localEulerAngles.y;
				}
				if (id != -1) {
					//if block succeeded, do not incorporate synced changes
					if (GameInfo.blockSucceed[id])
						return;
					GameObject activeObject = GameInfo.blockList[id];
					//actually rotate block
					float curOrient = activeObject.transform.localEulerAngles.y;
					activeObject.transform.Rotate(Vector3.up * (newOrient - curOrient));
				}
				
				
				bool valid = _moveValid.isMoveValid(activePiece);
				if (!valid) {
					if (LogTimeData.getPreEvent() == LogTimeData.moveStartEvent) {
						LogTimeData.setEvent(LogTimeData.moveEndEvent);
					}
					return;	
				}
				
				if (LogTimeData.getPreEvent() != LogTimeData.moveStartEvent) {
					LogTimeData.setEvent(LogTimeData.moveStartEvent);
				}
				GameInfo.PreActiveBlock = activePiece;
				GameInfo.blockList[activePiece].transform.position = active_player.getPosition();
				
				
				//if (twoplayerposchanged)
				if (GameController.active_player == MainController._twoPlayers
				    && twoPlayerPos.magnitude > .1) {
					GameObject obj = GameInfo.blockList[MainController._twoPlayers.getActivePiece()];
					obj.transform.position = twoPlayerPos;
					twoplayerposchanged = false;
				}

				//check the success of the block
				if (_moveValid.isSucceeded(activePiece)) {
					//send block succeed message to networked player
					string message = "blocksuccess: " + activePiece;
					GameController.SyncGame_command(message);
					
					secondaryActivePiece = -1;					
					LogTimeData.setEvent(LogTimeData.stepSuccessEvent);
					GameInfo.setSucceed(activePiece);
					
					thisStep = stepType.successStep;
					MainController.FSM.Fire(Trigger.endStep);
				}
				
				//sync block success from networked player
				if (blocksuccess_index > -1 && !GameInfo.blockSucceed[blocksuccess_index]) {

					secondaryActivePiece = -1;
					LogTimeData.setEvent(LogTimeData.stepSuccessEvent);
					GameInfo.setSucceed(blocksuccess_index);
					
					//reset timer
					//resetGameTimer();
					
					thisStep = stepType.successStep;
					MainController.FSM.Fire(Trigger.endStep);
					
					//reset
					blocksuccess_index = -1;
				}

			}
			if (LogTimeData.getPreEvent() == LogTimeData.moveStartEvent) {
				LogTimeData.setEvent(LogTimeData.moveEndEvent);
			}
		}
	}

	public static void finishOneStep()
	{
		//step 2. play the audio
		audioP.PlayOneShot(DClip);
		
		//step 3: trigger end the step
		if (turnNumber < totalStepNum - 1) {
			MainController.FSM.Fire(Trigger.startStep);
            
		} else {
			if (agent_player_switch_timer < GAME_LENGTH - 2) {
				//restart game
				MainController.curGameNum--;
				MainController.numGamesPlayed++;
				//TODO: randomize next target
				LogTimeData.setEvent(LogTimeData.repeatTaskEvent);
				
			}
			MainController.resetAllActivePieces();
			//start next game
			LanguageManager.DMFSM.Fire(DMTrigger.GameEnd);
			MainController.FSM.Fire(Trigger.endGame);
		}
	}


	public static void stepStart()
	{	
		++turnNumber;	
		flag = true;
		resetGameTimer();
		//switch user at each step
		DetectPlayState();
		//assign pre active block
		for (int i = 0; i < GameInfo.blockNumber; ++i) {
			if (!GameInfo.blockSucceed[i]) {
				GameInfo.PreActiveBlock = i;			
			}	
		}
		if (turnNumber > 0 && LanguageManager.DMFSM.IsInState(DMState.DMRount)) {
			LanguageManager.DMFSM.Fire(DMTrigger.Done);
		}
		secondaryActivePiece = -1;
		print("stepstart(), secondary active = " + secondaryActivePiece);
		LanguageManager.feedbackTimer0 = LanguageManager.TimerLen0;		
		LanguageManager.curIntention = intention.IntentNone;
		LanguageManager.correctNum = 0;
		LogTimeData.setEvent(LogTimeData.stepStartEvent);
	}

	public static void DetectPlayState()
	{
		if (GameInfo.stepControl[turnNumber] && GameInfo.otherStepControl[turnNumber]) {
			//two player condition
			print("ACTIVE: two player");
			//MainController._twoPlayers.setPlayers(MainController._localPlayer, MainController._agentPlayer);
			MainController._twoPlayers.setPlayers(MainController.white_player, MainController.black_player);
			active_player = MainController._twoPlayers;
			if (GameInfo.NoColorTaskFlag) {				
				curPlayState = PlayState.BothMoveAgentColor;
			} else {
				if (GameInfo.OtherNoColorTaskFlag) {
					curPlayState = PlayState.BothMoveUserColor;
				} else {
					curPlayState = PlayState.BothMoveBothColor;
				}
			}
			LogTimeData.setActivePlayer("p1_and_p2");

		} else {
			if (GameInfo.stepControl[turnNumber]) {
				//active_player = MainController._localPlayer;
				active_player = MainController.white_player;

				//color sharing with color
				if (GameInfo.NoColorTaskFlag) {
					curPlayState = PlayState.UserMoveAgentColor;
				} else {
					curPlayState = PlayState.UserMoveBothColor;
				}
				LogTimeData.setActivePlayer("p1");
			} else if (GameInfo.otherStepControl[turnNumber]) {

				//active_player = MainController._agentPlayer;	
				active_player = MainController.black_player;

				//color sharing without color
				if (GameInfo.OtherNoColorTaskFlag) {
					curPlayState = PlayState.AgentMoveUserColor;
				} else {
					curPlayState = PlayState.AgentMoveBothColor;
				}
				LogTimeData.setActivePlayer("p2");
			}

			//log player type
			print("ACTIVE: " + MainController.WhoIs(active_player).ToUpper());

		}
		LogTimeData.setStepIndex(turnNumber);
	}

	public static void shuffle(int ID)
	{
		int[] tempSorting = new int[3];
		int sortingSize = 1;
		tempSorting[0] = ID;
		// Randomize array
		for (int i = sortingSize - 1; i > 0; --i) {
			
			int j = (int)Mathf.Round(UnityEngine.Random.Range(0f, (float)i));
			
			int tempID = tempSorting[i];
			tempSorting[i] = tempSorting[j];
			tempSorting[j] = tempID;
		}
	}


	/* ------------------------------------------------------------------------------
	 * Networking code below
	 * ------------------------------------------------------------------------------ 
	 */
	

	//temporary stub
	public static void HandleInputAction(string sync_info)
	{
		//command other game instances to sync.
		SyncGame_command(sync_info);
	}


	public static void SyncGame_command(string sync_info)
	{
		if (_tcpclient == null) {
			Debug.Log("Failed to sync because tcp client is not initialized.");
			return;
		}

		//if agent active, IGNORE networked player
		if (MainController.isAgentActive && !sync_info.Contains("readyFlag"))
			return;

		Packet syncPacket = new Packet("sync", sync_info);
		Packet.SendPacket(_tcpclient.GetStream(), syncPacket);
	}
	
	//stores previous orientation, actually rotate in game loop
	static float newOrient;
	//store whether active piece changed --> primarily to reduce flicker caused by slight network delay
	public static bool activePieceChanged = false;
	//store whether block has succeeded on networked player's computer
	public static int blocksuccess_index = -1;
	
	static Vector3 twoPlayerPos = Vector3.zero;
	static bool twoplayerposchanged = false;
	
	// obey with an order to sync game
	public static void SyncGame_obey(string sync_info)
	{
		//Debug.Log("INFO: " + sync_info);
		
		// Parse "sync_info":
		// Rules:
		// key-value entry pairs set off by semicolon delimiter
		// each entry is in the format of "key:value" (trim spaces?)
		if (sync_info.Length == 0)
			return;

		//if agent active, IGNORE networked player
		if (MainController.isAgentActive && !sync_info.Contains("readyFlag"))
			return;
		
		if (!MainController.FSM.IsInState(PuzzleState.GAME_STEP) && !sync_info.Contains("readyFlag"))
			return;

		string[] entries = sync_info.Split(';');
		Debug.Log("MESSAGE: " + sync_info);
		
		foreach (string entry in entries) {
			//trim all beginning/ending whitespace in key/value
			string key = sync_info.Split(':')[0].Trim();
			string value = sync_info.Split(':')[1].Trim();
			
			//note: key or value may be empty...
			if (key == null || value == null)
				throw new Exception("Sync info in improper format.");
			
			switch (key) {
			//sync ready flags
				case "readyFlag":
					bool flag = Convert.ToBoolean(value);
					MainController._networkedPlayer.setReadyFlag(flag);
					break;
			//sync active piece
				case "activePiece":
					int aci = Convert.ToInt16(value);
					MainController._networkedPlayer.setActivePiece(aci);
					activePieceChanged = true;
					break;
			//sync position
				case "position":
					string[] locs = value.Split(',');
					if (locs.Length != 3)
						throw new Exception("Position could not be parsed to type: UnityEngine.Vector3");
					Vector3 pos = new Vector3(float.Parse(locs[0]), float.Parse(locs[1]), float.Parse(locs[2]));
					MainController._networkedPlayer.setPosition(pos);
					break;
			//sync orientation
				case "orientation":
					string[] orients = value.Split(',');
					if (orients.Length != 3)
						throw new Exception("Orientation could not be parsed to type: UnityEngine.Vector3");
					Vector3 orient = new Vector3(float.Parse(orients[0]), float.Parse(orients[1]), float.Parse(orients[2]));
					
					//set rotation
					MainController._networkedPlayer.setOrientation(orient);
					newOrient = orient.y;
					break;
			
				case "time":
					long time = Convert.ToInt32(value);
					long now = DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
					Debug.Log("Packet travel time: " + (now - time));
					break;
				
			//sync block success
				case "blocksuccess":
					int id = Convert.ToInt32(value);
					blocksuccess_index = id;
					Debug.Log("RECEIVED BLOCK SUCCESS: " + id);
					break;
					
				case "twoplayerpos":
					string[] twolocs = value.Split(',');
					if (twolocs.Length != 4)
						throw new Exception("Two player position could not be parsed to type: UnityEngine.Vector3");

					if (Convert.ToInt32(twolocs[3]) == MainController._localPlayer.getActivePiece()
					    && MainController._localPlayer.getActivePiece() > -1) {
						twoPlayerPos = new Vector3(float.Parse(twolocs[0]), float.Parse(twolocs[1]), float.Parse(twolocs[2]));
						twoplayerposchanged = true;
					}
					break;
					
			//if nothing matches, should throw error
				default:
					Debug.Log("Syncing error: No variable match found to sync.");
					break;
			}
		}
	}
	
	// Adds a networked player to the game
	public static bool AddTcpClient(TcpClient client)
	{
		// Make sure only one player was added
		if (_tcpclient == null) {
			_tcpclient = client;
			return true;
		}
		return false;
	}
	void OnApplicationQuit()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
		//shut down server
		_server.Shutdown();
	}

}
