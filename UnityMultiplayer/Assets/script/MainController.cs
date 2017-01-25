using UnityEngine;
using System;
using System.IO;

public enum NODE
{
	BLACK_NODE,
	WHITE_NODE,
}

public class MainController : MonoBehaviour
{

	// all static because GLOBAL VARS
	public static LocalPlayer _localPlayer;
	public static AgentPlayer _agentPlayer;
	public static NetworkedPlayer _networkedPlayer;
	public static TwoPlayers _twoPlayers;

	//initialized here before Start(), so NetworkConnection can call it remotely
	public static StateMachine FSM = new StateMachine();


	public static bool isAgentActive;

	//may be local player, agent player, networked player, twoPlayer
	public static IPlayerHandler black_player;
	public static IPlayerHandler white_player;

	public static int curGameNum;

	//hard-coded into config file
	public static NODE curNode;


	public static Camera mainCam;
	public static Camera progressCam;

	
	private static int totalGameNum;
	
	//returns who is black_player or white_player
	public static String WhoIs(IPlayerHandler player)
	{
		if (player == MainController._twoPlayers)
			return "Two players";
		if (player == MainController._networkedPlayer)
			return "Networked Player";
		if (player == MainController._localPlayer)
			return "Local Player";
		if (player == MainController._agentPlayer)
			return "Agent Player";
		//default
		return "Unknown player type";
		
	}

	void Start()
	{	//The node start state
		
		string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/Assets/script/config.txt");
		
		if (lines.Length < 2)
			throw new Exception("Node info can't be parsed.");
		if (lines[2].Trim() == "black")
			curNode = NODE.BLACK_NODE;
		else if (lines[2].Trim() == "white")
			curNode = NODE.WHITE_NODE;
		else
			throw new Exception("Node info can't be parsed.");
		

		//sets agent to active (b/c currently player plays with agent)
		//set to false for networking, true for agent
		isAgentActive = false;		


		//initializes game
		GameInfo.NodeInfoInitialization();
		print("GameInfo: Node info initialized");
		print("NODE: " + curNode.ToString());


		curGameNum = 0;
		totalGameNum = 10;


		//create one instance of each player type, may or may not be actually used
		_localPlayer = new LocalPlayer();
		_agentPlayer = new AgentPlayer();
		_networkedPlayer = new NetworkedPlayer();
		_twoPlayers = new TwoPlayers();


		//set cameras
		mainCam = GameObject.Find("Main Camera").camera;
		progressCam = GameObject.Find("ProgressCamera").camera;
	}


	//is called at the end of game intro by state machine
	public static void sendPlayerReady()
	{
		_localPlayer.setReadyFlag(true);
		AgentPlayer.readyFlag = true;
		
		//somehow do something to readyflag of networked player...?
	}

	void Update()
	{

		//start game at end of intro
		if (FSM.IsInState(PuzzleState.INTRO_END)) {
			if (_localPlayer.isReady()) {
				//print ("Local player ready.");
				if (_networkedPlayer.isReady()) {
					print("Networked player ready.");
					FSM.Fire(Trigger.startGame);
				}
				/*
				if(_agentPlayer.isReady()){
					print ("Agent player ready.");
					FSM.Fire(Trigger.startGame);
				}
				*/
			}
		}


		//switch cameras based on game state
		if (FSM.IsInState(PuzzleState.GAME_STEP) || FSM.IsInState(PuzzleState.GAME_END)) {
			mainCam.enabled = true;
			progressCam.enabled = false;		
		} else {
			mainCam.enabled = false;
			progressCam.enabled = true;									
		}
	}

	public static void IntroEntry()
	{
	}


	//when game finished, start next game
	public static void finishOneGame()
	{
		++curGameNum;
		if (curGameNum < totalGameNum) {
			//reset block success index on game end
			GameController.blocksuccess_index = -1;
			FSM.Fire(Trigger.startGame);
		} else {
			FSM.Fire(Trigger.endNode);	
		}
	}
}




