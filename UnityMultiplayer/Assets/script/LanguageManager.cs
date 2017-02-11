
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Stateless;


//Detected user intention
public enum intention
{
	IntentMoveReq,
	IntentColorReq,
	IntentObjectReq,
	IntentColorCheck,
	IntentColorProv,
	IntentAnswer,
	IntentColorMoveReq,
	IntentMovePolicy,
	IntentColorPolicy,
	IntentUnknown,
	IntentNone,
	IntentObjProv,
};

//The information agent will offer
public enum Information
{
	InforSalutation,
	InforAcknowledge,
	InforMyTurn,
	InforYourTurn,
	InforBothTurn,
	InforProvColor,
	InforProvName,
	InforReqColor,
	InforBothMove,
	InforAgentMoveUserColor,
	InforUserMoveAgentColor,
	InforBothMoveAgentColor,
	InforBothMoveBothColor,
	InforBothMoveUserColor,
	InforUnknown,
	InforWithoutColor,
	InforBothColor,
	InforNone,
	InforStartMove,
	InforMovePolicy,
	InforNoMoveReason1,
	InforNoMoveReason2,
	InforWithColor,
	InforAgentStartMove,
	InforConversationPolicy,
	InforRepeatReq,
	InforSucceedProv,
};


public class LanguageManager : MonoBehaviour
{

	//give the response in string
	public static string responseText;
	///The value is a string array. Get the string arrray responseTable[key] as string[], and then get the elements
	public static Hashtable responseTable;
	//
	public static intention curIntention = intention.IntentUnknown;
	public static Information curInfor = Information.InforNone;

	private static List<int> objectList;
	private static List<string> slotList;
	private static List<string> slotValList;
	public static DMStateMachine DMFSM;
	private static Hashtable speechHT;
	private int actIdx;
	public static int repeatTime;
	//give the feedback1 if the player is silence
	public static float feedbackTimer0;
	public static float feedbackTimer1;
	public static float feedbackTimer2;
	public static float feedbackTimer3;
	public static float TimerLen1 = 12;
	public static float TimerLen2 = 8;
	public static float TimerLen0 = 2;
	public static float TimerLen3 = 8;
	public static int fd1Frequency;
	public static int curBlock;
	public static string curColor;
	public static int intDectNum;
	public static int objDectNum;
	public static int correctNum;

	void Start()
	{
		responseTable = new Hashtable();
		TextAsset sysResponseText = Resources.Load("SysResponsesNew") as TextAsset;
		readResponse(sysResponseText);
		actIdx = -1;
		feedbackTimer1 = TimerLen1;
		feedbackTimer2 = TimerLen2;				
		InitialParams();
		correctNum = 0;
		DMFSM = new DMStateMachine();
		curInfor = new Information();
		print("initialized DMFSM");
	}


	public static void ReqConfIntention()
	{
		print("current state: enter the DMIntentionConf");
		++intDectNum;
		feedbackTimer2 = TimerLen2;
		AgentPlayer.moveBlockFlag = false;
		print("current intention: " + curIntention);
		//give some information based on intention 
		if (curIntention == intention.IntentColorCheck) {			
			if (LocalPlayer.activePiece != -1 || AgentPlayer.activePiece != -1) {

				curBlock = LocalPlayer.activePiece == -1 ? AgentPlayer.activePiece : LocalPlayer.activePiece;
				string theID = GameInfo.blockNameStr[GameInfo.RandomList[curBlock]];
				responseText = getOneRes("CheckColorWithBlock", "id", theID);
			} else {
				responseText = getOneRes("CheckColor");
			}
			correctNum = 0;
			VoiceSpeaker.speakOut(responseText);
			curIntention = intention.IntentColorCheck;
		} else if (curIntention == intention.IntentMovePolicy) {
			if (LocalPlayer.activePiece != -1 || AgentPlayer.activePiece != -1) {
				responseText = getOneRes("YourTurnWithBlock");
			} else {
				responseText = getOneRes("YourTurn");
			}
			VoiceSpeaker.speakOut(responseText);
			curIntention = intention.IntentNone;
		} else if (curIntention == intention.IntentMoveReq) {
			print("intention move and active piece is: " + LocalPlayer.activePiece.ToString());
			if (LocalPlayer.activePiece != -1 || AgentPlayer.activePiece != -1) {
				curBlock = LocalPlayer.activePiece == -1 ? AgentPlayer.activePiece : LocalPlayer.activePiece;
				string theID = GameInfo.blockNameStr[GameInfo.RandomList[curBlock]];
				print("detect act " + theID);
				responseText = getOneRes("MoveReqWithBlock", "id", theID);
				correctNum = 0;
				VoiceSpeaker.speakOut(responseText);
			} else {
				responseText = "";
			}

			if (GameController.curPlayState == PlayState.BothMoveAgentColor && curBlock != -1) {
				VoiceSpeaker.speakOut(getOneRes("ProvColor", "color", GameInfo.blockColorNameList[curBlock][0]));
			}
		} else if (curIntention == intention.IntentColorProv) {
			if (LocalPlayer.activePiece != -1 || AgentPlayer.activePiece != -1) {
				curBlock = LocalPlayer.activePiece == -1 ? AgentPlayer.activePiece : LocalPlayer.activePiece;
				string theID = GameInfo.blockNameStr[GameInfo.RandomList[curBlock]];
				responseText = getOneRes("CheckColorWithBlock", "id", theID);
				correctNum = 0;
			} else {
				responseText = getOneRes("CheckColor");
				correctNum = 0;
			}
			VoiceSpeaker.speakOut(responseText);	
		} else if (curIntention == intention.IntentColorMoveReq) {
			if (curBlock == -1) {
				if (LocalPlayer.activePiece != -1 || AgentPlayer.activePiece != -1) {
					curBlock = LocalPlayer.activePiece == -1 ? AgentPlayer.activePiece : LocalPlayer.activePiece;
				} else {
					curBlock = AI.selectPiece();
				}
			}
			if (GameInfo.agentSeeColor(curBlock) == 0) {
				string theID = GameInfo.blockNameStr[GameInfo.RandomList[curBlock]];
				responseText = getOneRes("ReqColor", "id", theID);
				GameInfo.agentColorVisible[curBlock] = 1;
				correctNum = 0;
			}
			//change intention to direct
			curIntention = intention.IntentMoveReq;	
			VoiceSpeaker.speakOut(responseText);				
		} else {
			responseText = "";
		}
	}

	public static void ReqConfObj()
	{
		print("current state: DMObjectConf" + curIntention);
		++objDectNum;
		if (curIntention == intention.IntentColorCheck || curIntention == intention.IntentColorReq) {
			responseText = getOneRes("ReqObjColor");
			curIntention = intention.IntentColorReq;
		} else if (curIntention == intention.IntentMoveReq) {
			string subj = getSubject();
			responseText = getOneRes("ReqObjMove", "subject", subj);
			curIntention = intention.IntentMoveReq;
		} else {
			responseText = "";
		}
		VoiceSpeaker.speakOut(responseText);
		feedbackTimer3 = TimerLen3;
	}

	public static string getSubject()
	{
		if (GameController.curPlayState == PlayState.AgentMoveBothColor
		    || GameController.curPlayState == PlayState.AgentMoveUserColor) {

			return "me";
		}
		
		if (GameController.curPlayState == PlayState.BothMoveBothColor
		    || GameController.curPlayState == PlayState.BothMoveAgentColor
		    || GameController.curPlayState == PlayState.BothMoveUserColor) {
			return "we";
		}

		return "";
	}

	void Update()
	{
		
		if (!MainController.FSM.IsInState(PuzzleState.GAME))
			return;
		if (DMFSM.IsInState(DMState.DMInitial)) {
			//feedback 0 is only triggered once for each step
			feedbackTimer0 -= Time.deltaTime;
			if (feedbackTimer0 <= 0) {
				//offer the color and movement policy
				feedbackTimer0 = GameController.gameLen;
				getCurGamePolicy();
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			}
			feedbackTimer1 -= Time.deltaTime;
			if (feedbackTimer1 < 0) {	
				print("timer1 feedback");
				feedbackTimer1 = GameController.gameLen;
				//offer some information based on game state
				IntentionInitilize();
				DMFSM.Fire(DMTrigger.Feedback1);
				return;
			}
		}

		if (DMFSM.IsInState(DMState.DMIntentionConf)) {
			if (curIntention == intention.IntentNone) {
				DMFSM.Fire(DMTrigger.Done);
				return;
			}

			feedbackTimer2 -= Time.deltaTime;
			if (feedbackTimer2 < 0) {
				feedbackTimer2 = GameController.gameLen;
				DMFSM.Fire(DMTrigger.Feedback2);
				return;
			}
		}

		if (DMFSM.IsInState(DMState.DMProvInfor)) {
			print("current state: DMProvInfor");
			string sysResponseName = curInfor.ToString();
			sysResponseName = sysResponseName.Substring(5);
			if (sysResponseName == "MoveReq") {
				responseText = getOneRes(sysResponseName, "subject", getSubject());
			} else if (sysResponseName == "ProvColor") {
				responseText = getOneRes(sysResponseName, "color", GameInfo.blockColorNameList[curBlock][0]);
			} else if (sysResponseName == "ProvName") {					
				responseText = getOneRes(sysResponseName, "id", GameInfo.blockNameStr[GameInfo.RandomList[curBlock]]);
			} else if (sysResponseName == "StartMove") {	
				if (GameInfo.blockSucceed[curBlock]) {
					responseText = "Done";
				} else {				
					responseText = getOneRes(sysResponseName, "id", GameInfo.blockNameStr[GameInfo.RandomList[curBlock]]);
				}
			} else if (sysResponseName == "AgentStartMove") {
				if (GameInfo.blockSucceed[curBlock]) {
					responseText = "Done";
				} else {
					responseText = getOneRes(sysResponseName, "id", GameInfo.blockNameStr[GameInfo.RandomList[curBlock]]);
				}
			} else {
				responseText = getOneRes(sysResponseName);		
			}
			VoiceSpeaker.speakOut(responseText);
			DMFSM.Fire(DMTrigger.Done);
			return;
		}

		if (DMFSM.IsInState(DMState.DMIntentionDetect)) {
			print("current state: DMIntentionDetect");
			if (NotRequ("outDomain")) {
				print("This is out-domain question");
			}



			if (NotRequ("negative")) {
				curInfor = Information.InforNone;
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			} else if (NotRequ("accept")) {
				print("accept");
				IntentionConvert();
				DMFSM.Fire(DMTrigger.Confirm);
				return;
			} else if (NotRequ("reject")) {
				curBlock = -1;
				DMFSM.Fire(DMTrigger.Done);
				return;
			} else if (NotRequ("acknowledge")) {
				DMFSM.Fire(DMTrigger.Done);
				return;
			} else if (NotRequ("salutation")) {
				curInfor = Information.InforSalutation;
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			} else if (NotRequ("outDomain")) {
				curInfor = Information.InforConversationPolicy;
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			} else {
				print("act num is: " + speechHT["act"].ToString());
				if (!speechHT.ContainsKey("act")) {
					DMFSM.Fire(DMTrigger.Reject);
					return;
				} else {
					//add the threashold here
					actIdx = System.Int16.Parse(speechHT["act"].ToString());
					switch (actIdx) {
						case  0://reqColor
							curIntention = intention.IntentColorReq;
							IntentionCorrect();
							if (curIntention == intention.IntentColorPolicy) {
								DMFSM.Fire(DMTrigger.ProvInfo);
							} else {
								DMFSM.Fire(DMTrigger.Confirm);
							}
							break;
						case 1://Provide
							//step1: update the intention based on the previous intention
							IntentionConvert();
							DMFSM.Fire(DMTrigger.Confirm);
							break;
						case 2://DirectMove
							curIntention = intention.IntentMoveReq;							
							IntentionCorrect();
							if (curIntention == intention.IntentMovePolicy) {
								DMFSM.Fire(DMTrigger.ProvInfo);
							} else {
								DMFSM.Fire(DMTrigger.Confirm);
							}
							break;	
						case 3://Acknowledge
							//step1: update the intention based on the previous intention
//							curInfor = Information.InforAcknowledge;
//							DMFSM.Fire(DMTrigger.ProvInfo);
							IntentionConvert();
							DMFSM.Fire(DMTrigger.Confirm);
							break;
						case 4://object request
							curIntention = intention.IntentObjectReq;
							DMFSM.Fire(DMTrigger.Confirm);
							break;
						default:
							DMFSM.Fire(DMTrigger.Reject);
							break;
					}	
					return;
				}
			}
		}


		if (DMFSM.IsInState(DMState.DMObjectDetect)) {
			print("current state: DMObjectDetect");
			print("current intention: " + curIntention);
				
			//step1: detect the object
			int tempBlock = GameInfo.getObject(speechHT);
//				if(tempBlock == -2){
//					curInfor = Information.InforSucceedProv;
//					DMFSM.Fire(DMTrigger.ProvInfo);
//					return;
//				} else 
			if (tempBlock != -1) {
				curBlock = tempBlock;
			} 


			//update the agent see color information
			updateAgentColorVisible();
			IntentionUpdate();

				
			if (curIntention == intention.IntentNone) {
				DMFSM.Fire(DMTrigger.Done);
				return;
			}

			//step3: take action

			if ((curBlock == -1 && curIntention == intention.IntentUnknown) || (correctNum > 3)) {
				correctNum = 0;
//	//					if it is task related, ask to repeat
				print("out domain");
				print("detect color" + speechHT["color"].ToString());
				if (NotRequ("color")
				    || NotRequ("action")
				    || NotRequ("policy")
				    || NotRequ("object")
				    || NotRequ("accept")
				    || NotRequ("acknowledge")
				    || NotRequ("id")) {
					curInfor = Information.InforRepeatReq;
					DMFSM.Fire(DMTrigger.ProvInfo);
					return;
				} else {//if it is not task related, provide conversation policy
					print("here provide the conversation policy");
					curInfor = Information.InforConversationPolicy;
					DMFSM.Fire(DMTrigger.ProvInfo);
					return;
				}
				DMFSM.Fire(DMTrigger.Done);
				return;
			}
//				else if(curBlock == -1 && curIntention != intention.IntentUnknown){
//					print ("cur intention " + curIntention);
//					DMFSM.Fire(DMTrigger.Done);
////					DMFSM.Fire(DMTrigger.AskObject);
//					return;
//				}

			if (curIntention == intention.IntentColorPolicy) {
				getCurGamePolicy();
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			} else if (curIntention == intention.IntentColorReq || curIntention == intention.IntentColorProv) {
				if (curBlock == -1) {
					curIntention = intention.IntentColorReq;
					DMFSM.Fire(DMTrigger.AskObject);
					return;
				} else {
					curInfor = Information.InforProvColor;
					DMFSM.Fire(DMTrigger.ProvInfo);
					return;
				}
			} else if (curIntention == intention.IntentMoveReq) {
				//move the block;
				if (curBlock == -1) {
					print("no block is found");
					DMFSM.Fire(DMTrigger.AskObject);
					return;
				} else {
					if (GameController.active_player.getActivePiece() == curBlock) {
						DMFSM.Fire(DMTrigger.Done);
						return;
					} else {
						//						if(GameInfo.agentSeeColor(curBlock) == 0){
						//							curIntention = intention.IntentColorMoveReq;
						//							DMFSM.Fire(DMTrigger.AskIntention);
						//							return;
						//						}
						if (GameInfo.agentSeeColor(curBlock) == 1) {
							responseText = getOneRes("GuessColor", "color", GameInfo.blockColorNameList[curBlock][0]);
							VoiceSpeaker.speakOut(responseText);
						}
						MainController._agentPlayer.startMoveBlock(curBlock);
						if (GameController.curPlayState == PlayState.BothMoveBothColor) {
							curInfor = Information.InforStartMove;
						} else {
							curInfor = Information.InforAgentStartMove;
						}
						DMFSM.Fire(DMTrigger.ProvInfo);
						return;
					}
				}
			} else if (curIntention == intention.IntentMovePolicy) {
				//offer move policy
				getCurMovePolicy();
				DMFSM.Fire(DMTrigger.ProvInfo);
				return;
			} else if (curIntention == intention.IntentColorMoveReq) {
				++correctNum;
				print("increase correct num");
				DMFSM.Fire(DMTrigger.AskIntention);
				return;
			} else if (curIntention == intention.IntentObjectReq) {
				if (curBlock == -1) {
//						++correctNum;
//						print ("increase correct num");
//						detecOutDomain();
//						curInfor = Information.InforRepeatReq;
					curInfor = Information.InforNone;
					DMFSM.Fire(DMTrigger.ProvInfo);	
					return;
				} else {
					DMFSM.Fire(DMTrigger.ProvInfo);
					return;
				}
			} else {
				DMFSM.Fire(DMTrigger.Done);
				return;
			}

			//The logic is 
		}

		if (DMFSM.IsInState(DMState.DMObjectConf)) {
			feedbackTimer3 -= Time.deltaTime;
			if (feedbackTimer3 < 0) {
				feedbackTimer3 = GameController.gameLen;
				DMFSM.Fire(DMTrigger.Done);
				return;
			}
		}


		//out of domain
		if (DMFSM.IsInState(DMState.DMUnknown)) {
			print("current state: DMUnknown");		
			responseText = getOneRes("Unknown");
			VoiceSpeaker.speakOut(responseText);		
			DMFSM.Fire(DMTrigger.Done);	
			return;	
		}
		
	}

	public static void IntentionCorrect()
	{
		//provide policy if the detected intention (request informtion) is not allowed
		//second update based on the game state
		print("(IntentionCorrect) cur intention: " + curIntention);
		
		PlayState ps = GameController.curPlayState;
		
		if (curIntention == intention.IntentMoveReq) {
			if (ps == PlayState.UserMoveBothColor
			    || ps == PlayState.UserMoveAgentColor) {
				curIntention = intention.IntentMovePolicy;
				curInfor = Information.InforYourTurn;
			}
		} else if (curIntention == intention.IntentColorReq) {
			if (ps == PlayState.AgentMoveUserColor
			    || ps == PlayState.BothMoveUserColor
			    || ps == PlayState.AgentMoveBothColor
			    || ps == PlayState.BothMoveBothColor
			    || ps == PlayState.UserMoveBothColor) {
				curIntention = intention.IntentColorPolicy;
			}
		}
	}

	public static void IntentionConvert()
	{
		print("(IntentionConvert) cur intention: " + curIntention);
		if (curIntention == intention.IntentColorCheck) {
			curIntention = intention.IntentColorReq;
		} else if (curIntention == intention.IntentColorMoveReq) {
			curIntention = intention.IntentMoveReq;	
		} else {
			curIntention = intention.IntentUnknown;		
		}
	}

	public static void updateAgentColorVisible()
	{
		if (curBlock != -1 && NotRequ("color")) {
			print("agent can see1 " + curBlock + "speechHI " + speechHT["color"]);
			GameInfo.agentColorVisible[curBlock] = 2; 		
		}
	}
	
	private static void IntentionUpdate_BothMove()
	{
		SpeakCurBlock("UserMoveBlock");
		curIntention = intention.IntentMoveReq;
	}
	private static void IntentionUpdate_AgentMove()
	{
		SpeakCurBlock("AgentMoveBlock");
		curIntention = intention.IntentMoveReq;
	}
	private static void IntentionUpdate_UserMove()
	{
		SpeakCurBlock("UserMoveBlock");
		curIntention = intention.IntentNone;
	}
	//helper method for above IntentionUpdate_...() methods
	private static void SpeakCurBlock(string description){
		curBlock = AI.selectPiece();
		responseText = getOneRes(description, "ID", GameInfo.blockNameStr[GameInfo.RandomList[curBlock]]);
		VoiceSpeaker.speakOut(responseText);
	}

	public static void IntentionUpdate()
	{
		print("(IntentionUpdate) cur intention: " + curIntention);
		if (curIntention == intention.IntentObjectReq) {
			//second update based on the game state
			switch (GameController.curPlayState) {
				case PlayState.AgentMoveBothColor:
				//can be moved
					IntentionUpdate_AgentMove();
					break;
				case PlayState.AgentMoveUserColor:
					if (NotRequ("color")) {
						curInfor = Information.InforProvName;
					} else if (NotRequ("action")) {
						IntentionUpdate_AgentMove();
					} else {
						curIntention = intention.IntentUnknown;
					}
					break;
				case PlayState.UserMoveBothColor:
					IntentionUpdate_UserMove();
					break;
				case PlayState.UserMoveAgentColor:
					if (NotRequ("color")) {
						curInfor = Information.InforProvName;
					} else if (NotRequ("action")) {
						
					} else {
						curIntention = intention.IntentUnknown;
					}
					break;
				case PlayState.BothMoveAgentColor:
					if (NotRequ("color")) {
						curInfor = Information.InforProvName;
					} else if (NotRequ("action")) {
						IntentionUpdate_BothMove();
					} else {
						curIntention = intention.IntentUnknown;
					}
					break;
				case PlayState.BothMoveBothColor:				
					IntentionUpdate_BothMove();
					break;
				case PlayState.BothMoveUserColor:
					IntentionUpdate_BothMove();
					break;
				default:
					curIntention = intention.IntentUnknown;
					break;
			}		
		}
		if (curIntention == intention.IntentUnknown) {
//			print ("increase correct number ");
			//second update based on the game state
			switch (GameController.curPlayState) {
				case PlayState.AgentMoveBothColor:
				//can be moved
					curIntention = intention.IntentMoveReq;
					break;
				case PlayState.AgentMoveUserColor:
				//can be moved 
					curIntention = intention.IntentMoveReq;
					break;
				case PlayState.UserMoveBothColor:
					curIntention = intention.IntentMovePolicy;
					break;
				case PlayState.UserMoveAgentColor:
					curIntention = intention.IntentColorReq;
					break;
				case PlayState.BothMoveAgentColor:
						//need to check the logic
					if (speechHT["policy"].ToString() == "color") {
						curIntention = intention.IntentColorReq;
					} else {
						curIntention = intention.IntentMoveReq;
					}
					break;
				case PlayState.BothMoveBothColor:
					curIntention = intention.IntentMoveReq;
					break;
				case PlayState.BothMoveUserColor:
					curIntention = intention.IntentMoveReq;
					break;
				default:
					curIntention = intention.IntentUnknown;
					break;
			}
		} 
		if ((curIntention == intention.IntentColorMoveReq || curIntention == intention.IntentMoveReq)
		    && (GameController.curPlayState == PlayState.AgentMoveUserColor || GameController.curPlayState == PlayState.BothMoveUserColor)
		    && curBlock != -1
		    && GameInfo.agentSeeColor(curBlock) == 0) {
			print("agen can not see color " + curBlock);
			curIntention = intention.IntentColorMoveReq;
		}

		if (curIntention == intention.IntentColorMoveReq
		    && (GameController.curPlayState == PlayState.AgentMoveUserColor || GameController.curPlayState == PlayState.BothMoveUserColor)
		    && curBlock != -1
		    && GameInfo.agentSeeColor(curBlock) != 0) {
			curIntention = intention.IntentMoveReq;
		}
	}

	public void getCurMovePolicy()
	{
		if (GameController.curPlayState == PlayState.AgentMoveBothColor
		    || GameController.curPlayState == PlayState.AgentMoveUserColor) {
			curInfor = Information.InforMyTurn;
		} else if (GameController.curPlayState == PlayState.UserMoveBothColor
		           || GameController.curPlayState == PlayState.UserMoveAgentColor) {
			curInfor = Information.InforYourTurn;				
		} else {
			curInfor = Information.InforBothTurn;
		}
	}

	public void getCurGamePolicy()
	{
		print("(getCurGamePolicy)current play state is: " + GameController.curPlayState);
		switch (GameController.curPlayState) {
			case PlayState.AgentMoveBothColor:
				curInfor = Information.InforMyTurn;
				break;
			case PlayState.AgentMoveUserColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforAgentMoveUserColor;
				else
					curInfor = Information.InforNone;
				break;
			case PlayState.UserMoveBothColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforWithColor;
				else
					curInfor = Information.InforYourTurn;
				break;
			case PlayState.UserMoveAgentColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforUserMoveAgentColor;
				else
					curInfor = Information.InforNone;
				break;
			case PlayState.BothMoveAgentColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforBothMoveAgentColor;
				else
					curInfor = Information.InforNone;
				break;
			case PlayState.BothMoveBothColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforBothMoveBothColor;
				else
					curInfor = Information.InforNone;
				break;
			case PlayState.BothMoveUserColor:
				if (GameController.turnNumber == 0)
					curInfor = Information.InforBothMoveUserColor;
				else
					curInfor = Information.InforNone;
				break;
		}
	}

	public string replaceInfo(string inStr, string newInfo)
	{
		int slotStart = inStr.IndexOf('$');
		int slotEnd = inStr.IndexOf('#');
		string theSlot = inStr.Substring(slotStart, slotEnd - slotStart + 1);
		inStr = responseText.Replace(theSlot, newInfo);			
		return inStr;
	}

	public static void InitialParams()
	{

		speechHT = new Hashtable();
		objectList = new List<int>();
		slotList = new List<string>();
		slotValList = new List<string>();
		repeatTime = 0;
		curBlock = -1;
		curInfor = Information.InforNone;
		curIntention = intention.IntentUnknown;
		feedbackTimer1 = TimerLen1;
		if (GameController.GameTimer < GameController.gameLen - TimerLen1) {
			feedbackTimer1 += 10;		
		}
		intDectNum = 0;
		objDectNum = 0;
		curColor = "";

		print("LanguageManager: Initialized parameters.");
	}

	void readResponse(TextAsset sysResContent)
	{
		string[] lines = sysResContent.text.Split('\n');
		for (int i = 0; i < lines.Length; ++i) {
			string[] temp1 = lines[i].Split(':');
			if (temp1.Length < 2)
				continue;
			string key = temp1[0];
			string temp2 = temp1[1];
			string[] values = temp2.Split('|');
			responseTable.Add(key, values);
		}
	}

	public static void takeAction(Hashtable _speechHT)
	{
		speechHT = _speechHT;
		if (!DMFSM.IsInState(DMState.DMIdle)) {
			DMFSM.Fire(DMTrigger.MLInput);
		}
	}


	public static void IntentionInitilize()
	{
		print("(IntentionInitilize)current play state is: " + GameController.curPlayState.ToString());		
		switch (GameController.curPlayState) {
			case PlayState.AgentMoveBothColor:
			//can be moved
				curIntention = intention.IntentMoveReq;
				break;
			case PlayState.AgentMoveUserColor:
			//can be moved 
				curIntention = intention.IntentColorMoveReq;
				break;
			case PlayState.UserMoveBothColor:
				curIntention = intention.IntentMovePolicy;
				break;
			case PlayState.UserMoveAgentColor:
				curIntention = intention.IntentColorProv;
				break;
			case PlayState.BothMoveAgentColor:
				curIntention = intention.IntentMoveReq;
				break;
			case PlayState.BothMoveBothColor:
				curIntention = intention.IntentMoveReq;
				break;
			case PlayState.BothMoveUserColor:
				curIntention = intention.IntentColorMoveReq;
				break;
			default:
				curIntention = intention.IntentUnknown;
				break;
		}
	}

	public static string getOneRes(string senKey)
	{
		string[] responseStrs = responseTable[senKey] as String[];
		int index = UnityEngine.Random.Range(0, responseStrs.Count());
		string responseStr = responseStrs[index];
		return responseStr;
	}

	public static string getOneRes(string senKey, string key, string value)
	{
		string responseStr = getOneRes(senKey);
		int slotStart = responseStr.IndexOf('$');
		int slotEnd = responseStr.IndexOf('#');
		if (slotStart >= 0) {
			string theSlot = responseStr.Substring(slotStart, slotEnd - slotStart + 1);
			responseStr = responseStr.Replace(theSlot, value);	
		} 
		return responseStr;
	}
	
	/// <summary>
	/// Looks up hashtable entry for given string, returns true if != "Requ"
	/// </summary>
	/// <param name="lookup_string">Lookup string</param>
	/// <returns></returns>
	private static bool NotRequ(string lookup_string)
	{
		return (speechHT[lookup_string] != "Requ");
	}
}
