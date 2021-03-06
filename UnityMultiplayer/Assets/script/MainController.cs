﻿using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
public enum NODE
{
	UNASSIGNED,
	WHITE_NODE,
	BLACK_NODE,
}

public class MainController : MonoBehaviour
{
	public static LocalPlayer _localPlayer{ get; private set; }
	public static AgentPlayer _agentPlayer{ get; private set; }
	public static NetworkedPlayer _networkedPlayer{ get; private set; }
	public static TwoPlayers _twoPlayers{ get; private set; }

	//initialized here before Start(), so NetworkConnection can call it remotely
	public static StateMachine FSM = new StateMachine();

	public static bool isAgentActive {
		get;
		private set;
	}

	// keep track of when need to switch to new game
	public static bool gameOdd {
		get;
		private set;
	}
	
	//number of times certain task/game was replayed
	public static int numGamesPlayed = 0;
	
	public static int RANDOM_SEED = -1;

	public static void setisAgentActive(bool val)
	{
		//find process ID value of Skype application
		//NOTE: error will be thrown if Skype is not running
		int skype_pid = Process.GetProcessesByName("Skype")[0].Id;		
		
		//mute and unmute Skype depending on whether Agent Active
		//NOTE: can't just set master volume because Agent will still talk to player	
		if(isAgentActive){
			//mute skype output on agent active
			AudioManager.SetApplicationVolume(skype_pid, 0);
		}
		else{
			//unmute skype output on agent inactive
			AudioManager.SetApplicationVolume(skype_pid, 50);
		}

		isAgentActive = val;
		//set players again each time agentActive changes.
		//setPlayer();

		//RESTART GAME
		if (!gameOdd) {
			//restart same game
			curGameNum--;
		} else {
			//restart different game (default)
		}
		gameOdd = !gameOdd;

		/*
        //reset all positions?
        _localPlayer.setPosition(Vector3.zero);
        _networkedPlayer.setPosition(Vector3.zero);
        _twoPlayers.setPosition(Vector3.zero);
        */

		//setPlayer();
		
	}
	public static void resetAllActivePieces()
	{
		_localPlayer.setActivePiece(-1);
		_networkedPlayer.setActivePiece(-1);
		_agentPlayer.setActivePiece(-1);
		_twoPlayers.setActivePiece(-1);
		
		//reset temp also
		GameController.blocksuccess_index = -1;
		
		GameController.activePieceChanged = false;
		GameController.secondaryActivePiece = -1;

	}

	//may be local player, agent player, networked player, twoPlayer
	public static IPlayerHandler black_player;
	public static IPlayerHandler white_player;

	public static int curGameNum;

	//hard-coded into config file
	public static NODE curNode = NODE.UNASSIGNED;


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
		isAgentActive = true;


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

	//needs to be called each time "isAgentActive" is switched
	public static void setPlayer()
	{
		if (isAgentActive) {
			//AI-HUMAN GAME
			/*
            if (curNode == NODE.BLACK_NODE)
            {
                black_player = _localPlayer;
                white_player = _agentPlayer;
            }
            else
            {
                black_player = _agentPlayer;
                white_player = _localPlayer;
            }
            */
			black_player = _agentPlayer;
			white_player = _localPlayer;
			print("PLAYERS SET --- local player + agent player");
		} else {
			//HUMAN-HUMAN GAME (networked)
			black_player = _networkedPlayer;
			white_player = _localPlayer;
			print("PLAYERS SET --- local player + networked player");
		}
	}


	//is called at the end of game intro by state machine
	public static void sendPlayerReady()
	{
		_localPlayer.setReadyFlag(true);
		AgentPlayer.readyFlag = true;
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
				//NOTE: when you use "else if(_agentPlayer.isReady())",
				//networked player connect will be less reliable
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
		//increment
		curGameNum++;
		//log game end
		LogTimeData.logGameEnd();
		if (curGameNum < totalGameNum) {
			//reset block success index on game end
			GameController.blocksuccess_index = -1;
			FSM.Fire(Trigger.startGame);
		} else {
			FSM.Fire(Trigger.endNode);
		}
	}
}




