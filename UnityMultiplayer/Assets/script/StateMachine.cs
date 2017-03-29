using UnityEngine;
using System;
using System.Linq;
using Stateless;

//Stateless.dll explained https://blogs.msdn.microsoft.com/nblumhardt/2009/04/16/state-machines-in-domain-models/

public enum PuzzleState
{
	//The upper level states
	NODE_START,
	INTRODUCTION,
	GAME,
	NODE_SWITCHER,
	NODE_END,
	
	//The introduction substates
	INTRO_INITIALIZATION,
	INTRO_PLAY,
	INTRO_END,
	
	//The game level states
	GAME_INITIALIZATION,
	//initialize all the game parameters
	GAME_STEP,
	//one step of the game
	GAME_SWITCHER,
	GAME_END}
;

public enum Trigger
{
	//The upper level trigger
	startNode,
	endNode,
	
	//The triggers in the introduction
	startIntro,
	playIntro,
	endIntro,
	
	//The game level trigger
	startGame,
	endGame,
	
	//step level trigger
	startStep,
	endStep,
};

public class StateMachine: Stateless.StateMachine<PuzzleState, Trigger>
{

	public StateMachine()
		: base(PuzzleState.NODE_START)
	{
		//all of form State --> trigger --> another State
		
		Configure(PuzzleState.NODE_START)
			.Permit(Trigger.startIntro, PuzzleState.INTRO_PLAY);

		Configure(PuzzleState.INTRO_PLAY)
			.SubstateOf(PuzzleState.INTRODUCTION)
				.Permit(Trigger.endIntro, PuzzleState.INTRO_END);
		
		Configure(PuzzleState.INTRO_END)
			.SubstateOf(PuzzleState.INTRODUCTION)
				.Permit(Trigger.startGame, PuzzleState.GAME_INITIALIZATION)
				.OnEntry(() => MainController.sendPlayerReady());
		
		Configure(PuzzleState.GAME_INITIALIZATION)
			.SubstateOf(PuzzleState.GAME)
				.OnEntry(() => GameController.gameInitialization())
				.Permit(Trigger.startStep, PuzzleState.GAME_STEP);
		
		
		Configure(PuzzleState.GAME_STEP)
			.SubstateOf(PuzzleState.GAME)
				.OnEntry(() => GameController.stepStart())
                .Permit(Trigger.endStep, PuzzleState.GAME_SWITCHER)
                .Permit(Trigger.endGame, PuzzleState.NODE_SWITCHER);
		
		
		Configure(PuzzleState.GAME_SWITCHER)
			.SubstateOf(PuzzleState.GAME)
				.OnEntry(() => GameController.finishOneStep())
				.Permit(Trigger.startStep, PuzzleState.GAME_STEP)
				.Permit(Trigger.endGame, PuzzleState.GAME_END);

		Configure(PuzzleState.GAME_END)
			.SubstateOf(PuzzleState.GAME)
				.Permit(Trigger.endGame, PuzzleState.NODE_SWITCHER);
		
		Configure(PuzzleState.NODE_SWITCHER)
			.OnEntry(() => MainController.finishOneGame())
				.Permit(Trigger.startGame, PuzzleState.GAME_INITIALIZATION)
				.Permit(Trigger.endNode, PuzzleState.NODE_END);
		
	}
}






