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
	private static string LogInfor = "";
	private static int TaskIndex;
	private static int StepIndex;
	private static string ActivePersonName;
	private static string ActiveBlockName;
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
	
	public static string NoneInfor = "No";
	
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
	private static System.DateTime preTaskTime;
	private static System.DateTime preStepTime;
	private static System.DateTime preMoveTime;
	private static System.DateTime preDragTime;
	private static System.DateTime preRotateTime;
	/*current information */
	private static System.TimeSpan currentTimespan;
	private static System.DateTime currentDateTime;
	private static string currentState;
	private static string preEvent;
	private static string preRotateEvent;
	public static string speakEvent;
	private static float initialAngle;
	private static float currentAngle;
	private static string additionalInfor;
	private static string preActiveBlock;
	private static string logFileName;
	private static float moveDuration;
	private static float totalSpanSeconds;
	
	void Start()
	{
		string appPath = Application.dataPath;
		appPath = appPath.LastIndexOf('/') == -1 ? appPath : appPath.Substring(0, appPath.LastIndexOf('/'));
		string timeString = System.DateTime.Now.ToString("hh-mm-ss-fff");
		logFileName = appPath + "/LoggedData/" + "Log_" + timeString + ".csv";
		
		if (!Directory.Exists(logFileName))
			Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
		string headers = "Event" + "," + "TimeStamp" + "," + "Description" + "," + "Duration" + "," + "TaskIndex" + "," + "StepIndex" + "," + "PersonIndex" + "," + "BlockIndex" + "," + "Commend";
		
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
		additionalInfor = "No";
	}
	
	public static void logGameStartParams()
	{
		StreamWriter sw = new StreamWriter(logFileName, true);
		sw.WriteLine("**************************************************************");
		System.DateTime dt = System.DateTime.Now;
		sw.WriteLine("DateTime: " + String.Format("{0:G}", dt));
		sw.WriteLine("Players: " + MainController.WhoIs(MainController.black_player)
		+ " and " + MainController.WhoIs(MainController.white_player));
		sw.WriteLine("Active player: " + MainController.WhoIs(GameController.active_player));
		//sw.WriteLine("Agent active: " + MainController.isAgentActive);
		
		
		sw.WriteLine("Assignment: " + MainController.curGameNum);
		//sw.WriteLine("Game Name: " + MainInfo.getAssignmentName());
		sw.WriteLine("Target Name: " + GameController.targetTName);
		sw.WriteLine("Nth time this game is being played: " + "<todo>");
		sw.Close();
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
			LogInfor = "";
			writeToFile();
			resetAdditionalInfor();
			return;
		}
		
		if (eventInf == repeatTaskEvent) {
			
			//TODO: fix
			LogInfor = "";
			writeToFile();
			resetAdditionalInfor();
			return;
		}
		
		//refresh active block each frame
		setActiveBlock(GameController.activePiece);
		
		
		//if finished the previous writing
		string blockName = ActiveBlockName;
		
		//refresh current date time
		currentDateTime = System.DateTime.Now;
		
		
		if (eventInf == stepSuccessEvent || eventInf == stepFailEvent || eventInf == stepStartEvent) {
			totalSpanSeconds = getTotalSecond(currentDateTime - preStepTime);
			preStepTime = currentDateTime;
			if (eventInf == stepSuccessEvent) {
				currentState = stepSuccessState;
				blockName = preActiveBlock;
				additionalInfor = moveDuration.ToString();
				resetMoveDuration();
			} else if (eventInf == stepFailEvent) {
				currentState = stepFailedState;
				blockName = preActiveBlock;
				additionalInfor = moveDuration.ToString();
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
				additionalInfor = currentAngle.ToString();		//additional angle
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
			additionalInfor = currentAngle.ToString();		//current angle
		} else if (eventInf == speakEvent) {
			additionalInfor = LanguageManager.responseText;
		}
		string text = eventInf + "," + convertToString(currentDateTime) + "," + currentState + "," + totalSpanSeconds.ToString() + "," + TaskIndex + "," + StepIndex + "," + ActivePersonName + "," + blockName + "," + additionalInfor;
		if (!isAgent) {
			LogInfor = text;
		} else {
			LogInfor = text + "," + "AGENT";
		}
		
		//	print("logInfor "+LogInfor);
		//	if(eventInf == moveStartEvent ||eventInf == moveEndEvent ){
		//		//remove the move event
		//	}else{
		writeToFile();
		//	}
		resetAdditionalInfor();
	}
	
	private static void writeToFile()
	{
		StreamWriter sw = new StreamWriter(logFileName, true);
		sw.WriteLine(LogInfor);
		sw.Close();
	}
	
	public static void setTaskIndex(int index)
	{
		TaskIndex = index;
	}
	
	public static void setStepIndex(int index)
	{
		StepIndex = index;
	}
	
	public static void setActivePerson(string name)
	{
		ActivePersonName = name;
	}
	
	public static void setActiveBlock(int active_block_index)
	{
		string name = GameInfo.blockNameStr[active_block_index];
		preActiveBlock = ActiveBlockName;
		ActiveBlockName = name;
	}
	
	private static void setNoState()
	{
		currentState = NoneInfor;
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
	
	
	private static void resetAdditionalInfor()
	{
		additionalInfor = NoneInfor;
	}
	
	private static string convertToString(System.DateTime t)
	{
		string timeString = t.ToString("hh:mm:ss:fff");
		//var timeString : String = t.Hour + ":" + t.Minute + ":" t.Second + ":"  + t.Millisecond; 
		return timeString;
	}
	
	
	private static void resetMoveDuration()
	{
		moveDuration = 0f;
	}
	
	private static float getTotalSecond(System.TimeSpan span)
	{
		return (float)span.TotalSeconds;
	}
}
