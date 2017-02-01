using UnityEngine;

public class GameGUI : MonoBehaviour
{
	private GameObject instructionImage;
	private string text;
	private int assignmentI;
	private int textIndex;
	private string buttonText;
	public GUIStyle textStyle;
	public GUIStyle buttonstyle;
	private float loopCounter;
	private int oldAssignmentIndex;
	private AudioClip Recording3;
	private AudioClip Recording4;
	private AudioClip Recording5;
	private bool audioPlayFlag;
	private GUIStyle TimeBarStyle;

	void Start()
	{	
		assignmentI = 0;
		textIndex = 4;
		oldAssignmentIndex = -1;
		textStyle = new GUIStyle();
		textStyle.fontSize = 40;
		textStyle.wordWrap = true;
		textStyle.alignment = TextAnchor.UpperCenter;
		buttonstyle = new GUIStyle();
		buttonstyle.fontSize = 40;
		buttonstyle.alignment = TextAnchor.MiddleCenter;
		Recording3 = Resources.Load("Recording 3") as AudioClip;
		Recording4 = Resources.Load("Recording 4") as AudioClip;
		Recording5 = Resources.Load("Recording 5") as AudioClip;
		audioPlayFlag = false;
	}

	private Texture2D MakeTex(int width, int height, Color col)
	{
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	void OnGUI()
	{
		if (TimeBarStyle == null) {			
			TimeBarStyle = new GUIStyle(GUI.skin.box);
			TimeBarStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 0.8f));
		}


		if (MainController.FSM.IsInState(PuzzleState.INTRO_INITIALIZATION)) {
			buttonText = "START";
			if (GUI.Button(new Rect(3f * Screen.width / 7f, 4f * Screen.height / 6f, 0.5f * Screen.width / 7f, 0.5f * Screen.height / 6f), buttonText)) {
				MainController.FSM.Fire(Trigger.playIntro);
				textIndex = 0;
			}
		}

		if (MainController.FSM.IsInState(PuzzleState.INTRO_PLAY)
		   //ensure other player connected before allowing ready button to be used.
		   && GameController._server.isOtherClientConnected) {
			if (textIndex == 0) {				
				text = "Hello again and welcome to the collaborative virtual environment. " +
				"Now you will play some block puzzles with your partner. Say hello to your partner. ";
				buttonText = "Next ";
				GUI.Label(new Rect(Screen.width / 7f, Screen.height / 6f, 5f * Screen.width / 7f, 2f * Screen.height / 6f), text, textStyle);
				audioPlay(Recording3);
				if (GUI.Button(new Rect(3f * Screen.width / 7f, 4f * Screen.height / 6f, 0.5f * Screen.width / 7f, 0.5f * Screen.height / 6f), buttonText)) {
					audioStop();
					textIndex = 1;
				}				
			} else if (textIndex == 1) {			
				text = "You and your partner will see the same virtual environment from different locations.  " +
				"Both of you will be able to see the blocks moving on the screen.";
				text += "The virtual environment includes “the selection area”, “the target”, " +
				"and “the play area” as shown here on your screen.  ";
				text += "The “target” is the final pattern that you will try to create using your blocks.  " +
				"The “selection area” will hold the blocks you can use to create the pattern.  " +
				"The “play area” is where you will both drag your blocks to create the pattern. ";
				text += "You can drag any block from the “selection area” to the “play area” to create the same pattern as the target.";			
				buttonText = "Next ";
				GUI.Label(new Rect(Screen.width / 16, 2 * Screen.height / 12, 8 * Screen.width / 14, 2 * Screen.height / 6), text, textStyle);
				audioPlay(Recording4);
				if (GUI.Button(new Rect(2.25f * Screen.width / 7, 9 * Screen.height / 12, 0.5f * Screen.width / 7, 0.5f * Screen.height / 6), buttonText)) {
					audioStop();
					textIndex = 2;
				}
			} else if (textIndex == 2) {
				text = "In some tasks, you will take turns moving the blocks on your own.  " +
				"In some tasks, you and your partner will need to move the blocks together, " +
				"by dragging the same block at the same time.  In some tasks, you will move " +
				"all the blocks on your own while your partner helps you by giving color or " +
				"position hints.";
				text += "Or you may need to help your partner while he or she move the blocks.  " +
				"The blocks have colors to help you create the pattern.  S" +
				"ometimes you will be able to see the colors, but your partner will not.";
				text += "Other times, your partner will see the block colors, but you will not. " +
				"Remember to ask your partner to help you!  And to rotate a block, click the right " +
				"and left arrows on the keyboard.  ";
				buttonText = "I am ready! ";
				GUI.Label(new Rect(Screen.width / 7, Screen.height / 6, 5 * Screen.width / 7, 2 * Screen.height / 6), text, textStyle);
				audioPlay(Recording5);
				if (GUI.Button(new Rect(3 * Screen.width / 7, 4.5f * Screen.height / 6, 0.5f * Screen.width / 7, 0.5f * Screen.height / 6), buttonText)) {
					audioStop();
					textIndex = 10;
					this.SetLocalPlayerReady();
					MainController.FSM.Fire(Trigger.endIntro);
				}
			} else if (textIndex == 3) {		//for post test
				text = "This is the final one. ";
				buttonText = "I am ready! ";
				GUI.Label(new Rect(Screen.width / 7, Screen.height / 6, 5 * Screen.width / 7, 2 * Screen.height / 6), text, textStyle);
				if (GUI.Button(new Rect(3.25f * Screen.width / 7, 4 * Screen.height / 6, 0.5f * Screen.width / 7, 0.5f * Screen.height / 6), buttonText)) {
					this.SetLocalPlayerReady();
					MainController.FSM.Fire(Trigger.endIntro);
				}
			} else if (textIndex == 4) {		//for post test
				text = "More block puzzle games. ";
				buttonText = "I am ready! ";
				GUI.Label(new Rect(Screen.width / 7, Screen.height / 6, 5 * Screen.width / 7, 2 * Screen.height / 6), text, textStyle);
				if (GUI.Button(new Rect(3.25f * Screen.width / 7, 4 * Screen.height / 6, 0.5f * Screen.width / 7, 0.5f * Screen.height / 6), buttonText)) {
					this.SetLocalPlayerReady();
					MainController.FSM.Fire(Trigger.endIntro);
				}

			}
		}

		if (MainController.FSM.IsInState(PuzzleState.GAME_STEP)) {
			if (GameController.GameTimer > 0) {
				GUI.Box(new Rect(0, 100, 300, 27), "");
				GUI.Box(new Rect(4, 104, (GameController.GameTimer / GameController.gameLen * 292), 19), "", TimeBarStyle);				
			}		
		}

		if (MainController.FSM.IsInState(PuzzleState.GAME_END)) {
			buttonText = "finish";
			
			if (GUI.Button(new Rect(3.25f * Screen.width / 7, 4.5f * Screen.height / 6f, 0.5f * Screen.width / 7, 0.5f * Screen.height / 6), buttonText)) {
				
				MainController.FSM.Fire(Trigger.endGame);
			}
		}

		if (MainController.FSM.IsInState(PuzzleState.GAME_INITIALIZATION)) {
			if (MainController.curGameNum == 0) {
				text = "Game1: Move block one by one. ";
			} else if (MainController.curGameNum == 1) {
				text = "Game2: This is a turn-taking game ";
				
			} else if (MainController.curGameNum == 2) {
				text = "Game3: Two players move block together in this game. ";
				
			} else if (MainController.curGameNum == 3) {
				text = "Game4: One player moves block. The other player has color information. ";
				
			} else if (MainController.curGameNum == 4) {
				text = "Game5: One player moves block. The other player has color information.";
				
			} else if (MainController.curGameNum == 5) {
				text = "Game6: Two players need to move blocks together. ";
				
			} else if (MainController.curGameNum == 6) {
				text = "Game7: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 7) {
				text = "Game8: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 8) {
				text = "Game9: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 9) {
				text = "Game10: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 10) {
				text = "Game11: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 11) {
				text = "Game12: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 12) {
				text = "Game13: Two players need to move blocks together. ";				
			} else if (MainController.curGameNum == 13) {
				text = "Game14: Two players need to move blocks together. ";				
			}	
			GUI.Label(new Rect(Screen.width / 7, Screen.height / 6, 5 * Screen.width / 7, 2 * Screen.height / 6), text, textStyle);
		}

	}
	
	//default true value
	void SetLocalPlayerReady(bool value = true)
	{
		MainController._localPlayer.setReadyFlag(value);
	}

	
	void audioPlay(AudioClip audioC)
	{
		if (!audioPlayFlag) {
			audioPlayFlag = true;	//only play once
			audio.PlayOneShot(audioC);
		}
	}
	
	void audioStop()
	{
		audioPlayFlag = false;
		audio.Stop();
	}
}
