using UnityEngine;
using System.Collections.Generic;

public class MainInfo : MonoBehaviour {
	
	private static List<int> tasks_randomized = new List<int>();
	private static bool tasks_initialized = false;

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
		//taskI = MainController.curGameNum + 1;
		
		if(MainController.RANDOM_SEED==-1){
			Debug.Log("ERROR: 'MainController.RANDOM_SEED' has not been initialized.");
		}
		
		// initialize list of random tasks
		if(!MainInfo.tasks_initialized){
			//deterministic random number generator
			System.Random rand = new System.Random(MainController.RANDOM_SEED);
			tasks_randomized = new List<int>(new int[]{1,2,3,4,5,6,7,8,9});
			//log
			
			MainInfo.tasks_initialized = true;
		}
		
		//select constrained random assignment for each game
		taskI = tasks_randomized[MainController.curGameNum];
		
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
		
		//generates deterministic random number targets for each new repeat of same game
		//same for all computers
		int seed = MainController.numGamesPlayed + MainController.RANDOM_SEED;
		System.Random rng = new System.Random(seed);
		taskI = rng.Next(0, 11);
		
		TargetName = "Target" + taskI.ToString();
		return TargetName; 
	}
}
