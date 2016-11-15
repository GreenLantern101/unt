using UnityEngine;
using System.Collections;

public class MainInfo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static string getAssignmentName(){
		string assignmentName = "";
		int taskI = 0;
		//if(MainController.curGameNum < 7){
		//	taskI = MainController.curGameNum + 1;
		//}else{
		//	taskI = 14 - MainController.curGameNum;
		//}
		taskI = MainController.curGameNum + 1;
		assignmentName = "Assignment" + taskI.ToString();
		return assignmentName; 
	}

	public static string getTargetName(){
		string TargetName = "";
		int taskI = 0;
		if(MainController.curGameNum < 7){
			taskI = MainController.curGameNum + 1 ;
		}else{
			taskI = 14 - MainController.curGameNum;
		}
		TargetName = "Target" + taskI.ToString();
		return TargetName; 
	}
}
