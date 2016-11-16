using UnityEngine;
using System.Collections;
using System.IO;
using System;

public enum NODE{
	BLACK_NODE,
	WHITE_NODE,
}

public class MainController : MonoBehaviour {

	// all static because GLOBAL VARS
	public static LocalPlayer _localPlayer;
	public static AgentPlayer _agentPlayer;
	public static NetworkedPlayer _networkedPlayer;
	public static TwoPlayers _twoPlayers;


	public static StateMachine FSM = new StateMachine ();
	public static bool isAgentActive;

	//may be local player, agent player, networked player, twoPlayer
	public static IPlayerHandler black_player;
	public static IPlayerHandler white_player;

	public static int curGameNum;
	public static NODE curNode;
	public static Camera mainCam ;
	public static Camera progressCam;

	
	private static int totalGameNum;

	void Start()
	{	//The node start state
		print ("FSM initilized. ");


		//sets agent to active (b/c currently player plays with agent)
		//will need to set to false for networking??
		isAgentActive = true;		


		//initializes game
		GameInfo.NodeInfoInitialization ();


		curGameNum = 0;
		curNode = NODE.BLACK_NODE;
		totalGameNum = 10;


		// currently creates one instance of each player type --> why?
		_localPlayer = new LocalPlayer();
		_agentPlayer = new AgentPlayer();
		_networkedPlayer = new NetworkedPlayer ();
		_twoPlayers = new TwoPlayers ();


		//set cameras
		mainCam = GameObject.Find ("Main Camera").camera;
		progressCam = GameObject.Find ("ProgressCamera").camera;
	}


	//is called at the end of game intro by state machine
	public static void sendPlayerReady(){
		LocalPlayer.readyFlag = true;
		AgentPlayer.readyFlag = true;
		NetworkedPlayer.sendReadyFlag();
	}

	void Update(){

		//start game at end of intro
		if (FSM.IsInState (PuzzleState.INTRO_END)) {
			if(_localPlayer.isReady()){
				if(_agentPlayer.isReady()){
					FSM.Fire(Trigger.startGame);
				}
			}
		}


		//switch cameras based on game state
		if (FSM.IsInState (PuzzleState.GAME_STEP) ||FSM.IsInState(PuzzleState.GAME_END)) {

			mainCam.enabled = true;
			progressCam.enabled = false;		
		} else {
			mainCam.enabled = false;
			progressCam.enabled = true;									
		}
	}

	public static void IntroEntry(){
	}


	//when game finished, start next game
	public static void finishOneGame(){
		++curGameNum;
		if (curGameNum < totalGameNum) {
			FSM.Fire (Trigger.startGame);
		} else {
			FSM.Fire(Trigger.endNode);	
		}
	}
}




