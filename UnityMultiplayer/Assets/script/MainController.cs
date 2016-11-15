using UnityEngine;
using System.Collections;
using System.IO;
using System;

public enum NODE{
	BLACK_NODE,
	WHITE_NODE,
}

public class MainController : MonoBehaviour {

	public static LocalPlayer _localPlayer;
	public static AgentPlayer _agentPlayer;
	public static NetworkedPlayer _networkedPlayer;
	public static TwoPlayers _twoPlayers;
	public static StateMachine FSM = new StateMachine ();
	public static bool isAgentActive;
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
		isAgentActive = true;		
		GameInfo.NodeInfoInitialization ();
		curGameNum = 0;
		curNode = NODE.BLACK_NODE;
		totalGameNum = 10;
		_localPlayer = new LocalPlayer();
		_agentPlayer = new AgentPlayer();
		_networkedPlayer = new NetworkedPlayer ();
		_twoPlayers = new TwoPlayers ();
		mainCam = GameObject.Find ("Main Camera").camera;
		progressCam = GameObject.Find ("ProgressCamera").camera;
	}

	public static void sendPlayerReady(){
		LocalPlayer.readyFlag = true;
		AgentPlayer.readyFlag = true;
		NetworkedPlayer.sendReadyFlag();
	}

	void Update(){

		if (FSM.IsInState (PuzzleState.INTRO_END)) {
			if(_localPlayer.isReady()){
				if(_agentPlayer.isReady()){
					FSM.Fire(Trigger.startGame);
				}
			}
		}

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

	public static void finishOneGame(){
		++curGameNum;
		if (curGameNum < totalGameNum) {
			FSM.Fire (Trigger.startGame);
		} else {
			FSM.Fire(Trigger.endNode);	
		}
	}
}




