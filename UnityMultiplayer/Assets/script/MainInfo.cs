using UnityEngine;
using System.Collections.Generic;

public class MainInfo : MonoBehaviour
{
	
	private static List<int> tasks_randomized = new List<int>();
	private static bool tasks_initialized = false;

	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}

	public static string getAssignmentName()
	{
		string assignmentName = "";
		int taskI = 0;
		//if(MainController.curGameNum < 7){
		//	taskI = MainController.curGameNum + 1;
		//}else{
		//	taskI = 14 - MainController.curGameNum;
		//}
		//taskI = MainController.curGameNum + 1;
		
		if (MainController.RANDOM_SEED == -1) {
			Debug.Log("ERROR: 'MainController.RANDOM_SEED' has not been initialized.");
		}
		
		// initialize list of random tasks
		if (!MainInfo.tasks_initialized) {
			
			//populate list of random tasks
			FillRandomTasks();
			
			//log
			string s = "+++++++++++++++++++++ ";
			foreach (int i in tasks_randomized) {
				s += i + ", ";
			}
			Debug.Log(s);
			
			
			MainInfo.tasks_initialized = true;
		}
		
		//select constrained random assignment for each game
		taskI = tasks_randomized[MainController.curGameNum];
		
		assignmentName = "Assignment" + taskI.ToString();
		return assignmentName; 
	}

	public static string getTargetName()
	{
		string TargetName = "";
		int taskI = 0;
		/*
		if(MainController.curGameNum < 7){
			taskI = MainController.curGameNum + 1 ;
		}else{
			taskI = 14 - MainController.curGameNum;
		}
		*/
		taskI = tasks_randomized[MainController.curGameNum];
		
		TargetName = "Target" + taskI.ToString();
		return TargetName; 
	}
	/// <summary>
	/// Generates deterministic random number targets for each new repeat of same game.
	///	Same for all computers.
	/// </summary>
	private static void FillRandomTasks()
	{
		/*
			 * Algorithm: imagine all games plotted on coordinate system
			 * such that the x-axis is the communication value & y-axis is the collaboration value.
			 * 
			 * Then we seek a path through the points such that
			 * 1. each point is visited only once.
			 * 2. the path only consists of horizontal & vertical edges.
			 * 3. have a variety of forward/reverse edges (automatically satisfied b/c of #1 and #2)
			 * 
			 * Extra conditions (self-imposed to more easily generate):
			 * 1. Start in the middle
			 * 2. if there is one remaining node in row/column, must go to that node
			 *  X X X
			 *  X X X
			 *  X X X
			 */
			
			
		//deterministic random number generator
		System.Random r = new System.Random(MainController.RANDOM_SEED);
		Debug.Log("RANDOM SEED USED: " + MainController.RANDOM_SEED);
			
		//3x3 array initialized to 1 (unvisited). Note: 0 = visited
		int[,] arr = new int[3, 3];
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 3; j++) {
				arr[i, j] = 1;
			}
		}
			
		//start anywhere
		int x = r.Next(0, 2);
		int y = r.Next(0, 2);
		/* uncomment line below to start in the middle */
		//x = 1; y = 1;
		arr[x, y] = 0;
		//Note: task# = 3*x_index + y_index + 1
		tasks_randomized.Add(3 * x + y + 1);
			
		//get available points
		Coord cur = new Coord(x, y);
		
		//stores list of candidate next points in two lists
		//in same row as cur
		List<Coord> tmp_row = new List<Coord>();
		//in same column as cur
		List<Coord> tmp_column = new List<Coord>();
		
		do {
			tmp_row.Clear();
			tmp_column.Clear();
			
			//refresh all available points
			for (int i = 0; i < 3; i++) {
				for (int j = 0; j < 3; j++) {
					//check if visited
					if (arr[i, j] == 0)
						continue;
					//check if same row/column
					if (i != cur.x && j != cur.y)
						continue;
					// should never happen, b/c "visited" check should catch
					if (i == cur.x && j == cur.y) {
						Debug.Log("ERROR: repeated same game.");
						break;
					}
					
					//increment counters
					if (i == cur.x)
						tmp_column.Add(new Coord(i, j));
					if (j == cur.y)
						tmp_row.Add(new Coord(i, j));
					
				}
			}
				
			//If there is one remaining node in row/column, must go to that node
			if (tmp_row.Count == 1 && tmp_column.Count == 1) {
				//if one node in both row and column, take a pick
				if (r.Next(0, 100) < 50) {
					cur = tmp_column[0];
				} else {
					cur = tmp_row[0];
				}
			} else if (tmp_column.Count == 1) {
				cur = tmp_column[0];
			} else if (tmp_row.Count == 1) {
				cur = tmp_row[0];
			}
			//else pick a point randomly & move there
			else {
				if (tmp_row.Count > 0 && tmp_column.Count > 0) {
					if (r.Next(0, 100) < 50) {
						cur.x = tmp_row[r.Next(0, tmp_row.Count * 100 - 1) / 100].x;
					} else {
						cur.y = tmp_column[r.Next(0, tmp_column.Count * 100 - 1) / 100].y;
					}
				} else if (tmp_row.Count > 0) {
					cur.x = tmp_row[r.Next(0, tmp_row.Count * 100 - 1) / 100].x;
				} else if (tmp_column.Count > 0) {
					cur.y = tmp_column[r.Next(0, tmp_column.Count * 100 - 1) / 100].y;
				}
			}
			
			//Note: task# = 3*x_index + y_index + 1
			tasks_randomized.Add(cur.x * 3 + cur.y + 1);
			arr[cur.x, cur.y] = 0;
			
		}
		//repeat until all points visited
		while(tasks_randomized.Count < 9);
	}
}


//coordinate struct
public struct Coord
{
	public Coord(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	public int x, y;
}
