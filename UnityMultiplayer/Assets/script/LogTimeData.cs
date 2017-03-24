using UnityEngine;
using System.Collections;
using System.IO;
using System;

/*
 * Log data for research purposes into /LoggedData/*
 */

public class LogTimeData : MonoBehaviour
{
	
	public static string participantName;
	private static string logPath;
	private static string LogInfo;
	private static int TaskIndex, StepIndex;
	private static string ActivePlayerName, ActiveBlockName;
	private static bool stepFlag;
	/* the event list*/
	public static string taskStartEvent = "start_new_task";
	public static string taskEndEvent = "the_end_of_task";
	public static string repeatTaskEvent = "repeat_task";
	
	public static string stepStartEvent = "start_new_step";
	public static string stepSuccessEvent = "hit_target";
	public static string stepFailEvent = "time_over";
	
	public static string dragStartEvent = "start_drag_block";
	public static string dragEndEvent = "stop_drag_block";
	public static string moveStartEvent = "start_move_block";
	public static string moveEndEvent = "stop_move_block";
	
	public static string rotateStartEvent = "start_rotate_block";
	public static string rotateEndEvent = "stop_rotate_block";
	
	public static string NoneInfo = "No";
	
	/*the state list*/
	private static string stepSuccessState = "one_success_step";
	//one step
	private static string stepFailedState = "one_failed_step";
	//one step
	private static string taskState = "time_for_current_task";
	//one task
	private static string dragState = "draging_block";
	//mouse down, block not move--unsuccess movement
	private static string moveState = "moving_block_successfully";
	//block moving
	private static string rotateState = "rotating_block";
	//block rotate
	private static string silenceState = "no_movement";
	//start of task, or end of previous mouse up, untill the new mouse down
	private static string interTaskState = "move_in_seconds";
	//the state between the two task
	private static string interStepState = "move_in_seconds";
	//not 0 for the two feedbacks
	/*record DateTime for each event*/
	private static DateTime preTaskTime, preStepTime, preMoveTime, preDragTime, preRotateTime;
	/*current Infomation */
	private static TimeSpan currentTimespan;
	private static DateTime currentDateTime;
	private static string currentState;
	private static string preEvent, preRotateEvent;
	public static string speakEvent;
	private static float initialAngle, currentAngle;
	private static string additionalInfo;
	private static string preActiveBlock;
	private static string logFileName;
	private static float moveDuration, totalSpanSeconds;
	
	void Start()
	{
		string appPath = Application.dataPath;
		appPath = appPath.LastIndexOf('/') == -1 ? appPath : appPath.Substring(0, appPath.LastIndexOf('/'));
		string timeString = DateTime.Now.ToString("hh-mm-ss-fff");
		logFileName = appPath + "/LoggedData/" + "Log_" + timeString + ".csv";
		
		if (!Directory.Exists(logFileName))
			Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
		string headers = "Event" + "," + "TimeStamp" + "," + "Description" + ","
		                 + "Duration" + "," + "TaskIndex" + "," + "StepIndex" + "," + "PersonIndex"
		                 + "," + "BlockIndex" + "," + "Command";
		
		//log column headers
		StreamWriter sw = new StreamWriter(logFileName, true);
		//column headers
		sw.WriteLine(headers);
		sw.Close();
		
		preEvent = "Nothing";
		preRotateEvent = "Nothing";
		speakEvent = "Spoken";
		currentState = "Nothing";
		StepIndex = -1;
		TaskIndex = -1;
		initialAngle = 0f;
		currentAngle = 0f;
		additionalInfo = "No";
	}
	
	public static void logParams_startGame()
	{
		StreamWriter sw = new StreamWriter(logFileName, true);
		sw.WriteLine("**************************************************************");
		DateTime dt = DateTime.Now;
		sw.WriteLine("DateTime: " + String.Format("{0:G}", dt));
		//p1 = white player, p2 = black player
		sw.WriteLine("Players: p1 -" + MainController.WhoIs(MainController.white_player)
		+ " and p2 -" + MainController.WhoIs(MainController.black_player));
		sw.WriteLine("Active player: " + MainController.WhoIs(GameController.active_player));
		//sw.WriteLine("Agent active: " + MainController.isAgentActive);
		
		
		sw.WriteLine("Assignment: " + MainController.curGameNum);
		//sw.WriteLine("Game Name: " + MainInfo.getAssignmentName());
		sw.WriteLine("Target Name: " + GameController.targetTName);
		sw.WriteLine("Nth time this game is being played: " + (MainController.numGamesPlayed + 1));
		sw.Close();
		
		setTaskIndex(MainController.curGameNum);
		setStepIndex(GameController.turnNumber);
	}
	
	
	public static void setEvent(string eventInf, bool isAgent = false)
	{
		//	print("new event: " + eventInf);
		logEvent(eventInf, isAgent);
	}
	
	
	private static void logEvent(string eventInf, bool isAgent = false)
	{
		if (eventInf == taskStartEvent || eventInf == taskEndEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preTaskTime);
			preTaskTime = currentDateTime;
			if (eventInf == taskStartEvent) {
				if (TaskIndex != 0) {
					currentState = interTaskState;
				} else {
					totalSpanSeconds = getTotalSecond(currentDateTime - currentDateTime);
					setNoState();			
				}
			} else {
				currentState = taskState;
			}
			
			//TODO: fix
			logParams_startGame();
			resetAdditionalInfo();
			return;
		}
		
		if (eventInf == repeatTaskEvent) {
			
			//TODO: fix
			logParams_startGame();
			resetAdditionalInfo();
			return;
		}
		
		//refresh active block each frame
		setActiveBlock(GameController.activePiece);
		
		
		//if finished the previous writing
		string blockName = ActiveBlockName;
		
		//refresh current date time
		currentDateTime = DateTime.Now;
		
		
		if (eventInf == stepSuccessEvent || eventInf == stepFailEvent || eventInf == stepStartEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preStepTime);
			preStepTime = currentDateTime;
			if (eventInf == stepSuccessEvent) {
				currentState = stepSuccessState;
				blockName = preActiveBlock;
				additionalInfo = moveDuration.ToString();
				resetMoveDuration();
			} else if (eventInf == stepFailEvent) {
				currentState = stepFailedState;
				blockName = preActiveBlock;
				additionalInfo = moveDuration.ToString();
				resetMoveDuration();
			} else {
				if (StepIndex != 0) {
					currentState = interStepState;
				} else {
					setNoState();
				}
			}
		} else if (eventInf == dragStartEvent || eventInf == moveStartEvent || eventInf == rotateStartEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preMoveTime);
			currentState = silenceState;
			if (eventInf == dragStartEvent) {
				preDragTime = currentDateTime;
			}
			if (eventInf == moveStartEvent) {
				preMoveTime = currentDateTime;
				preEvent = moveStartEvent;
			}
			if (eventInf == rotateStartEvent) {
				preRotateEvent = eventInf;
				preRotateTime = currentDateTime;
				additionalInfo = currentAngle.ToString();		//additional angle
			}
		} else if (eventInf == dragEndEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preDragTime);
			currentState = dragState;
			blockName = preActiveBlock;
		} else if (eventInf == moveEndEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preMoveTime);
			currentState = moveState;
			preEvent = moveEndEvent;
			blockName = preActiveBlock;
			moveDuration += totalSpanSeconds;
		} else if (eventInf == rotateEndEvent) {
			preRotateEvent = eventInf;
			totalSpanSeconds = getTotalSecond(currentDateTime - preRotateTime);
			currentState = rotateState;
			blockName = preActiveBlock;
			additionalInfo = currentAngle.ToString();		//current angle
		} else if (eventInf == speakEvent) {
			additionalInfo = LanguageManager.responseText;
		}
		string text = eventInf + "," + prettify(currentDateTime) + ","
		              + currentState + "," + totalSpanSeconds + "," + TaskIndex + ","
		              + StepIndex + "," + ActivePlayerName + "," + blockName + "," + additionalInfo;
		if (!isAgent) {
			LogInfo = text;
		} else {
			LogInfo = text + "," + "_agent";
		}
		
		//	print("logInfo "+LogInfo);
		//	if(eventInf == moveStartEvent ||eventInf == moveEndEvent ){
		//		//remove the move event
		//	}else{
		writeToFile();
		//	}
		resetAdditionalInfo();
	}
	
	private static void writeToFile()
	{
		StreamWriter sw = new StreamWriter(logFileName, true);
		sw.WriteLine(LogInfo);
		sw.Close();
	}
	
	private static void setTaskIndex(int index)
	{
		TaskIndex = index;
	}
	
	public static void setStepIndex(int index)
	{
		StepIndex = index;
	}
	
	public static void setActivePlayer(string name)
	{
		ActivePlayerName = name;
	}
	
	private static void setActiveBlock(int active_block_index)
	{
		string name = "";
		if (active_block_index > -1)
			name = GameInfo.blockNameStr[active_block_index];
		preActiveBlock = ActiveBlockName;
		ActiveBlockName = name;
	}
	
	private static void setNoState()
	{
		currentState = NoneInfo;
		currentTimespan = currentDateTime - currentDateTime;	
	}
	
	public static string getPreEvent()
	{
		return preEvent;
	}
	
	public static string getPreRotateEvent()
	{
		return preRotateEvent;
	}

	
	public static void setCurrentAngle(float inAngle)
	{
		currentAngle = inAngle;
	}
	
	
	private static void resetAdditionalInfo()
	{
		additionalInfo = NoneInfo;
	}
	
	private static string prettify(DateTime t)
	{
		return t.ToString("hh:mm:ss:fff");
	}
	
	
	private static void resetMoveDuration()
	{
		moveDuration = 0f;
	}
	
	private static float getTotalSecond(TimeSpan span)
	{
		return (float)span.TotalSeconds;
	}
}
