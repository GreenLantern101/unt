using UnityEngine;
using System.Collections;

public enum DMState
{
	DMIdle,
	DMGame,
	DMInitial, 
	DMRount,
	DMIntentionDetect,
	DMObjectDetect,
	DMIntentionConf,
	DMObjectConf,
	DMProvInfor,
	DMUnknown,
};

public enum DMTrigger
{
	MLInput, 
	ProvInfo,
	Confirm,
	Reject,
	Feedback1,
	Feedback2,
	Feedback3,
	AskIntention,
	AskObject,
	GameStart,
	GameEnd,
	Done,
};


public class DMStateMachine: Stateless.StateMachine<DMState, DMTrigger>{
	
	public DMStateMachine(): base(DMState.DMIdle)
	{
		
		Configure (DMState.DMIdle)
			.Permit (DMTrigger.GameStart, DMState.DMInitial);

		Configure (DMState.DMGame)
			.Permit (DMTrigger.GameEnd, DMState.DMIdle);

		Configure(DMState.DMInitial)
			.SubstateOf (DMState.DMGame)
			.OnEntry(()=>LanguageManager.InitialParms())
				.Permit (DMTrigger.Feedback1, DMState.DMIntentionConf)
				.Permit (DMTrigger.ProvInfo, DMState.DMProvInfor)
				.Permit (DMTrigger.MLInput, DMState.DMIntentionDetect);
		
		Configure (DMState.DMRount)
			.Permit (DMTrigger.GameEnd, DMState.DMIdle)
			.Permit (DMTrigger.Done, DMState.DMInitial);
		
		Configure (DMState.DMIntentionDetect)
			.SubstateOf(DMState.DMRount)
				.Permit (DMTrigger.ProvInfo, DMState.DMProvInfor)
				.Permit (DMTrigger.Confirm, DMState.DMObjectDetect)
				.Permit (DMTrigger.Reject, DMState.DMUnknown)
				.Permit (DMTrigger.AskIntention, DMState.DMIntentionConf);
		
		Configure (DMState.DMObjectDetect)		
			.SubstateOf (DMState.DMRount)	
				.Permit (DMTrigger.Reject, DMState.DMUnknown)
				.Permit (DMTrigger.AskIntention, DMState.DMIntentionConf)
				.Permit (DMTrigger.AskObject, DMState.DMObjectConf)
				.Permit (DMTrigger.ProvInfo, DMState.DMProvInfor);
		
		Configure (DMState.DMIntentionConf)
			.SubstateOf (DMState.DMRount)
				.OnEntry (() => LanguageManager.ReqConfIntention ())
				.Permit (DMTrigger.Feedback2, DMState.DMObjectConf)
				.Permit (DMTrigger.MLInput, DMState.DMIntentionDetect);

		Configure (DMState.DMObjectConf)
			.SubstateOf(DMState.DMRount)
				.OnEntry(()=>LanguageManager.ReqConfObj())
				.Permit (DMTrigger.Feedback3, DMState.DMInitial)
				.Permit (DMTrigger.MLInput, DMState.DMObjectDetect);
		
		Configure (DMState.DMUnknown)
			.SubstateOf(DMState.DMRount);
		
		Configure (DMState.DMProvInfor)
			.SubstateOf(DMState.DMRount)
				.Permit (DMTrigger.Done, DMState.DMInitial);
		
	}
}
